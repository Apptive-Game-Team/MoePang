using _01.Scripts._04.UI.InGame;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    }
    
    public class PuzzleGenerator : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] private RectTransform puzzleFrame;
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
        [SerializeField] private float obstacleSpawnInterval = 10f;
        [Range(0, 100)] [SerializeField] private float goldTileSpawnRate = 5f;
        
        
        [Header("Puzzle Prefabs")]
        [SerializeField] private GameObject[] normalPuzzlePrefabs;
        [SerializeField] private GameObject[] specialPuzzlePrefabs;
        [SerializeField] private GameObject[] obstaclePuzzlePrefabs;
        [SerializeField] private Sprite[] normalPuzzleImages;
        [SerializeField] private GameObject[] specialPuzzleParticlePrefabs;
        [SerializeField] private GameObject obstacleWarningPrefab;
        [SerializeField] private GameObject goldPrefab;
        
        [Header("Spawn Settings")] 
        [SerializeField] private SpawnStackManager spawnStackManager;
        
        private PuzzleObject[,] _puzzles;
        
        private bool _isProcessing;
        public bool IsProcessing => _isProcessing;
            
        private Vector2Int _lastMovePos;
        private List<MatchGroup> _currentMatchGroups = new();
        private Queue<Func<IEnumerator>> _taskQueue = new();
        private HashSet<Vector2Int> _movedPositions = new();

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
            public ObstaclePuzzleType type;
            public Vector2Int pos;
        }

        [Serializable]
        private struct ObstacleWeight
        {
            public ObstaclePuzzleType type;
            [Range(0, 100)] public int weight;
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
        /// 시작 퍼즐 관련 함수 (시작 시 매치가 안 일어나게 설정)
        /// </summary>
        #region Start Puzzle
        private IEnumerator GenerateBoard()
        {
            _isProcessing = true;
            
            _puzzles = new PuzzleObject[x, y];
            yield return SetStartPuzzle();
            
            _isProcessing = false;
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
                    
                    float distance = Vector3.Distance(po.transform.position, targetPos);
                    float duration = distance / dropSpeed;
                    float startAt = columnDropDelay * i + rowDropDelay * j;
                    
                    Tween fallTween = po.transform.DOMove(targetPos, duration)
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
        }

        private GameObject SetStartRandomPuzzle(int col, int row)
        {
            bool isObstacle = false;
            ObstaclePuzzleType obstacleType = ObstaclePuzzleType.DeActivated;

            foreach (var data in startObstacles[0].obstacles)
            {
                if (data.pos.x == col && data.pos.y == row)
                {
                    isObstacle = true;
                    obstacleType = data.type;
                }
            }

            GameObject puzzle;
            if (isObstacle)
            {
                puzzle = Instantiate(obstaclePuzzlePrefabs[(int)obstacleType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
                PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                Habitat randomType = GetValidRandomType(col, row);
                switch (po)
                {
                    case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.DeActivated } op:
                        op.habitat = randomType;
                        break;
                    case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op:
                        op.habitat = randomType;
                        puzzle.GetComponent<Image>().sprite = normalPuzzleImages[(int)randomType];
                        break;
                }
            }
            else
            {
                Habitat randomType = GetValidRandomType(col, row);
                puzzle = Instantiate(normalPuzzlePrefabs[(int)randomType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
                
                float prob = Random.Range(0, 100f);
                if (prob < goldTileSpawnRate)
                {
                    SetGoldTile(puzzle);
                }
            }
            
            return puzzle;
        }

        private GameObject SetRandomPuzzle(int col, int row, int spawnOrder)
        {
            var types = Enum.GetValues(typeof(Habitat));
            var randomType = (Habitat)types.GetValue(Random.Range(0, types.Length));
            
            Vector3 startPos = CalculateDropPos(col, spawnOrder);
            GameObject puzzle = Instantiate(normalPuzzlePrefabs[(int)randomType], startPos, Quaternion.identity, puzzleFrame);
            puzzle.name = $"Puzzle({col + 1}, {row + 1})"; 
            
            float prob = Random.Range(0, 100f);
            if (prob < goldTileSpawnRate)
            {
                SetGoldTile(puzzle);
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
            
            // normal <-> normal
            if (p1.puzzleType  == PuzzleType.Normal && p2.puzzleType == PuzzleType.Normal)
            {
                if (p1.GetPuzzleSubType() == p2.GetPuzzleSubType())
                {
                    return true;
                }
            }

            // (fixed or normal) <-> (fixed or normal)
            int t1 = -1, t2 = -1;
            
            if (p1 is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op1)
            {
                t1 = (int)op1.habitat;
            }
            else if (p1.puzzleType == PuzzleType.Normal)
            {
                t1 = p1.GetPuzzleSubType();
            }

            if (p2 is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op2)
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
            if (p.puzzleType == PuzzleType.Normal)
            {
                if ((Habitat)p.GetPuzzleSubType() == type)
                {
                    return true;
                }
            }
            else if (p is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op)
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
            
            return new Vector3(puzzleFrame.transform.position.x + col * space - offsetX
                ,puzzleFrame.transform.position.y + row * space - offsetY, 0f);
        }

        private Vector3 CalculateDropPos(int col, int spawnOrder)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;
            
            float spawnY = (y + spawnOrder) * space - offsetY;

            return new Vector3(puzzleFrame.transform.position.x + col * space - offsetX
                ,puzzleFrame.transform.position.y + spawnY, 0f);
        }
        #endregion
        
        /// <summary>
        /// 퍼즐을 옮겼을 때 완성 되는지 확인하는 함수 및 퍼즐을 맞추고, 퍼즐이 사라지고, 내려오고, 채워지는 함수
        /// </summary>
        #region Swap And Match Puzzle
        public void TrySwapPuzzles(int x1, int y1, int x2, int y2)
        {
            if (_taskQueue.Count > 0 || _isProcessing) return;
            if (x2 < 0 || x2 >= x || y2 < 0 || y2 >= y) return;
            
            if (_puzzles[x1, y1] is ObstaclePuzzleObject ||
                _puzzles[x2, y2] is ObstaclePuzzleObject)
            {
                _puzzles[x1, y1].FailedSwapEffect(x2 - x1, y2 - y1, 
                    Vector2.Distance(_puzzles[x1, y1].transform.position, _puzzles[x2, y2].transform.position) / 2);
                return;
            }
            
            _lastMovePos = new Vector2Int(x2, y2);
            AddTask(() => SwapAndCheck(x1, y1, x2, y2));
        }

        private IEnumerator SwapAndCheck(int x1, int y1, int x2, int y2)
        {
            _movedPositions.Clear();
            _movedPositions.Add(new Vector2Int(x1, y1));
            _movedPositions.Add(new Vector2Int(x2, y2));
            
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
                        CheckType(_puzzles[i, j], _puzzles[i + 2, j]))
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
                        CheckType(_puzzles[i, j], _puzzles[i, j + 2]))
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

            for (int d = 0; d < 4; d++)
            {
                int ni = i + dx[d];
                int nj = j + dy[d];

                if (ni >= 0 && ni < x && nj >= 0 && nj < y)
                {
                    if (_puzzles[ni, nj] is ObstaclePuzzleObject op &&
                        _puzzles[ni, nj] is not ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed })
                    {
                        op.isTriggered = true;
                    }
                }
            }
        }
        
        private MatchGroup GetMatchGroupBfs(int startX, int startY, bool[,] visited)
        {
            MatchGroup group = new();
            Habitat habitat = _puzzles[startX, startY] is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op ?
                op.habitat : (Habitat)_puzzles[startX, startY].GetPuzzleSubType();
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

                    if (nx >= 0 && nx < x && ny >= 0 && ny < y && !visited[nx, ny])
                    {
                        if (_puzzles[nx, ny] != null && _puzzles[nx, ny].isMatched && 
                            CheckNormalType(_puzzles[nx, ny], habitat))
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
            return group;
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
            foreach (var group in _currentMatchGroups)
            {
                foreach (var pos in group.positions)
                {
                    if (_puzzles[pos.x, pos.y] != null)
                    {
                        _puzzles[pos.x, pos.y].puzzleState = PuzzleState.Matching;
                    }
                }
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null)
                    {
                        if (_puzzles[i, j] is ObstaclePuzzleObject { isTriggered: true } op)
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
                }
                yield return seq1.WaitForCompletion();
                
                Sequence seq2 = DOTween.Sequence();
                List<PuzzleObject> targets = new();
                foreach (var pos in group.positions)
                {
                    var targetPuzzle = _puzzles[pos.x, pos.y];
                    targets.Add(targetPuzzle);
                    if (targetPuzzle == null) continue;
                    
                    _puzzles[pos.x, pos.y] = null;

                    if (group.resultType != null)
                    {
                        Tween t1 = targetPuzzle.transform.DOMove(destination, 0.2f);
                        seq2.Join(t1);
                    }

                    if (targetPuzzle is NormalPuzzleObject no && TryGetGoldTile(no, out GameObject gold))
                    {
                        GoldMoveEffect(gold);
                    }
                    
                    Tween t2 = targetPuzzle.transform.DOScale(tileScale / 3, 0.2f).SetEase(Ease.InSine);
                    seq2.Join(t2);
                }
                yield return seq2.WaitForCompletion();
                
                foreach (PuzzleObject targetPuzzle in targets)
                {
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

                if (group.resultType != null)
                {
                    GameObject newPuzzle = Instantiate(specialPuzzlePrefabs[(int)group.resultType], puzzleFrame);
                    newPuzzle.transform.position = destination;
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

            for (int i = 0; i < x; i++)
            {
                int spawnOrder = 0;

                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] == null)
                    {
                        PuzzleObject targetPo = null;
                        bool foundUpperTile = false;
                        
                        for (int k = j + 1; k < y; k++)
                        {
                            if (_puzzles[i, k] != null)
                            {
                                _puzzles[i, k].puzzleState = PuzzleState.Falling;
                                _puzzles[i, j] = _puzzles[i, k];
                                _puzzles[i, k] = null;
                                targetPo = _puzzles[i, j];
                                foundUpperTile = true;
                                break;
                            }
                        }
                        
                        if (!foundUpperTile)
                        {
                            GameObject puzzle = SetRandomPuzzle(i, j, spawnOrder);
                            spawnOrder++;
                            targetPo = puzzle.GetComponent<PuzzleObject>();
                            targetPo.puzzleState = PuzzleState.Falling;
                            _puzzles[i, j] = targetPo;
                        }

                        if (targetPo != null)
                        {
                            targetPo.gameObject.name = $"Puzzle({i + 1},{j + 1})";
                            targetPo.Init(this, i, j);
                            _movedPositions.Add(new Vector2Int(i, j));

                            Vector3 targetPos = CalculatePos(i, j);
                            
                            float distance = Vector3.Distance(targetPo.transform.position, targetPos);
                            float duration = distance / dropSpeed;
                            float startAt = columnDropDelay * i + rowDropDelay * j;
                            
                            Tween fallTween = targetPo.transform.DOMove(targetPos, duration)
                                .SetEase(Ease.InSine)
                                .OnComplete(() => 
                                {
                                    targetPo.transform.DOPunchPosition(Vector3.down * 0.05f, 0.15f, 8)
                                        .OnComplete(() =>
                                        {
                                            targetPo.puzzleState = PuzzleState.Idle;
                                        });
                                });

                            seq.Insert(startAt, fallTween);
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
                if (_puzzles[pos.x, pos.y] == null) continue;

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
            
            if (po is NormalPuzzleObject no2 && TryGetGoldTile(no2, out GameObject gold))
            {
                GoldMoveEffect(gold);
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

        private IEnumerator SpawnObstaclePuzzle()
        {
            while (true)
            {
                yield return new WaitForSeconds(obstacleSpawnInterval);
                Vector2Int pos = SetObstacleSpawnPos();
                yield return SpawnObstacleWarning(pos.x, pos.y);
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
            
            PuzzleObject target = list[Random.Range(0, list.Count)];

            return new Vector2Int(target.column, target.row);
        }

        private IEnumerator SpawnObstacleWarning(int curX, int curY)
        {
            Vector2 pos = CalculatePos(curX, curY);
            GameObject warningOb = Instantiate(obstacleWarningPrefab, pos, Quaternion.identity, puzzleFrame);
            yield return warningOb.GetComponent<SpriteRenderer>().DOFade(0.1f, obstacleSpawnDelay / 4)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    Destroy(warningOb);
                })
                .WaitForCompletion();
        }
        
        private IEnumerator SpawnRandomObstaclePuzzleCoroutine(int curX, int curY)
        {
            yield return new WaitUntil(() =>
                _puzzles[curX, curY] != null &&
                _puzzles[curX, curY].puzzleState == PuzzleState.Idle
            );
            
            PuzzleObject target = _puzzles[curX, curY];

            if (target is ObstaclePuzzleObject)
            {
                yield break;
            }

            Vector3 currentPos = target.transform.position;
            Habitat type = (Habitat)_puzzles[curX, curY].GetPuzzleSubType();
            _puzzles[curX, curY] = null;
            Destroy(target.gameObject);
            
            GameObject newPuzzle = Instantiate(obstaclePuzzlePrefabs[(int)GetWeightedRandomObstacle()], puzzleFrame);
            newPuzzle.transform.position = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = po;
            po.Init(this, curX, curY);
            po.isMatched = false;

            switch (po)
            {
                case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.DeActivated } op:
                    op.habitat = type;
                    break;
                case ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op:
                    op.habitat = type;
                    newPuzzle.GetComponent<Image>().sprite = normalPuzzleImages[(int)type];
                    break;
            }
            
            Tween t =  newPuzzle.transform.DOScale(tileScale, 0.2f)
                .SetEase(Ease.InSine);
            yield return t.WaitForCompletion();
        }
        
        private ObstaclePuzzleType GetWeightedRandomObstacle()
        {
            int totalWeight = 0;
            foreach (var obstacle in obstacleWeights)
            {
                totalWeight += obstacle.weight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            
            int currentSum = 0;
            foreach (var obstacle in obstacleWeights)
            {
                currentSum += obstacle.weight;
                if (randomValue < currentSum)
                {
                    return obstacle.type;
                }
            }
            
            return ObstaclePuzzleType.DeActivated;
        }

        private IEnumerator ObstacleMatch(int curX, int curY, ObstaclePuzzleType type)
        {
            switch (type)
            {
                case ObstaclePuzzleType.DeActivated:
                    yield return DeActivatedMatch(curX, curY);
                    break;
                case ObstaclePuzzleType.Fixed:
                    break;
            }
        }

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
                .SetEase(Ease.InSine);

            yield return null;
        }
        #endregion
        
        /// <summary>
        /// 골드 관련 함수
        /// </summary>
        #region Gold
        private void SetGoldTile(GameObject puzzle)
        {
            GameObject gold = Instantiate(goldPrefab, puzzle.transform.position + new Vector3(0.2f, -0.2f, 0), Quaternion.identity, puzzle.transform);
            gold.name = "Gold";
        }

        private bool TryGetGoldTile(NormalPuzzleObject puzzle, out GameObject gold)
        {
            Transform goldTr = puzzle.transform.Find("Gold");
            
            if (goldTr != null)
            {
                gold = goldTr.gameObject;
                return true;
            }
            
            gold = null;
            return false;
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
                GoldManager.Instance.AddGold(1);
                goldUI.AddGoldEffect();
                 
                Destroy(tr.gameObject);
            });
        }
        #endregion
    }
}