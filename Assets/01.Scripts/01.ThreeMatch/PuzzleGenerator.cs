using _01.Scripts._01.ThreeMatch.Obstacle;
using _01.Scripts._04.UI.InGame;
using _01.Scripts._11.HabitatMode;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

namespace _01.Scripts._01.ThreeMatch
{
    public enum PuzzleType
    {
        Normal,
        Special,
        Obstacle,
    }

    public enum SpecialPuzzleType
    {
        CircleBomb,
        CrossBomb,
        RowBomb,
        ColumnBomb,
        ColorBomb,
    }

    public enum ObstaclePuzzleType
    {
        DeActivated,
        Fixed,
        ForcedRowColumn,
        ChangingHabitat,
        LockedTwice,
        Portal,
        Infection,
    }
    
    public class PuzzleGenerator : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] private RectTransform puzzleFrame;
        [SerializeField] private GameObject downParticleFrame;
        [SerializeField] private GameObject particleFrame;
        [SerializeField] private GoldUI goldUI;
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private float space;
        [SerializeField] private float rowDropDelay = 0.01f;
        [SerializeField] private float columnDropDelay = 0.02f;
        [SerializeField] private float dropSpeed = 10f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float tileScale = 0.6f;
        [SerializeField] private List<ObstacleSpawnGroup> startObstacles;
        [SerializeField] private List<ObstacleWeight> obstacleWeights;
        [SerializeField] private float obstacleSpawnDelay = 2f;
        public float obstacleSpawnInterval = 10f;
        [Range(0, 100)] [SerializeField] private float goldTileSpawnRate = 5f;
        [Range(0, 100)] [SerializeField] private float swapTileSpawnRate = 5f;
        
        [Header("Puzzle Prefabs")]
        [SerializeField] private GameObject[] normalPuzzlePrefabs;
        [SerializeField] private GameObject jokerPuzzlePrefab;
        [SerializeField] private GameObject[] specialPuzzlePrefabs;
        [SerializeField] private GameObject[] obstaclePuzzlePrefabs;
        public Sprite[] normalPuzzleImages;
        [SerializeField] private GameObject[] specialPuzzleParticlePrefabs;
        [SerializeField] private GameObject obstacleWarningPrefab;
        [SerializeField] private GameObject goldPrefab;
        [SerializeField] private GameObject swapPrefab;
        [SerializeField] private GameObject[] forcedRowColumnDirectionPrefabs;
        
        [Header("Spawn Settings")] 
        [SerializeField] private SpawnStackManager spawnStackManager;

        [Header("Puzzle Reset Setting")] 
        [SerializeField] private bool isDebug = false;
        [SerializeField] private GameObject resetRectanglePrefab;
        [SerializeField] private float resetEffectDuration = 0.8f;
        [SerializeField] private Vector2 resetScaleRange = new Vector2(1f, 1.3f);
        [SerializeField] private Vector2 resetMoveDistanceRange = new Vector2(0.1f, 0.3f);
        [SerializeField, Range(0f, 1f)] private float resetStayChance = 0.25f;
        [SerializeField] private Image resetPopUpImage;
        [SerializeField] private TextMeshProUGUI resetPopUpText;
        [SerializeField] private float blinkSpeed = 2.0f; // 깜빡임 속도
        [SerializeField] private GameObject resetSpawnEffectPrefab;
        [SerializeField] private float resetSpawnEffectDestroyDelay = 1.5f;
        
        private PuzzleObject[,] _puzzles;

        private bool _isProcessing;
        public bool IsProcessing => _isProcessing;
        private int _swapCount;
        public int maxSwapCount = -1;
        private Habitat? _lastMovedHabitat;
        public bool isContinuousHabitatBanned;
        private bool isReset;
        private const string DesertCoverName = "DesertPuzzleCover";
        private Coroutine _desertCoverCoroutine;
        private GameObject _desertSandStorm;
        private ParticleSystem _desertSandParticle;

        public Action OnComboInitialized;
        public Action OnComboDetected;
        public Action<int> OnSwapCountChanged;
        
        private List<MatchGroup> _currentMatchGroups = new();
        private List<PuzzleObject> _bannedPuzzles = new();
        private Queue<Func<IEnumerator>> _taskQueue = new();
        private HashSet<Vector2Int> _movedPositions = new();
        private HashSet<Vector2Int> _swappedPositions = new();
        private List<PortalPuzzleObject> _portals = new();
        
        private List<GameObject> resetRectangles = new();

        private class MatchGroup
        {
            public List<Vector2Int> positions = new();
            public Vector2Int spawnPos;  
            public SpecialPuzzleType? resultType = null;
            public Habitat habitat;
        }

        [Serializable]
        private struct ObstacleSpawnGroup
        {
            public List<ObstacleSpawnData> obstacles;
        }

        [Serializable]
        private struct ObstacleSpawnData
        {
            public int type;
            public Vector2Int pos;
        }

        [Serializable]
        private struct ObstacleWeight
        {
            public ObstaclePuzzleType type;
            [Range(0, 100)] public int weight;
        }
        
        private enum DropSegmentType
        {
            Fall,
            Teleport
        }

        private class DropSegment
        {
            public DropSegmentType type;
            public Vector3 start;
            public Vector3 end;
        }

        private class DropPath
        {
            public List<DropSegment> segments = new();
        }
        
        private void Start()
        {
            AddTask(GenerateBoard);
            StartCoroutine(SpawnObstaclePuzzle());
        }
        
        /// <summary>
        /// 작업 큐 함수
        /// </summary>
        #region Task Queue
        public void AddTask(Func<IEnumerator> task)
        {
            _taskQueue.Enqueue(task);
            if (!_isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        private IEnumerator ProcessQueue()
        {
            try
            {
                _isProcessing = true;

                while (_taskQueue.Count > 0)
                {
                    while (Time.timeScale == 0f)
                    {
                        yield return new WaitForSecondsRealtime(0.05f);
                    }
                    
                    Func<IEnumerator> task = _taskQueue.Dequeue();
                    yield return StartCoroutine(task());
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }
        #endregion
        
        /// <summary>
        /// 시작 퍼즐 및 타입 검사 관련 함수 (시작 시 매치가 안 일어나게 설정)
        /// </summary>
        #region Start Puzzle
        public IEnumerator GenerateBoard()
        {
            _puzzles = new PuzzleObject[x, y];
            
            yield return SetStartPuzzle();
            
            if (isReset)
            {
                PlayResetSpawnEffects();
            }

            isReset = false;
        }

        public IEnumerator ResetBoard()
        {
            yield return ResetPopUpEvent();
            
            for (int i = 0;i < x;i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null)
                    {
                        Destroy(_puzzles[i, j].gameObject);
                    }
                }
            }

            yield return SpawnResetRectangles();
        }
        
        private IEnumerator SetStartPuzzle()
        {
            Sequence seq = DOTween.Sequence();
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    GameObject puzzle = SetStartRandomPuzzle(i, j);
                    PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                    po.puzzleState = PuzzleState.Falling;
                    puzzle.name = $"Puzzle({i + 1},{j + 1})";
                    _puzzles[i, j] = po;
                    
                    Vector3 targetPos = CalculatePos(i, j);
                    
                    if (isReset)
                    {
                        po.transform.localPosition = targetPos;
                        po.puzzleState = PuzzleState.Idle;
                        po.Init(this, i, j);
                        continue;
                    }
                    
                    float distance = Vector3.Distance(po.transform.localPosition, targetPos);
                    float duration = distance / dropSpeed;
                    float startAt = columnDropDelay * i + rowDropDelay * j;
                    
                    Tween fallTween = po.transform.DOLocalMove(targetPos, duration)
                        .SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            po.transform.DOPunchPosition(Vector3.down * 0.05f, 0.15f, 8)
                                .OnComplete(() =>
                                {
                                    po.puzzleState = PuzzleState.Idle;
                                });
                        });

                    seq.Insert(startAt, fallTween);
                    
                    po.Init(this, i, j);
                }
            }

            yield return seq.WaitForCompletion();

            for (int i = 0; i < _portals.Count; i += 2)
            {
                _portals[i].linkedPortal = _portals[i + 1];
                _portals[i + 1].linkedPortal = _portals[i];
            }
        }

        private GameObject SetStartRandomPuzzle(int col, int row)
        {
            bool isObstacle = false;
            ObstaclePuzzleType obstacleType = ObstaclePuzzleType.DeActivated;

            int currentStage = HabitatModeManager.Instance && HabitatModeManager.Instance.IsHabitatBattle
                ? StageManager.Instance.CurrentHabitatStage + 50 : StageManager.Instance.CurrentStage;

            var obstacleSpawnGroups = currentStage >= 100 ? startObstacles[1] : startObstacles[0];
            
            foreach (var data in obstacleSpawnGroups.obstacles)
            {
                if (data.pos.x == col && data.pos.y == row)
                {
                    isObstacle = true;
                    switch (data.type)
                    {
                        case 1:
                            obstacleType = ObstaclePuzzleType.Fixed;
                            break;
                        case 2:
                            obstacleType = ObstaclePuzzleType.DeActivated;
                            break;
                        case 3:
                            obstacleType = GetRandomObstacleType();
                            break;
                        case 4:
                            obstacleType = ObstaclePuzzleType.Portal;
                            break;
                    }
                }
            }

            GameObject puzzle;
            if (isObstacle)
            {
                puzzle = Instantiate(obstaclePuzzlePrefabs[(int)obstacleType], puzzleFrame);
                puzzle.transform.localPosition = CalculateDropPos(col, row);
                PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                
                if (po is PortalPuzzleObject pp)
                {
                    _portals.Add(pp);
                }
                
                Habitat randomType = GetValidRandomType(col, row);
                Material material = new(normalPuzzlePrefabs[(int)randomType].GetComponent<Image>().material);
                
                switch (po)
                {
                    case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.DeActivated } op:
                        op.habitat = randomType;
                        break;
                    case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op:
                        op.habitat = randomType;
                        op.GetComponent<Image>().sprite = normalPuzzleImages[(int)randomType];
                        op.SetMaterial(material);
                        break;
                    case ForcedRowColumnPuzzleObject frc:
                        frc.habitat = randomType;
                        frc.forcedDirection = Random.Range(0, 2) == 0 ? ForcedDirection.ForcedRow : ForcedDirection.ForcedColumn;
                        frc.GetComponent<Image>().sprite = normalPuzzleImages[(int)randomType];
                        frc.SetMaterial(material);
                    
                        GameObject fp = Instantiate(forcedRowColumnDirectionPrefabs[(int)frc.forcedDirection], po.transform);
                        fp.transform.localPosition = new Vector3(0.2f, -0.2f, 0);
                        break;
                    case ChangingHabitatPuzzleObject ch:
                        ch.InitialSetting(this, normalPuzzlePrefabs);
                        break;
                    case LockedTwicePuzzleObject lt:
                        lt.habitat = randomType;
                        lt.GetComponent<Image>().sprite = normalPuzzleImages[(int)randomType];
                        lt.SetMaterial(material);
                        break;
                    case InfectionPuzzleObject ip:
                        ip.Init(this);
                        break;
                }
            }
            else
            {
                Habitat randomType = GetValidRandomType(col, row);
                puzzle = Instantiate(normalPuzzlePrefabs[(int)randomType], puzzleFrame);
                puzzle.transform.localPosition = CalculateDropPos(col, row);
                
                float prob = Random.Range(0, 100f);
                
                if (prob < goldTileSpawnRate)
                {
                    SetGoldTile(puzzle);
                }
                else if (prob >= goldTileSpawnRate && prob < goldTileSpawnRate + swapTileSpawnRate)
                {
                    if (maxSwapCount != -1)
                    {
                        SetSwapTile(puzzle);   
                    }
                }
            }
            
            return puzzle;
        }
        
        private ObstaclePuzzleType GetRandomObstacleType()
        {
            if (Enum.GetValues(typeof(ObstaclePuzzleType)).Length <= 2)
            {
                return (ObstaclePuzzleType)Random.Range(0, Enum.GetValues(typeof(ObstaclePuzzleType)).Length);
            }
            
            int[] weights = BalanceFormula.InitialObstacleWeights;

            int totalWeight = 0;
            for (int i = 2;
                 i <= Enum.GetValues(typeof(ObstaclePuzzleType)).Length - 1;
                 i++)
            {
                if (i == 5)
                {
                    continue;
                }

                totalWeight += weights[i];
            }

            int random = Random.Range(0, totalWeight);

            for (int i = 2;
                 i <= Enum.GetValues(typeof(ObstaclePuzzleType)).Length - 1;
                 i++)
            {
                if (i == 5)
                {
                    continue;
                }

                if (random < weights[i])
                {
                    return (ObstaclePuzzleType)i;
                }

                random -= weights[i];
            }

            return (ObstaclePuzzleType)3;
        }

        private GameObject SetRandomPuzzle(int col, int row, int spawnOrder)
        {
            var types = Enum.GetValues(typeof(Habitat));
            var randomType = (Habitat)types.GetValue(Random.Range(0, types.Length));
            
            Vector3 startPos = CalculateDropPos(col, spawnOrder);
            GameObject puzzle = Instantiate(normalPuzzlePrefabs[(int)randomType], puzzleFrame);
            puzzle.transform.localPosition = startPos;
            puzzle.name = $"Puzzle({col + 1}, {row + 1})"; 
            
            float prob = Random.Range(0, 100f);
            
            if (prob < goldTileSpawnRate)
            {
                SetGoldTile(puzzle);
            }
            else if (prob >= goldTileSpawnRate && prob < goldTileSpawnRate + swapTileSpawnRate)
            {
                if (maxSwapCount != -1)
                {
                    SetSwapTile(puzzle);   
                }
            }
            
            return puzzle;
        }

        private Habitat GetValidRandomType(int curX, int curY)
        {
            var types = Enum.GetValues(typeof(Habitat));
            var randomType = (Habitat)types.GetValue(Random.Range(0, types.Length));

            while (IsStartingMatch(curX, curY, randomType))
            {
                randomType = (Habitat)types.GetValue(Random.Range(0, types.Length));
            }

            return randomType;
        }

        private bool IsStartingMatch(int curX, int curY, Habitat type)
        {
            if (curX > 1)
            {
                if (CheckNormalType(_puzzles[curX - 1, curY], type) && 
                    CheckNormalType(_puzzles[curX - 2, curY], type))
                {
                    return true;
                }
            }
            
            if (curY > 1)
            {
                if (CheckNormalType(_puzzles[curX, curY - 1], type) && 
                    CheckNormalType(_puzzles[curX, curY - 2], type))
                {
                    return true;
                }
            }

            return false;
        }
        
        private bool CheckType(PuzzleObject p1, PuzzleObject p2)
        {
            if (p1 == null || p2 == null)
            {
                return false;
            }

            if (p1 is JokerPuzzleObject || p2 is JokerPuzzleObject)
            {
                return true;
            }
            
            // normal <-> normal
            if (p1.puzzleType  == PuzzleType.Normal && p2.puzzleType == PuzzleType.Normal)
            {
                if (p1.GetPuzzleSubType() == p2.GetPuzzleSubType())
                {
                    return true;
                }
            }

            // (obstacle or normal) <-> (obstacle or normal)
            int t1 = -1, t2 = -1;
            
            if (p1 is ObstaclePuzzleObject { isMatchable : true } op1)
            {
                t1 = (int)op1.habitat;
            }
            else if (p1.puzzleType == PuzzleType.Normal)
            {
                t1 = p1.GetPuzzleSubType();
            }

            if (p2 is ObstaclePuzzleObject { isMatchable : true } op2)
            {
                t2 = (int)op2.habitat;
            }
            else if (p2.puzzleType == PuzzleType.Normal)
            {
                t2 = p2.GetPuzzleSubType();
            }

            if (t1 != -1 && t2 != -1 && t1 == t2)
            {
                return true;
            }
            
            return false;
        }

        private bool CheckNormalType(PuzzleObject p, Habitat type)
        {
            if (p == null)
            {
                return false;
            }

            if (p is JokerPuzzleObject)
            {
                return true;
            }
            
            if (p.puzzleType == PuzzleType.Normal)
            {
                if ((Habitat)p.GetPuzzleSubType() == type)
                {
                    return true;
                }
            }
            else if (p is ObstaclePuzzleObject { isMatchable : true } op)
            {
                if (op.habitat == type)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        
        /// <summary>
        /// 실제 퍼즐 위치, 떨어지기 전의 퍼즐 위치 계산
        /// </summary>
        #region Puzzle Position
        private Vector3 CalculatePos(int col, int row)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;
            
            return new Vector3(col * space - offsetX, row * space - offsetY, 0f);
        }

        private Vector3 CalculateDropPos(int col, int spawnOrder)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;
            
            float spawnY = (y + spawnOrder) * space - offsetY;

            return new Vector3(col * space - offsetX, spawnY, 0f);
        }
        #endregion

        /// <summary>
        /// 매치 불가 시, 리셋 관련 함수
        /// 1. 리셋 알림 UI
        /// 2. 반짝이는 네모 생성
        /// 3. 퍼즐 생성 파티클
        /// </summary>
        /// <returns></returns>

        #region Reset
        //디버그용 체크
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                isDebug = true;
                StartCoroutine(ResetBoard());
            }
        }
        
        private IEnumerator ResetPopUpEvent()
        {
            isReset = true;
            
            if (resetPopUpImage == null)
            {
                yield break;
            }

            Color originImageColor = resetPopUpImage.color;
            Color originTextColor = resetPopUpText != null
                ? resetPopUpText.color
                : Color.white;

            resetPopUpImage.gameObject.SetActive(true);

            float duration = 2f;
            int repeatCount = 2;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float normalizedTime = Mathf.Clamp01(timer / duration);

                float wave = Mathf.PingPong(normalizedTime * repeatCount * 2f, 1f);
                float alpha = wave;

                Color imageColor = originImageColor;
                imageColor.a = alpha;
                resetPopUpImage.color = imageColor;

                if (resetPopUpText != null)
                {
                    Color textColor = originTextColor;
                    textColor.a = alpha;
                    resetPopUpText.color = textColor;
                }

                yield return null;
            }

            Color finalImageColor = originImageColor;
            finalImageColor.a = 0f;
            resetPopUpImage.color = finalImageColor;

            if (resetPopUpText != null)
            {
                Color finalTextColor = originTextColor;
                finalTextColor.a = 0f;
                resetPopUpText.color = finalTextColor;
            }

            resetPopUpImage.gameObject.SetActive(false);

            resetPopUpImage.color = originImageColor;

            if (resetPopUpText != null)
            {
                resetPopUpText.color = originTextColor;
            }
        }

        private IEnumerator SpawnResetRectangles()
        {
            if (resetRectanglePrefab == null)
            {
                yield return null;
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    GameObject rect = Instantiate(resetRectanglePrefab, puzzleFrame);

                    rect.name = $"ResetRectangle({i + 1},{j + 1})";
                    rect.transform.localPosition = CalculatePos(i, j);
                    
                    resetRectangles.Add(rect);
                }
            }
            
            float timer = 0f;

            Vector3[] startPositions = new Vector3[resetRectangles.Count];
            Vector3[] targetPositions = new Vector3[resetRectangles.Count];
            Vector3[] startScales = new Vector3[resetRectangles.Count];
            Vector3[] targetScales = new Vector3[resetRectangles.Count];

            for (int i = 0; i < resetRectangles.Count; i++)
            {
                GameObject rect = resetRectangles[i];

                startPositions[i] = rect.transform.localPosition;
                startScales[i] = rect.transform.localScale;

                float targetScale = Random.Range(resetScaleRange.x, resetScaleRange.y);
                targetScales[i] = startScales[i] * targetScale;

                bool stay = Random.value < resetStayChance;

                if (stay)
                {
                    targetPositions[i] = startPositions[i];
                }
                else
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                    float distance = Random.Range(resetMoveDistanceRange.x, resetMoveDistanceRange.y);
                    targetPositions[i] = startPositions[i] + direction * distance;
                }
            }

            while (timer < resetEffectDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / resetEffectDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                for (int i = 0; i < resetRectangles.Count; i++)
                {
                    GameObject rect = resetRectangles[i];

                    if (rect == null)
                    {
                        continue;
                    }

                    rect.transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                    rect.transform.localScale = Vector3.Lerp(startScales[i], targetScales[i], t);
                }

                yield return null;
            }
            
            ClearResetRectangles();

            yield return null;
        }
        
        private void ClearResetRectangles()
        {
            foreach (GameObject rect in resetRectangles)
            {
                if (rect != null)
                {
                    Destroy(rect);
                }
            }

            resetRectangles.Clear();

            if (isDebug)
            {
                isDebug = false;
                StartCoroutine(GenerateBoard());
            }
        }
        
        private void PlayResetSpawnEffects()
        {
            if (resetSpawnEffectPrefab == null)
            {
                return;
            }

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    GameObject effect = Instantiate(resetSpawnEffectPrefab, puzzleFrame);
                    effect.transform.localPosition = CalculatePos(i, j); 

                    ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (ParticleSystem particle in particles)
                    {
                        if (particle == null)
                        {
                            continue;
                        }

                        particle.Play(true);
                    }

                    Destroy(effect, resetSpawnEffectDestroyDelay);
                }
            }
        }
        #endregion
        
        /// <summary>
        /// 집을 지켜라 사막모드 퍼즐 가리개
        /// </summary>
        #region PuzzleHide
        public IEnumerator StartDesertPuzzleCover(GameObject coverPrefab, GameObject sandStormPrefab, GameObject sandParticlePrefab, 
            float duration, float refreshInterval = 0.1f)
        {
            if (coverPrefab == null)
            {
                yield break;
            }

            if (_desertCoverCoroutine != null)
            {
                StopCoroutine(_desertCoverCoroutine);
                yield return ClearDesertPuzzleCovers();
            }

            _desertCoverCoroutine = StartCoroutine(DesertPuzzleCoverRoutine(coverPrefab, sandStormPrefab, sandParticlePrefab, duration, refreshInterval));

            yield return _desertCoverCoroutine;
        }
        
        private IEnumerator DesertPuzzleCoverRoutine(GameObject coverPrefab, GameObject sandStormPrefab, GameObject sandParticlePrefab,
            float duration, float refreshInterval)
        {
            _desertSandParticle = Instantiate(sandParticlePrefab, downParticleFrame.transform).GetComponent<ParticleSystem>();
            _desertSandParticle.Play();
            
            _desertSandStorm = Instantiate(sandStormPrefab, downParticleFrame.transform);
            Material mat = _desertSandStorm.GetComponent<SpriteRenderer>().material;
            mat.DOFloat(1, "_Appear", 1f).WaitForCompletion();
            
            float timer = 0f;

            while (timer < duration)
            {
                StartCoroutine(AttachDesertCoversToCurrentPuzzles(coverPrefab));

                timer += refreshInterval;
                yield return new WaitForSeconds(refreshInterval);
            }

            StartCoroutine(ClearDesertPuzzleCovers());
            _desertCoverCoroutine = null;
        }
        
        private IEnumerator AttachDesertCoversToCurrentPuzzles(GameObject coverPrefab)
        {
            if (_puzzles == null)
            {
                yield break;
            }

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    PuzzleObject puzzle = _puzzles[i, j];

                    if (puzzle == null)
                    {
                        continue;
                    }

                    if (puzzle.transform.Find(DesertCoverName) != null)
                    {
                        continue;
                    }

                    GameObject cover = Instantiate(coverPrefab, puzzle.transform);
                    cover.name = DesertCoverName;
                    cover.transform.SetAsLastSibling();

                    RectTransform coverRect = cover.GetComponent<RectTransform>();
                    RectTransform puzzleRect = puzzle.GetComponent<RectTransform>();

                    if (coverRect != null && puzzleRect != null)
                    {
                        coverRect.anchorMin = Vector2.zero;
                        coverRect.anchorMax = Vector2.one;
                        coverRect.offsetMin = Vector2.zero;
                        coverRect.offsetMax = Vector2.zero;
                        coverRect.localScale = Vector3.one;
                        coverRect.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        cover.transform.localPosition = Vector3.zero;
                        cover.transform.localRotation = Quaternion.identity;
                        cover.transform.localScale = Vector3.one;
                    }

                    Graphic[] graphics = cover.GetComponentsInChildren<Graphic>(true);
                    foreach (Graphic graphic in graphics)
                    {
                        graphic.raycastTarget = false;
                    }
                }
            }
            
            Material targetMaterial = _puzzles[0, 0].transform.Find(DesertCoverName).GetChild(0).GetComponent<Image>().material;
            yield return targetMaterial.DOFloat(1f, "_Appear", 1.0f).WaitForCompletion();
        }
        
        private IEnumerator ClearDesertPuzzleCovers()
        {
            if (_puzzles == null)
            {
                yield break;
            }

            ParticleSystem.EmissionModule emission = _desertSandParticle.emission;
            emission.enabled = false;
            
            Material mat = _desertSandStorm.GetComponent<SpriteRenderer>().material;
            mat.DOFloat(0, "_Appear", 1f).WaitForCompletion();
            Destroy(_desertSandStorm);
            
            Material targetMaterial = _puzzles[0, 0].transform.Find(DesertCoverName).GetChild(0).GetComponent<Image>().material;
            yield return targetMaterial.DOFloat(0f, "_Appear", 1.0f).WaitForCompletion();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    PuzzleObject puzzle = _puzzles[i, j];

                    if (puzzle == null)
                    {
                        continue;
                    }

                    Transform cover = puzzle.transform.Find(DesertCoverName);

                    if (cover != null)
                    {
                        Destroy(cover.gameObject);
                    }
                }
            }
            
            Destroy(_desertSandParticle.gameObject);
        }
        #endregion
        
        /// <summary>
        /// 퍼즐을 옮기는 것과 연관된 함수들
        /// </summary>
        #region Swap And Match Puzzle
        public void TrySwapPuzzles(int x1, int y1, int x2, int y2)
        {
            if (x2 < 0 || x2 >= x || y2 < 0 || y2 >= y)
            {
                return;
            }
            
            if (_taskQueue.Count > 0 || _isProcessing)
            {
                return;
            }
            
            if (_puzzles[x1, y1] is ObstaclePuzzleObject { isSwappable : false } 
                || _puzzles[x2, y2] is ObstaclePuzzleObject { isSwappable : false }
                || (maxSwapCount != -1 && _swapCount >= maxSwapCount)
                || (isContinuousHabitatBanned && _lastMovedHabitat != null &&
                    ((_puzzles[x1, y1] is NormalPuzzleObject np1 && np1.GetPuzzleSubType() == (int)_lastMovedHabitat)
                     || (_puzzles[x2, y2] is NormalPuzzleObject np2 && np2.GetPuzzleSubType() == (int)_lastMovedHabitat))))
            {
                if (_puzzles[x1, y1] is SpecialPuzzleObject sp1)
                {
                    sp1.isBlocked = true;
                }
                if (_puzzles[x2, y2] is SpecialPuzzleObject sp2)
                {
                    sp2.isBlocked = true;
                }
                
                SoundManager.Instance.PlaySFX(SFX.SFX3_CannotMove);
                _puzzles[x1, y1].FailedSwapEffect(x2 - x1, y2 - y1, 
                    Vector2.Distance(_puzzles[x1, y1].transform.position, _puzzles[x2, y2].transform.position) / 2);
                return;
            }
            
            _swapCount++;
            OnSwapCountChanged?.Invoke(maxSwapCount - _swapCount);
            
            AddTask(() => SwapAndCheck(x1, y1, x2, y2));
        }

        private IEnumerator SwapAndCheck(int x1, int y1, int x2, int y2)
        {
            _movedPositions.Clear();
            _movedPositions.Add(new Vector2Int(x1, y1));
            _movedPositions.Add(new Vector2Int(x2, y2));

            _swappedPositions.Clear();
            _swappedPositions.Add(new Vector2Int(x1, y1));
            _swappedPositions.Add(new Vector2Int(x2, y2));
            
            var p1 = _puzzles[x1, y1];
            var p2 = _puzzles[x2, y2];
            p1.puzzleState = PuzzleState.Swapping;
            p2.puzzleState = PuzzleState.Swapping;
    
            _puzzles[x1, y1] = p2;
            _puzzles[x2, y2] = p1;
            
            Vector3 pos1 = p1.transform.position;
            Vector3 pos2 = p2.transform.position;
            
            Sequence seq1 = DOTween.Sequence();
            Tween t1 = p1.transform.DOMove(pos2, 0.2f);
            Tween t2 = p2.transform.DOMove(pos1, 0.2f);
            seq1.Append(t1);
            seq1.Join(t2);
            
            yield return seq1.WaitForCompletion();
            
            p1.Init(this, x2, y2);
            p2.Init(this, x1, y1);
            
            bool isSpecialPuzzleExist = false;
            Vector2Int? delayedBombPos = null;
            
            if (_puzzles[x1, y1] is SpecialPuzzleObject)
            {
                isSpecialPuzzleExist = true;
                delayedBombPos = new Vector2Int(x1, y1);
            }
            
            if (_puzzles[x2, y2] is SpecialPuzzleObject sp2)
            {
                isSpecialPuzzleExist = true;
                yield return ActivateSpecialBomb(x2, y2, sp2.specialPuzzleType, !delayedBombPos.HasValue);
            }
            
            if (CheckAnyMatches() || isSpecialPuzzleExist)
            {
                yield return MatchPuzzle(delayedBombPos);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                _puzzles[x1, y1] = p1;
                _puzzles[x2, y2] = p2;
            
                Sequence seq2 = DOTween.Sequence();
                Tween t3 = p1.transform.DOMove(pos1, 0.2f);
                Tween t4 = p2.transform.DOMove(pos2, 0.2f);
                seq2.Append(t3);
                seq2.Join(t4);
            
                yield return seq2.WaitForCompletion();
            
                p1.Init(this, x1, y1);
                p2.Init(this, x2, y2);
                
                p1.puzzleState = PuzzleState.Idle;
                p2.puzzleState = PuzzleState.Idle;
            }

            if (maxSwapCount != -1 && _swapCount >= maxSwapCount)
            {
                StageManager.Instance.GameOver();
            }
        }

        private bool CheckAnyMatches()
        {
            _currentMatchGroups.Clear();
            bool hasMatch = false;
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null)
                    {
                        _puzzles[i, j].isMatched = false;
                    }
                }
            }
            
            for (int j = 0; j < y; j++)
            {
                for (int i = 0; i < x - 2; i++)
                {
                    if (CheckType(_puzzles[i, j], _puzzles[i + 1, j]) &&
                        CheckType(_puzzles[i, j], _puzzles[i + 2, j]) && 
                        CheckType(_puzzles[i + 1, j], _puzzles[i + 2, j]))
                    {
                        _puzzles[i, j].isMatched = true;
                        CheckAnyObstaclePuzzle(i, j);
                        _puzzles[i + 1, j].isMatched = true;
                        CheckAnyObstaclePuzzle(i + 1, j);
                        _puzzles[i + 2, j].isMatched = true;
                        CheckAnyObstaclePuzzle(i + 2, j);
                        hasMatch = true;
                    }
                }
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y - 2; j++)
                {
                    if (CheckType(_puzzles[i, j], _puzzles[i, j + 1]) &&
                        CheckType(_puzzles[i, j], _puzzles[i, j + 2]) && 
                        CheckType(_puzzles[i, j + 1], _puzzles[i, j + 2]))
                    {
                        _puzzles[i, j].isMatched = true;
                        CheckAnyObstaclePuzzle(i, j);
                        _puzzles[i, j + 1].isMatched = true;
                        CheckAnyObstaclePuzzle(i, j + 1);
                        _puzzles[i, j + 2].isMatched = true;
                        CheckAnyObstaclePuzzle(i, j + 2);
                        hasMatch = true;
                    }
                }
            }

            if (!hasMatch) return false;
            
            bool[,] visited = new bool[x, y];

            for (int j = 0; j < y; j++)
            {
                for (int i = 0; i < x; i++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].isMatched && !visited[i, j])
                    {
                        if (_puzzles[i, j] is JokerPuzzleObject)
                        {
                            continue;
                        }
                        
                        MatchGroup group = GetMatchGroupBfs(i, j, visited);
                
                        if (group.positions.Count >= 3)
                        {
                            if (group.positions.Count >= 4)
                            {
                                DetermineSpecialType(group);
                            }
                            _currentMatchGroups.Add(group);
                        }
                    }
                }
            }

            return _currentMatchGroups.Count > 0;
        }

        private void CheckAnyObstaclePuzzle(int i, int j)
        {
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { 1, -1, 0, 0 };
            
            if (_puzzles[i, j] is ObstaclePuzzleObject op1)
            {
                op1.isTriggered = true;
            }

            for (int d = 0; d < 4; d++)
            {
                int ni = i + dx[d];
                int nj = j + dy[d];

                if (ni >= 0 && ni < x && nj >= 0 && nj < y)
                {
                    if (_puzzles[ni, nj] is LockedTwicePuzzleObject lt)
                    {
                        if (_swappedPositions.Contains(new Vector2Int(ni, nj)))
                        {
                            lt.isTriggered = true;
                        }
                    }
                    else if (_puzzles[ni, nj] is ObstaclePuzzleObject op2)
                    {
                        op2.isTriggered = true;
                    }
                }
            }
        }
        
        private MatchGroup GetMatchGroupBfs(int startX, int startY, bool[,] visited)
        {
            MatchGroup group = new();
            Habitat habitat = _puzzles[startX, startY] switch
            {
                ObstaclePuzzleObject { isMatchable : true } o => o.habitat,
                _ => (Habitat)_puzzles[startX, startY].GetPuzzleSubType()
            };
            group.habitat = habitat;

            Queue<Vector2Int> queue = new();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                Vector2Int curr = queue.Dequeue();
                group.positions.Add(curr);

                foreach (var dir in dirs)
                {
                    int nx = curr.x + dir.x;
                    int ny = curr.y + dir.y;
                    
                    if (!CheckForcedDirection(_puzzles[curr.x, curr.y], dir))
                    {
                        continue;
                    }
                    
                    if (nx >= 0 && nx < x && ny >= 0 && ny < y && !visited[nx, ny])
                    {
                        if (!CheckForcedDirection(_puzzles[nx, ny], dir))
                        {
                            continue;
                        }
                        
                        if (_puzzles[nx, ny] && _puzzles[nx, ny].isMatched && 
                            CheckNormalType(_puzzles[nx, ny], habitat))
                        {
                            if (_puzzles[nx, ny] is not JokerPuzzleObject)
                            {
                                visited[nx, ny] = true;
                            }

                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
            return group;
        }
        
        private bool CheckForcedDirection(PuzzleObject puzzle, Vector2Int dir)
        {
            if (puzzle is not ForcedRowColumnPuzzleObject forced)
            {
                return true;
            }

            return forced.forcedDirection switch
            {
                ForcedDirection.ForcedRow =>
                    dir == Vector2Int.left || dir == Vector2Int.right,

                ForcedDirection.ForcedColumn =>
                    dir == Vector2Int.up || dir == Vector2Int.down,

                _ => true
            };
        }
        
        private void DetermineSpecialType(MatchGroup group)
        {
            // 유저가 이동시키거나 옮겨진 타일이 포함되면 우선으로 생성위치 부여(그 중에서 왼쪽 아래 우선)
            List<Vector2Int> movedCandidates = new();
            foreach (var pos in group.positions)
            {
                if (_movedPositions.Contains(pos))
                {
                    movedCandidates.Add(pos);
                }
            }
            
            if (movedCandidates.Count == 0)
            {
                movedCandidates = group.positions;
            }
            
            Vector2Int bestPos = movedCandidates[0];
            foreach (var pos in movedCandidates)
            {
                if (pos.y < bestPos.y || (pos.y == bestPos.y && pos.x < bestPos.x))
                {
                    bestPos = pos;
                }
            }

            group.spawnPos = bestPos;
            
            int maxH = 0;
            int maxV = 0;

            foreach (var pos in group.positions)
            {
                int h = 1 + GetLength(group, pos, Vector2Int.left) + GetLength(group, pos, Vector2Int.right);
                int v = 1 + GetLength(group, pos, Vector2Int.up) + GetLength(group, pos, Vector2Int.down);
        
                maxH = Mathf.Max(maxH, h);
                maxV = Mathf.Max(maxV, v);
            }
            
            if (maxH >= 5 || maxV >= 5 || (maxH >= 3 && maxV >= 3))
            {
                // 5개 이상 일렬이거나, 가로/세로가 교차(T, L자)하는 경우
                group.resultType = SpecialPuzzleType.CircleBomb;
            }
            else if (maxH == 4)
            {
                // 가로로 4개 -> 가로 폭탄 (Column)
                group.resultType = SpecialPuzzleType.RowBomb;
            }
            else if (maxV == 4)
            {
                // 세로로 4개 -> 세로 폭탄 (Row)
                group.resultType = SpecialPuzzleType.ColumnBomb;
            }
            // todo : CrossBomb, ColorBomb 조건 추가
        }
        
        private int GetLength(MatchGroup group, Vector2Int start, Vector2Int dir)
        {
            int count = 0;
            Vector2Int next = start + dir;
            while (group.positions.Contains(next))
            {
                count++;
                next += dir;
            }
            return count;
        }
        
        private IEnumerator MatchPuzzle(Vector2Int? delayedBombPos = null)
        {
            foreach (Vector2Int pos in _movedPositions)
            {
                PuzzleObject puzzle = _puzzles[pos.x, pos.y];

                if (!puzzle)
                {
                    continue;
                }

                if (puzzle.puzzleState != PuzzleState.Swapping)
                {
                    continue;
                }

                bool isMatched = _currentMatchGroups.Any(group => group.positions.Contains(pos));

                if (!isMatched)
                {
                    puzzle.puzzleState = PuzzleState.Idle;
                }
            }
            
            foreach (var group in _currentMatchGroups)
            {
                foreach (var pos in group.positions)
                {
                    if (_puzzles[pos.x, pos.y] != null)
                    {
                        _puzzles[pos.x, pos.y].puzzleState = PuzzleState.Matching;
                        SoundManager.Instance.PlaySFX(SFX.SFX5_TilePop);
                    }
                }
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j])
                    {
                        if (_puzzles[i, j] is ObstaclePuzzleObject { isTriggered : true } op)
                        {
                            yield return ObstacleMatch(i, j, op.obstaclePuzzleType);
                        }
                    }
                }
            }
            
            foreach (var group in _currentMatchGroups)
            {
                Vector3 destination = CalculatePos(group.spawnPos.x, group.spawnPos.y);
                
                Sequence seq1 = DOTween.Sequence();
                foreach (var pos in group.positions)
                {
                    if (_puzzles[pos.x, pos.y] is NormalPuzzleObject no)
                    {
                        if (TryGetGoldTile(no, out _))
                        {
                            goldUI.ShowUI();
                        }
                        
                        seq1.Join(no.HighlightEffect());
                    }

                    if (_puzzles[pos.x, pos.y] is ObstaclePuzzleObject { isMatchable : true } op)
                    {
                        seq1.Join(op.HighlightEffect());
                    }
                }
                yield return seq1.WaitForCompletion();
                
                OnComboDetected?.Invoke();
                
                Sequence seq2 = DOTween.Sequence();
                List<PuzzleObject> targets = new();
                foreach (var pos in group.positions)
                {
                    var targetPuzzle = _puzzles[pos.x, pos.y];
                    
                    if (!targetPuzzle)
                    {
                        continue;
                    }

                    if (targetPuzzle is LockedTwicePuzzleObject { isLocked : true } )
                    {
                        continue;
                    }
                    
                    targets.Add(targetPuzzle);
                    _puzzles[pos.x, pos.y] = null;

                    if (group.resultType != null)
                    {
                        Tween t1 = targetPuzzle.transform.DOLocalMove(destination, 0.2f);
                        seq2.Join(t1);
                    }

                    if (targetPuzzle is NormalPuzzleObject no1 && TryGetGoldTile(no1, out GameObject gold))
                    {
                        GoldMoveEffect(gold);
                    }

                    if (targetPuzzle is NormalPuzzleObject no2 && TryGetSwapTile(no2))
                    {
                        _swapCount--;
                        OnSwapCountChanged?.Invoke(maxSwapCount - _swapCount);
                    }
                    
                    Tween t2 = targetPuzzle.transform.DOScale(tileScale / 3, 0.2f).SetEase(Ease.InSine);
                    seq2.Join(t2);
                }
                yield return seq2.WaitForCompletion();
                
                foreach (PuzzleObject targetPuzzle in targets)
                {
                    if (!targetPuzzle)
                    {
                        continue;
                    }
                    
                    targetPuzzle.transform.SetParent(puzzleFrame.parent);
                    
                    Vector3 startPos = targetPuzzle.transform.position;
                    Vector3 endPos = spawnStackManager.SetStack(group.habitat).transform.position;

                    float distance = Vector3.Distance(startPos, endPos);
                    float speed = 7.5f;
                    float duration = distance / speed;
                    float jumpPower = distance * 0.3f;
                    
                    Sequence seq = DOTween.Sequence();

                    seq.Append(DOTween.To(
                        () => 0f,
                        t => {
                            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

                            float height = Mathf.Sin(t * Mathf.PI) * jumpPower;

                            targetPuzzle.transform.position = pos + Vector3.up * height;
                        },
                        1f,
                        duration
                    ).SetEase(Ease.OutSine)
                    .OnComplete(() =>
                    {
                        spawnStackManager.AddStack(group.habitat, 1);
                        StageManager.Instance.SetUsedTile(1);
                        Destroy(targetPuzzle.gameObject);
                    }));
                }

                if (group.resultType != null && !_puzzles[group.spawnPos.x, group.spawnPos.y])
                {
                    GameObject newPuzzle = Instantiate(specialPuzzlePrefabs[(int)group.resultType], puzzleFrame);
                    newPuzzle.transform.localPosition = destination;
                    newPuzzle.name = $"Puzzle({group.spawnPos.x + 1},{group.spawnPos.y + 1})";
                    newPuzzle.transform.localScale = Vector3.zero;
            
                    PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
                    _puzzles[group.spawnPos.x, group.spawnPos.y] = po;
                    po.Init(this, group.spawnPos.x, group.spawnPos.y);
                    po.isMatched = false;
            
                    yield return newPuzzle.transform.DOScale(tileScale, 0.2f)
                        .SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            po.puzzleState = PuzzleState.Idle;
                        })
                        .WaitForCompletion();
                }
                
                _lastMovedHabitat = group.habitat;
            }

            if (delayedBombPos.HasValue)
            {
                int bx = delayedBombPos.Value.x;
                int by = delayedBombPos.Value.y;

                if (_puzzles[bx, by] is SpecialPuzzleObject sp)
                {
                    yield return StartCoroutine(SpecialMatch(bx, by, sp.specialPuzzleType));
                }
            }
            
            yield return DropBlocks();
        }

        private IEnumerator DropBlocks()
        {
            _movedPositions.Clear();
            Sequence seq = DOTween.Sequence();

            bool hasEmptySlot = true;
            
            while(hasEmptySlot)
            {
                hasEmptySlot = false;
                
                for (int i = 0; i < x; i++)
                {
                    int spawnOrder = 0;

                    for (int j = 0; j < y; j++)
                    {
                        if (!_puzzles[i, j])
                        {
                            hasEmptySlot = true;
                            
                            PuzzleObject targetPo = null;
                            bool foundUpperTile = false;
                            PortalPuzzleObject portal = null;
                            PortalPuzzleObject linkedPortal = null;

                            for (int k = j + 1; k < y; k++)
                            {
                                if (_puzzles[i, k] is PortalPuzzleObject pp)
                                {
                                    portal = pp;
                                    linkedPortal = pp.linkedPortal;
                                    break;
                                }

                                if (_puzzles[i, k])
                                {
                                    _puzzles[i, k].puzzleState = PuzzleState.Falling;
                                    _puzzles[i, j] = _puzzles[i, k];
                                    _puzzles[i, k] = null;
                                    targetPo = _puzzles[i, j];
                                    foundUpperTile = true;
                                    break;
                                }
                            }

                            if (portal)
                            {
                                int linkedX = linkedPortal.column;
                                for (int k = linkedPortal.row + 1; k < y; k++)
                                {
                                    if (_puzzles[linkedX, k])
                                    {
                                        _puzzles[linkedX, k].puzzleState = PuzzleState.Falling;
                                        _puzzles[i, j] = _puzzles[linkedX, k];
                                        _puzzles[linkedX, k] = null;
                                        targetPo = _puzzles[i, j];
                                        foundUpperTile = true;
                                        break;
                                    }
                                }
                            }

                            if (!foundUpperTile)
                            {
                                GameObject puzzle = portal
                                    ? SetRandomPuzzle(linkedPortal.column, j, spawnOrder)
                                    : SetRandomPuzzle(i, j, spawnOrder);
                                spawnOrder++;
                                targetPo = puzzle.GetComponent<PuzzleObject>();
                                targetPo.puzzleState = PuzzleState.Falling;
                                _puzzles[i, j] = targetPo;
                            }

                            if (targetPo)
                            {
                                targetPo.gameObject.name = $"Puzzle({i + 1},{j + 1})";
                                targetPo.Init(this, i, j);
                                _movedPositions.Add(new Vector2Int(i, j));

                                Vector3 targetPos = CalculatePos(i, j);

                                float distance = Vector3.Distance(targetPo.transform.localPosition, targetPos);
                                float duration = distance / dropSpeed;
                                float startAt = columnDropDelay * i + rowDropDelay * j;

                                Tween fallTween;

                                if (portal)
                                {
                                    float distance1 = Vector3.Distance(targetPo.transform.localPosition,
                                        linkedPortal.transform.localPosition);
                                    float distance2 = Vector3.Distance(portal.transform.localPosition, targetPos);
                                    float duration1 = distance1 / dropSpeed;
                                    float duration2 = distance2 / dropSpeed;

                                    Vector3 linkedPortalPos = CalculatePos(linkedPortal.column, linkedPortal.row);
                                    Vector3 portalPos = CalculatePos(portal.column, portal.row);
                                    fallTween = targetPo.transform.DOLocalMove(linkedPortalPos, duration1)
                                        .SetEase(Ease.InSine)
                                        .OnComplete(() =>
                                        {
                                            targetPo.transform.localPosition = portalPos;
                                            targetPo.transform.DOLocalMove(targetPos, duration2)
                                                .SetEase(Ease.InSine)
                                                .OnComplete(() =>
                                                {
                                                    targetPo.transform.DOPunchPosition(Vector3.down * 0.05f, 0.15f, 8)
                                                        .OnComplete(() =>
                                                        {
                                                            targetPo.puzzleState = PuzzleState.Idle;
                                                        });
                                                });
                                        });
                                }
                                else
                                {
                                    fallTween = targetPo.transform.DOLocalMove(targetPos, duration)
                                        .SetEase(Ease.InSine)
                                        .OnComplete(() =>
                                        {
                                            targetPo.transform.DOPunchPosition(Vector3.down * 0.05f, 0.15f, 8)
                                                .OnComplete(() =>
                                                {
                                                    targetPo.puzzleState = PuzzleState.Idle;
                                                });
                                        });
                                }

                                seq.Insert(startAt, fallTween);
                            }
                        }
                    }
                }
            }
            
            yield return seq.WaitForCompletion();
            yield return new WaitForSeconds(0.1f);
            
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
            }
            else
            {
                OnComboInitialized?.Invoke();
                goldUI.ShowUI(false);
            }
        }
        
        #endregion
        
        /// <summary>
        /// 특수 및 장애물 타일 관련 함수
        /// </summary>
        #region Abnormal Puzzle
        public IEnumerator ActivateSpecialBomb(int curX, int curY, SpecialPuzzleType type, bool dropAfter = true)
        {
            yield return SpecialMatch(curX, curY, type);
            
            yield return new WaitForSeconds(0.2f);

            if (dropAfter)
            {
                yield return DropBlocks();
            }
        }
        
        private IEnumerator SpecialMatch(int curX, int curY, SpecialPuzzleType type)
        {
            if (_puzzles[curX, curY] == null)
            {
                yield break;
            }

            List<Vector2Int> targets = GetExplosionRange(curX, curY, type);
            if (type == SpecialPuzzleType.CircleBomb)
            {
                SoundManager.Instance.PlaySFX(SFX.SFX4_CircleTile);
            }
            if (type == SpecialPuzzleType.RowBomb || type == SpecialPuzzleType.ColumnBomb)
            {
                SoundManager.Instance.PlaySFX(SFX.SFX9_LineTile);
            }
            
            GameObject self = _puzzles[curX, curY].gameObject;
            Vector2 center = new (self.transform.position.x, self.transform.position.y);
            _puzzles[curX, curY] = null;

            yield return self.transform.DOScale(tileScale * 1.2f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    StageManager.Instance.SetUsedTile(1);
                    Destroy(self);
                })
                .WaitForCompletion();
            
            Queue<(SpecialPuzzleObject, Vector2Int)> q = new();
            List<PuzzleObject> targetPuzzles = new();
            
            foreach (var pos in targets)
            {
                if (!_puzzles[pos.x, pos.y] || _puzzles[pos.x, pos.y] is PortalPuzzleObject)
                {
                    continue;
                }

                PuzzleObject targetPuzzle = _puzzles[pos.x, pos.y];
                targetPuzzle.puzzleState = PuzzleState.Matching;
                
                if (targetPuzzle is SpecialPuzzleObject nextSp)
                {
                    q.Enqueue((nextSp, new Vector2Int(pos.x, pos.y)));
                }
                else
                {
                    targetPuzzles.Add(targetPuzzle);
                    _puzzles[pos.x, pos.y] = null;
                }
            }

            yield return SetExplosionEffect(center.x, center.y, targetPuzzles, type);

            OnComboDetected?.Invoke();

            while (q.Count > 0)
            {
                var sp = q.Dequeue();
                yield return DelayedSpecialMatch(sp.Item2.x, sp.Item2.y, sp.Item1.specialPuzzleType);
            }
        }

        private IEnumerator DelayedSpecialMatch(int curX, int curY, SpecialPuzzleType type, float delay = 0.1f)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(SpecialMatch(curX, curY, type));
        }
        
        private List<Vector2Int> GetExplosionRange(int curX, int curY, SpecialPuzzleType type)
        {
            List<Vector2Int> range = new();

            switch (type)
            {
                case SpecialPuzzleType.CircleBomb:
                    for (int i = curX - 2; i <= curX + 2; i++)
                    {
                        for (int j = curY - 2; j <= curY + 2; j++)
                        {
                            if (i < 0 || i >= x || j < 0 || j >= y) continue;
                    
                            bool isCorner = (i == curX - 2 || i == curX + 2) && (j == curY - 2 || j == curY + 2);
            
                            if (!isCorner)
                            {
                                range.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                    break;
                case SpecialPuzzleType.ColumnBomb:
                    for (int j = 0; j < y; j++)
                    {
                        range.Add(new Vector2Int(curX, j));
                    }
                    break;
                case SpecialPuzzleType.RowBomb:
                    for (int i = 0; i < x; i++)
                    {
                        range.Add(new Vector2Int(i, curY));
                    }
                    break;
                case SpecialPuzzleType.CrossBomb:
                    for (int i = 0; i < x; i++)
                    {
                        for (int j = 0; j < y; j++)
                        {
                            if (i != curX && j != curY)
                            {
                                continue;
                            }
                    
                            range.Add(new Vector2Int(i, j));
                        }
                    }
                    break;
                case SpecialPuzzleType.ColorBomb:
                    var normalType = _puzzles[curX, curY].GetComponent<SpecialPuzzleObject>().habitat;

                    for (int i = 0; i < x; i++)
                    {
                        for (int j = 0; j < y; j++)
                        {
                            if (_puzzles[i, j] != null && _puzzles[i, j].puzzleType == PuzzleType.Normal && 
                                (Habitat)_puzzles[i, j].GetPuzzleSubType() == normalType)
                            {
                                range.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                    break;
            }
            return range;
        }

        private IEnumerator SetExplosionEffect(float posX, float posY, List<PuzzleObject> list, SpecialPuzzleType type)
        {
            GameObject effect = Instantiate(specialPuzzleParticlePrefabs[(int)type], new Vector2(posX, posY), Quaternion.identity, particleFrame.transform);
            PlayParticleEffect(effect);
            ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(effect, totalDuration);
            
            float maxLifetime = ps.main.startLifetime.constant;
            Vector2 centerPos = new(posX, posY);
            switch (type)
            {
                case SpecialPuzzleType.CircleBomb:
                    if (ps != null)
                    {
                        foreach (PuzzleObject po in list)
                        {
                            float distance = Vector2.Distance(centerPos, new Vector2(po.transform.position.x, po.transform.position.y));
                            
                            float maxRadius = 3f;
                            float delayRatio = Mathf.Clamp01(distance / maxRadius);
                            float finalDelay = delayRatio * maxLifetime;
                            
                            StartCoroutine(DelayedTileEffect(po, finalDelay));
                        }
                    }
                    break;
                case SpecialPuzzleType.CrossBomb:
                    foreach (PuzzleObject po in list)
                    {
                        float distance = Vector2.Distance(centerPos, new Vector2(po.transform.position.x, po.transform.position.y));
                        float delay = maxLifetime / distance / 1.5f;
                        
                        StartCoroutine(DelayedTileEffect(po, delay));
                    }
                    break;
                case SpecialPuzzleType.RowBomb:
                    foreach (PuzzleObject po in list)
                    {
                        float distance = Vector2.Distance(centerPos, new Vector2(po.transform.position.x, po.transform.position.y));
                        
                        float maxDistance = 4.0f;
                        float normalizedDist = Mathf.Clamp01(distance / maxDistance);
                        float curveFactor = Mathf.Pow(1 - normalizedDist, 1.5f);

                        float delay = maxLifetime * curveFactor;
                        
                        StartCoroutine(DelayedTileEffect(po, delay));
                    }
                    break;
                case SpecialPuzzleType.ColumnBomb:
                    foreach (PuzzleObject po in list)
                    {
                        float distance = Vector2.Distance(centerPos, new Vector2(po.transform.position.x, po.transform.position.y));
                        
                        float maxDistance = 4.0f;
                        float normalizedDist = Mathf.Clamp01(distance / maxDistance);
                        float curveFactor = Mathf.Pow(1 - normalizedDist, 1.5f);

                        float delay = maxLifetime * curveFactor;
                        
                        StartCoroutine(DelayedTileEffect(po, delay));
                    }
                    break; 
            }
            yield return new WaitForSeconds(0.1f);
        }

        private void PlayParticleEffect(GameObject effect)
        {
            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                particle.Play();
            }
        }

        private IEnumerator DelayedTileEffect(PuzzleObject po, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (po is NormalPuzzleObject no1)
            {
                if (TryGetGoldTile(no1, out _))
                {
                    goldUI.ShowUI();
                }
                yield return no1.HighlightEffect().WaitForCompletion();
            }

            if (po is ForcedRowColumnPuzzleObject frc)
            {
                yield return frc.HighlightEffect().WaitForCompletion();
            }
            
            if (po is ChangingHabitatPuzzleObject ch)
            {
                yield return ch.HighlightEffect().WaitForCompletion();
            }
            
            if (po is NormalPuzzleObject no2 && TryGetGoldTile(no2, out GameObject gold))
            {
                GoldMoveEffect(gold);
            }
            
            if (po is NormalPuzzleObject no3 && TryGetSwapTile(no3))
            {
                _swapCount--;
                OnSwapCountChanged?.Invoke(maxSwapCount - _swapCount);
            }
            
            yield return po.transform.DOScale(tileScale / 3f, 0.2f).WaitForCompletion();
            
            FlyingTileEffect(po);
        }

        private void FlyingTileEffect(PuzzleObject targetPuzzle)
        {
            if (targetPuzzle is NormalPuzzleObject no)
            {
                targetPuzzle.transform.SetParent(puzzleFrame.parent);
                    
                Vector3 startPos = targetPuzzle.transform.position;
                Vector3 endPos = spawnStackManager.SetStack(no.habitat).transform.position;

                float distance = Vector3.Distance(startPos, endPos);
                float speed = 7.5f;
                float duration = distance / speed;
                float jumpPower = distance * 0.3f;
                
                DOTween.To(
                        () => 0f,
                        t => {
                            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

                            float height = Mathf.Sin(t * Mathf.PI) * jumpPower;

                            targetPuzzle.transform.position = pos + Vector3.up * height;
                        },
                        1f,
                        duration
                    ).SetEase(Ease.OutSine)
                    .OnComplete(() =>
                    {
                        spawnStackManager.AddStack(no.habitat, 1);
                        StageManager.Instance.SetUsedTile(1);
                        Destroy(targetPuzzle.gameObject);
                    });
            }
            else
            {
                StageManager.Instance.SetUsedTile(1);
                Destroy(targetPuzzle.gameObject);
            }
        }
        
        public void SpawnSpecialPuzzle(SpecialPuzzleType type)
        {
            AddTask(() => SpawnSpecialPuzzleCoroutine(type));
        }

        private IEnumerator SpawnSpecialPuzzleCoroutine(SpecialPuzzleType type)
        {
            List<PuzzleObject> list = new();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null &&
                        _puzzles[i, j] is not SpecialPuzzleObject and not ObstaclePuzzleObject)
                    {
                        list.Add(_puzzles[i, j]);
                    }
                }
            }

            PuzzleObject target = list[Random.Range(0, list.Count)];
            Vector3 currentPos = target.transform.position;
            Vector2Int grid = new(target.column, target.row);

            _puzzles[target.column, target.row] = null;
            Destroy(target.gameObject);

            GameObject newPuzzle = Instantiate(specialPuzzlePrefabs[(int)type], puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({grid.x + 1},{grid.y + 1})";
            newPuzzle.transform.localScale = Vector3.zero;

            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[grid.x, grid.y] = po;
            po.Init(this, grid.x, grid.y);
            po.isMatched = false;

            yield return newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    po.puzzleState = PuzzleState.Idle;
                })
                .WaitForCompletion();
        }

        private IEnumerator SpawnObstaclePuzzle()
        {
            while (true)
            {
                yield return new WaitForSeconds(obstacleSpawnInterval);
                Vector2Int pos = SetObstacleSpawnPos();
                if (pos == new Vector2Int(-1, -1))
                {
                    yield break;
                }
                
                StartCoroutine(SpawnRandomObstaclePuzzleCoroutine(pos.x, pos.y));
            }
        }

        private Vector2Int SetObstacleSpawnPos()
        {
            List<PuzzleObject> list = new();
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].puzzleType != PuzzleType.Obstacle)
                    {
                        list.Add(_puzzles[i, j]);
                    }
                }
            }

            if (list.Count == 0)
            {
                return new Vector2Int(-1, -1);
            }
            
            PuzzleObject target = list[Random.Range(0, list.Count)];

            return new Vector2Int(target.column, target.row);
        }
        
        private IEnumerator SpawnRandomObstaclePuzzleCoroutine(int curX, int curY)
        {
            Vector2 pos = CalculatePos(curX, curY);
            GameObject warningOb = Instantiate(obstacleWarningPrefab, puzzleFrame);
            warningOb.transform.localPosition = pos;
            Tween warningTween = warningOb.GetComponent<SpriteRenderer>().DOFade(0.1f, obstacleSpawnDelay / 4)
                .SetLoops(-1, LoopType.Yoyo);

            yield return new WaitForSeconds(obstacleSpawnDelay);
            
            yield return new WaitUntil(() =>
                _puzzles[curX, curY] &&
                _puzzles[curX, curY].puzzleState == PuzzleState.Idle
            );
            
            warningTween.Kill();
            Destroy(warningOb);
            
            PuzzleObject target = _puzzles[curX, curY];

            if (target is ObstaclePuzzleObject)
            {
                yield break;
            }
            
            target.puzzleState = PuzzleState.Swapping;

            Vector3 currentPos = target.transform.position;
            Habitat type = (Habitat)_puzzles[curX, curY].GetPuzzleSubType();
            Material material = new(normalPuzzlePrefabs[(int)type].GetComponent<Image>().material);
            _puzzles[curX, curY] = null;
            Destroy(target.gameObject);
            
            GameObject newPuzzle = Instantiate(obstaclePuzzlePrefabs[(int)GetWeightedRandomObstacle()], puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = po;
            po.Init(this, curX, curY);
            po.puzzleState = PuzzleState.Idle;
            po.isMatched = false;

            switch (po)
            {
                case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.DeActivated } op:
                    op.habitat = type;
                    break;
                case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op:
                    op.habitat = type;
                    op.GetComponent<Image>().sprite = normalPuzzleImages[(int)type];
                    op.SetMaterial(material);
                    break;
                case ForcedRowColumnPuzzleObject frc:
                    frc.habitat = type;
                    frc.forcedDirection = Random.Range(0, 2) == 0 ? ForcedDirection.ForcedRow : ForcedDirection.ForcedColumn;
                    frc.GetComponent<Image>().sprite = normalPuzzleImages[(int)type];
                    frc.SetMaterial(material);
                    
                    GameObject fp = Instantiate(forcedRowColumnDirectionPrefabs[(int)frc.forcedDirection], po.transform);
                    fp.transform.localPosition = new Vector3(0.2f, -0.2f, 0);
                    break;
                case ChangingHabitatPuzzleObject ch:
                    ch.InitialSetting(this, normalPuzzlePrefabs);
                    break;
                case LockedTwicePuzzleObject lt:
                    lt.habitat = type;
                    lt.GetComponent<Image>().sprite = normalPuzzleImages[(int)type];
                    lt.SetMaterial(material);
                    break;
                case InfectionPuzzleObject ip:
                    ip.Init(this);
                    break;
            }
            
            Tween t =  newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine);
            yield return t.WaitForCompletion();
        }
        
        private ObstaclePuzzleType GetWeightedRandomObstacle()
        {
            bool canCreateInfection = InfectionPuzzleObject.CanCreate();
            
            int totalWeight = 0;
            foreach (var obstacle in obstacleWeights)
            {
                if (obstacle.type == ObstaclePuzzleType.Infection &&
                    !canCreateInfection)
                {
                    continue;
                }
                
                totalWeight += obstacle.weight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            
            int currentSum = 0;
            foreach (var obstacle in obstacleWeights)
            {
                if (obstacle.type == ObstaclePuzzleType.Infection &&
                    !canCreateInfection)
                {
                    continue;
                }
                
                currentSum += obstacle.weight;
                if (randomValue < currentSum)
                {
                    return obstacle.type;
                }
            }
            
            return ObstaclePuzzleType.DeActivated;
        }
        
        // 서식지 변경 방해타일 용 서식지 확인 함수
        private bool CanChangeHabitat(int r, int c, Habitat habitat)
        {
            int count = 1;
            
            for (int i = r - 1; i >= 0; i--)
            {
                if (CheckNormalType(_puzzles[i, c], habitat))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int i = r + 1; i < x; i++)
            {
                if (CheckNormalType(_puzzles[i, c], habitat))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count >= 3)
            {
                return false;
            }
            
            count = 1;
            
            for (int j = c - 1; j >= 0; j--)
            {
                if (CheckNormalType(_puzzles[r, j], habitat))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int j = c + 1; j < y; j++)
            {
                if (CheckNormalType(_puzzles[r, j], habitat))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count < 3;
        }
        
        // 서식지 변경 방해타일 용 서식지 확인 함수
        public Habitat GetRandomSafeHabitat(int r, int c, Habitat currentHabitat)
        {
            List<Habitat> candidates = Enum.GetValues(typeof(Habitat))
                .Cast<Habitat>()
                .Where(h => h != currentHabitat && CanChangeHabitat(r, c, h))
                .ToList();

            return candidates.Count == 0 ? currentHabitat : candidates[Random.Range(0, candidates.Count)];
        }

        private IEnumerator ObstacleMatch(int curX, int curY, ObstaclePuzzleType type)
        {
            if (_puzzles[curX, curY] is ObstaclePuzzleObject op)
            {
                op.isTriggered = false;
            }
            
            switch (type)
            {
                case ObstaclePuzzleType.DeActivated:
                    yield return DeActivatedMatch(curX, curY);
                    break;
                case ObstaclePuzzleType.LockedTwice:
                    LockedTwicePuzzleObject lt = _puzzles[curX, curY] as LockedTwicePuzzleObject;
                    if (lt)
                    {
                        yield return lt.Unlock(this, curX, curY);
                    }
                    break;
                case ObstaclePuzzleType.Infection:
                    GameObject go = _puzzles[curX, curY].gameObject;
                    _puzzles[curX, curY] = null;
                    go.transform.DOScale(0, 0.2f)
                        .SetEase(Ease.OutSine)
                        .OnComplete(() =>
                        {
                            Destroy(go);
                        });
                    yield return null;
                    break;
            }
        }

        // 비활성화 방해타일 해제
        private IEnumerator DeActivatedMatch(int curX, int curY)
        {
            var obstacleObj = _puzzles[curX, curY].GetComponent<ObstaclePuzzleObject>();
            Habitat targetType = obstacleObj.habitat;
            Vector3 currentPos = _puzzles[curX, curY].transform.position;
            
            GameObject oldObject = _puzzles[curX, curY].gameObject;
            _puzzles[curX, curY] = null;
            Destroy(oldObject);
            
            GameObject newPuzzle = Instantiate(normalPuzzlePrefabs[(int)targetType], puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = po;
            po.Init(this, curX, curY);
            po.isMatched = false;

            newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    po.puzzleState = PuzzleState.Idle;
                });

            yield return null;
        }
        
        // 감염타일 감염 함수
        public IEnumerator Infect(InfectionPuzzleObject ip)
        {
            int indexX = ip.column;
            int indexY = ip.row;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            List<Vector2Int> candidates = new();

            foreach (Vector2Int direction in directions)
            {
                int targetX = indexX + direction.x;
                int targetY = indexY + direction.y;
                
                if (targetX < 0 || targetX >= x ||
                    targetY < 0 || targetY >= y)
                {
                    continue;
                }

                PuzzleObject target = _puzzles[targetX, targetY];
                
                if (!target || target is ObstaclePuzzleObject)
                {
                    continue;
                }

                candidates.Add(new Vector2Int(targetX, targetY));
            }
            
            if (candidates.Count == 0)
            {
                yield break;
            }
            
            Vector2Int selected = candidates[Random.Range(0, candidates.Count)];
            
            yield return new WaitUntil(() =>
                _puzzles[selected.x, selected.y] &&
                _puzzles[selected.x, selected.y].puzzleState == PuzzleState.Idle
            );

            if (!InfectionPuzzleObject.CanCreate())
            {
                yield break;
            }
            
            Vector3 currentPos = _puzzles[selected.x, selected.y].transform.position;
            GameObject oldObject = _puzzles[selected.x, selected.y].gameObject;
            _puzzles[selected.x, selected.y] = null;
            Destroy(oldObject);
            
            InfectionPuzzleObject newPuzzle = Instantiate(obstaclePuzzlePrefabs[(int)ObstaclePuzzleType.Infection], puzzleFrame).GetComponent<InfectionPuzzleObject>();
            newPuzzle.Init(this);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({selected.x + 1},{selected.y + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            _puzzles[selected.x, selected.y] = newPuzzle;
            newPuzzle.Init(this, selected.x, selected.y);
            newPuzzle.isMatched = false;
            
            newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    newPuzzle.puzzleState = PuzzleState.Idle;
                });

        }
        #endregion

        #region Item

        public IEnumerator UseJokerItem(PuzzleObject po)
        {
            if (po is not NormalPuzzleObject)
            {
                yield return null;
            }
            
            int curX = po.column, curY = po.row;
            
            yield return new WaitUntil(() =>
                _puzzles[curX, curY] != null &&
                _puzzles[curX, curY].puzzleState == PuzzleState.Idle
            );

            _puzzles[curX, curY].puzzleState = PuzzleState.Swapping;
            
            Vector3 currentPos = po.transform.position;
            _puzzles[curX, curY] = null;
            Destroy(po.gameObject);
            
            GameObject newPuzzle = Instantiate(jokerPuzzlePrefab, puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject puzzle = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = puzzle;
            puzzle.Init(this, curX, curY);
            puzzle.puzzleState = PuzzleState.Idle;
            puzzle.isMatched = false;
            
            Tween t =  newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine);
            yield return t.WaitForCompletion();
            
            AddTask(() => MatchPuzzle());
        }

        public IEnumerator UseDestroyObstacleItem(PuzzleObject po)
        {
            if (po is not ObstaclePuzzleObject { obstaclePuzzleType: not ObstaclePuzzleType.Portal })
            {
                yield return null;
            }
            
            int curX = po.column, curY = po.row;
            
            yield return new WaitUntil(() =>
                _puzzles[curX, curY] != null &&
                _puzzles[curX, curY].puzzleState == PuzzleState.Idle
            );

            _puzzles[curX, curY].puzzleState = PuzzleState.Swapping;
            _puzzles[curX, curY] = null;

            Tween t = po.transform.DOScale(0, 0.2f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    Destroy(po.gameObject);
                });

            yield return t.WaitForCompletion();
            
            AddTask(() => MatchPuzzle());
        }

        public IEnumerator UseCreateLineBombItem(PuzzleObject po)
        {
            if (po is not NormalPuzzleObject)
            {
                yield return null;
            }
            
            int curX = po.column, curY = po.row;
            
            yield return new WaitUntil(() =>
                _puzzles[curX, curY] != null &&
                _puzzles[curX, curY].puzzleState == PuzzleState.Idle
            );

            _puzzles[curX, curY].puzzleState = PuzzleState.Swapping;
            
            Vector3 currentPos = po.transform.position;
            _puzzles[curX, curY] = null;
            Destroy(po.gameObject);

            int ranIdx = Random.Range(2, 4); 
            GameObject newPuzzle = Instantiate(specialPuzzlePrefabs[ranIdx], puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject puzzle = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = puzzle;
            puzzle.Init(this, curX, curY);
            puzzle.puzzleState = PuzzleState.Idle;
            puzzle.isMatched = false;
            
            Tween t =  newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine);
            yield return t.WaitForCompletion();
            
            AddTask(() => MatchPuzzle());
        }

        #endregion
        
        /// <summary>
        /// 추가 타일 관련 함수
        /// </summary>
        #region Extra Tile
        private void SetGoldTile(GameObject puzzle)
        {
            GameObject gold = Instantiate(goldPrefab, puzzle.transform.position + new Vector3(0.2f, -0.2f, 0), Quaternion.identity, puzzle.transform);
            gold.name = "Gold";
        }

        private void SetSwapTile(GameObject puzzle)
        {
            GameObject swap = Instantiate(swapPrefab, puzzle.transform.position + new Vector3(0.2f, -0.2f, 0), Quaternion.identity, puzzle.transform);
            swap.name = "Swap";
        }

        private bool TryGetGoldTile(NormalPuzzleObject puzzle, out GameObject gold)
        {
            Transform goldTr = puzzle.transform.Find("Gold");
            
            if (goldTr)
            {
                gold = goldTr.gameObject;
                return true;
            }
            
            gold = null;
            return false;
        }

        private bool TryGetSwapTile(NormalPuzzleObject puzzle)
        {
            Transform swapTr = puzzle.transform.Find("Swap");

            return swapTr;
        }

        private void GoldMoveEffect(GameObject gold)
        {
            Transform tr = gold.transform;
            Vector3 scale = tr.localScale;
            Sequence seq = DOTween.Sequence();

            tr.SetParent(puzzleFrame.parent);
            Tween t1 = tr.DOMove(goldUI.GoldImagePos, 0.4f)
                .SetEase(Ease.OutSine);
            Tween t2 = tr.DOScale(scale * 1.2f, 0.2f)
                .SetEase(Ease.OutSine)
                .SetLoops(2, LoopType.Yoyo);

            seq.Join(t1).Join(t2);

            seq.OnComplete(() =>
            {
                GoldManager.Instance.AdjustGold(1);
                SoundManager.Instance.PlaySFX(SFX.SFX7_ConsumGoldTile);
                goldUI.AddGoldEffect();
                  
                Destroy(tr.gameObject);
            });
        }
        #endregion
    }
}
