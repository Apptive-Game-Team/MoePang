using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ThreeMatch
{
    public enum PuzzleType
    {
        Normal,
        Special,
        Obstacle,
    }

    public enum NormalPuzzleType
    {
        Flower,
        Leaf,
        Sand,
        Snow,
        Water,
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
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private float space;
        [Range(0, 100)] [SerializeField] private float normalProbability;
        [Range(0, 100)] [SerializeField] private float specialProbability;
        
        [Header("Puzzle Prefabs")]
        [SerializeField] private GameObject[] normalPuzzlePrefabs;
        [SerializeField] private GameObject[] specialPuzzlePrefabs;
        [SerializeField] private GameObject[] obstaclePuzzlePrefabs;
        [SerializeField] private Sprite[] normalPuzzleImages;
        
        [Header("Spawn Settings")]
        [SerializeField] private UnitSpawner unitSpawner;

        [Header("Effect Settings")] 
        [SerializeField] private GameObject normalEffect;
        [SerializeField] private GameObject circleBombEffect;
        
        private PuzzleObject[,] _puzzles;
        
        private bool _isProcessing;
        public bool IsProcessing => _isProcessing;
            
        private Vector2Int _lastMovePos;
        private List<MatchGroup> _currentMatchGroups = new();
        
        private class MatchGroup
        {
            public List<Vector2Int> positions = new();
            public Vector2Int spawnPos; 
            public SpecialPuzzleType? resultType = null;
            public NormalPuzzleType color;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                SpawnObstaclePuzzle();
            }
        }

        private void Start()
        {
            StartCoroutine(GenerateBoard());
        }
        
        /// <summary>
        /// 시작 퍼즐 관련 함수 (시작 시 매치가 안 일어나게 설정)
        /// </summary>
        /// <returns></returns>
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
            
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    GameObject puzzle = SetRandomPuzzle(j, i);
                    
                    PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                    puzzle.name = $"Puzzle({j + 1},{i + 1})";
                    _puzzles[j, i] = po;
                    
                    Tween t = puzzle.transform.DOMove(CalculatePos(j, i), 0.3f);
                    seq.Join(t);
                    
                    po.Init(this, j, i);
                }
            }

            yield return seq.WaitForCompletion();
        }

        private GameObject SetRandomPuzzle(int col, int row)
        {
            int randomType = (int)GetValidRandomType(col, row);
            GameObject puzzle = Instantiate(normalPuzzlePrefabs[randomType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
            
            return puzzle;
        }

        private NormalPuzzleType GetValidRandomType(int curX, int curY)
        {
            var types = Enum.GetValues(typeof(NormalPuzzleType));
            var randomType = (NormalPuzzleType)types.GetValue(Random.Range(0, types.Length));

            while (IsStartingMatch(curX, curY, randomType))
            {
                randomType = (NormalPuzzleType)types.GetValue(Random.Range(0, types.Length));
            }

            return randomType;
        }

        private bool IsStartingMatch(int curX, int curY, NormalPuzzleType type)
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
            // normal <-> normal
            if (p1.puzzleType == p2.puzzleType)
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
                t1 = (int)op1.normalPuzzleType;
            }
            else if (p1.puzzleType == PuzzleType.Normal)
            {
                t1 = p1.GetPuzzleSubType();
            }

            if (p2 is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op2)
            {
                t2 = (int)op2.normalPuzzleType;
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

        private bool CheckNormalType(PuzzleObject p, NormalPuzzleType type)
        {
            if (p.puzzleType == PuzzleType.Normal)
            {
                if ((NormalPuzzleType)p.GetPuzzleSubType() == type)
                {
                    return true;
                }
            }
            else if (p is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op)
            {
                if (op.normalPuzzleType == type)
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
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        #region Puzzle Position
        private Vector3 CalculatePos(int col, int row)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;
            
            return new Vector3(col * space - offsetX, row * space - offsetY, 0f);
        }

        private Vector3 CalculateDropPos(int col, int row)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y + 3) * space / 2f;

            return new Vector3(col * space - offsetX, row * space + offsetY, 0f);
        }
        #endregion
        
        /// <summary>
        /// 퍼즐을 옮겼을 때 완성 되는지 확인하는 함수 및 퍼즐을 맞추고, 퍼즐이 사라지고, 내려오고, 채워지는 함수
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        #region Swap And Match Puzzle
        public void TrySwapPuzzles(int x1, int y1, int x2, int y2)
        {
            if (_isProcessing) return;
            if (x2 < 0 || x2 >= x || y2 < 0 || y2 >= y) return;
            
            if (_puzzles[x1, y1] is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } ||
                _puzzles[x2, y2] is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed })
            {
                return;
            }
            
            _lastMovePos = new Vector2Int(x2, y2);
            StartCoroutine(SwapAndCheck(x1, y1, x2, y2));
        }

        private IEnumerator SwapAndCheck(int x1, int y1, int x2, int y2)
        {
            _isProcessing = true;
            
            var p1 = _puzzles[x1, y1];
            var p2 = _puzzles[x2, y2];
    
            _puzzles[x1, y1] = p2;
            _puzzles[x2, y2] = p1;
            
            Vector3 pos1 = p1.transform.localPosition;
            Vector3 pos2 = p2.transform.localPosition;
            
            Sequence seq1 = DOTween.Sequence();
            Tween t1 = p1.transform.DOMove(pos2, 0.2f);
            Tween t2 = p2.transform.DOMove(pos1, 0.2f);
            seq1.Append(t1);
            seq1.Join(t2);
            
            yield return seq1.WaitForCompletion();
            
            p1.Init(this, x2, y2);
            p2.Init(this, x1, y1);

            _isProcessing = false;
            
            bool isSpecialPuzzleExist = false;
            if (_puzzles[x1, y1] is SpecialPuzzleObject sp1)
            {
                isSpecialPuzzleExist = true;
                yield return ActivateSpecialBomb(x1, y1, sp1.specialPuzzleType);
            }
            if (_puzzles[x2, y2] is SpecialPuzzleObject sp2)
            {
                isSpecialPuzzleExist = true;
                yield return ActivateSpecialBomb(x2, y2, sp2.specialPuzzleType);
            }

            _isProcessing = true;
            
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
            }
            else
            {
                if (!isSpecialPuzzleExist)
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
                }
            }

            _isProcessing = false;
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

            return true;
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
                    if (_puzzles[ni, nj].puzzleType == PuzzleType.Obstacle &&
                        _puzzles[ni, nj] is not ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed })
                    {
                        _puzzles[ni, nj].isMatched = true;
                    }
                }
            }
        }
        
        private MatchGroup GetMatchGroupBfs(int startX, int startY, bool[,] visited)
        {
            MatchGroup group = new();
            NormalPuzzleType color = (NormalPuzzleType)_puzzles[startX, startY].GetPuzzleSubType();
            group.color = color;

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
                            CheckNormalType(_puzzles[nx, ny], color))
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
            // 유저가 마지막으로 옮긴 위치가 그룹에 포함되면 그곳에서 생성, 아니면 그룹의 첫 번째 타일 위치
            group.spawnPos = group.positions.Contains(_lastMovePos) ? _lastMovePos : group.positions[0];
            
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
                // 가로로 4개 -> 세로 폭탄 (Column)
                group.resultType = SpecialPuzzleType.ColumnBomb;
            }
            else if (maxV == 4)
            {
                // 세로로 4개 -> 가로 폭탄 (Row)
                group.resultType = SpecialPuzzleType.RowBomb;
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
        
        private IEnumerator MatchPuzzle()
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].isMatched)
                    {
                        if (_puzzles[i, j] is ObstaclePuzzleObject op)
                        {
                            ObstacleMatch(i, j, op.obstaclePuzzleType);
                        }
                    }
                }
            }
            
            foreach (var group in _currentMatchGroups)
            {
                Vector3 destination = CalculatePos(group.spawnPos.x, group.spawnPos.y);
                
                Sequence seq = DOTween.Sequence();
                foreach (var pos in group.positions)
                {
                    if (group.resultType != null)
                    {
                        Tween t1 = _puzzles[pos.x, pos.y].transform.DOMove(destination, 0.2f);
                        seq.Join(t1);
                    }

                    SetEffect(_puzzles[pos.x, pos.y].transform.position, normalEffect);
                    
                    Tween t2 = _puzzles[pos.x, pos.y].transform.DOScale(0, 0.2f)
                        .OnComplete(() =>
                        {
                            Destroy(_puzzles[pos.x, pos.y].gameObject);
                            _puzzles[pos.x, pos.y] = null;
                        });
                    seq.Join(t2);
                }
                
                yield return seq.WaitForCompletion();

                if (group.resultType != null)
                {
                    GameObject newPuzzle = Instantiate(specialPuzzlePrefabs[(int)group.resultType], puzzleFrame);
                    newPuzzle.transform.localPosition = destination;
                    newPuzzle.name = $"Puzzle({destination.x + 1},{destination.y + 1})";
                    newPuzzle.transform.localScale = Vector3.zero;
            
                    PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
                    _puzzles[group.spawnPos.x, group.spawnPos.y] = po;
                    po.Init(this, group.spawnPos.x, group.spawnPos.y);
                    po.isMatched = false;
            
                    newPuzzle.transform.DOScale(0.8f, 0.2f);
                }

                unitSpawner.FriendlySpawn();
            }
            
            yield return new WaitForSeconds(0.3f);
            
            yield return DropBlocks();
        }
        
        private IEnumerator DropBlocks()
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] == null)
                    {
                        for (int k = j + 1; k < y; k++)
                        {
                            if (_puzzles[i, k] != null && 
                                _puzzles[i, k] is not ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed })
                            {
                                _puzzles[i, j] = _puzzles[i, k];
                                _puzzles[i, k] = null;

                                Tween t = _puzzles[i, j].transform.DOMove(CalculatePos(i, j), 0.3f);
                                seq.Join(t);

                                _puzzles[i, j].gameObject.name = $"Puzzle({i + 1},{j + 1})";
                                _puzzles[i, j].Init(this, i, j);

                                break;
                            }
                        }
                    }
                }
            }
            
            yield return seq.WaitForCompletion();
            yield return new WaitForSeconds(0.2f);
            
            yield return RefillBlocks();
        }
        
        private IEnumerator RefillBlocks()
        {
            Sequence seq = DOTween.Sequence();
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] == null)
                    {
                        GameObject puzzle = SetRandomPuzzle(i, j);
                        
                        PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                        puzzle.name = $"Puzzle({i + 1},{j + 1})";
                        _puzzles[i, j] = po;
                        
                        Tween t = po.transform.DOMove(CalculatePos(i, j), 0.3f);
                        seq.Join(t);
                        
                        po.Init(this, i, j);
                    }
                }
            }

            yield return seq.WaitForCompletion();
            yield return new WaitForSeconds(0.2f);
    
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
            }
        }
        #endregion
        
        /// <summary>
        /// 스페셜, 장애물 퍼즐들의 매치, 발동 로직 함수들
        /// </summary>
        /// <param name="curX"></param>
        /// <param name="curY"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        #region Abnormal Puzzle
        public IEnumerator ActivateSpecialBomb(int curX, int curY, SpecialPuzzleType type)
        {
            if (_isProcessing) yield break;
            _isProcessing = true;
            
            yield return SpecialMatch(curX, curY, type);
            
            yield return new WaitForSeconds(0.2f);
            
            yield return DropBlocks();

            _isProcessing = false;
        }
        
        private IEnumerator SpecialMatch(int curX, int curY, SpecialPuzzleType type)
        {
            if (_puzzles[curX, curY] == null)
            {
                yield break;
            }

            List<Vector2Int> targets = GetExplosionRange(curX, curY, type);
            
            if (_puzzles[curX, curY] is SpecialPuzzleObject { specialPuzzleType: SpecialPuzzleType.CircleBomb })
            {
                Debug.Log("Bomb");
                SetEffect(_puzzles[curX, curY].transform.position, circleBombEffect);
            }
            
            GameObject self = _puzzles[curX, curY].gameObject;
            _puzzles[curX, curY] = null;
            
            self.transform.DOScale(0, 0.1f).OnComplete(() => Destroy(self));

            Sequence seq = DOTween.Sequence();
            Queue<(SpecialPuzzleObject, Vector2Int)> q = new();
            
            foreach (var pos in targets)
            {
                if (_puzzles[pos.x, pos.y] == null) continue;

                PuzzleObject targetPuzzle = _puzzles[pos.x, pos.y];
                
                if (targetPuzzle is SpecialPuzzleObject nextSp)
                {
                    q.Enqueue((nextSp, new Vector2Int(pos.x, pos.y)));
                }
                else
                {
                    SetEffect(_puzzles[pos.x, pos.y].transform.position, normalEffect);
                    _puzzles[pos.x, pos.y] = null;
                    Tween t = targetPuzzle.transform.DOScale(0, 0.15f)
                        .OnComplete(() => Destroy(targetPuzzle.gameObject));
                    seq.Join(t);
                }
            }
            
            yield return seq.WaitForCompletion();
            
            unitSpawner.FriendlySpawn();

            while (q.Count > 0)
            {
                var sp = q.Dequeue();
                yield return DelayedSpecialMatch(sp.Item2.x, sp.Item2.y, sp.Item1.specialPuzzleType);
            }
        }

        private IEnumerator DelayedSpecialMatch(int curX, int curY, SpecialPuzzleType type, float delay = 0.2f)
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
                    CircleBombMatch(curX, curY, range);
                    break;
                case SpecialPuzzleType.ColumnBomb:
                    ColumnBombMatch(curX,range);
                    break;
                case SpecialPuzzleType.RowBomb:
                    RowBombMatch(curY, range);
                    break;
                case SpecialPuzzleType.CrossBomb:
                    CrossBombMatch(curX, curY, range);
                    break;
                case SpecialPuzzleType.ColorBomb:
                    ColorBombMatch(curX, curY, range);
                    break;
            }
            return range;
        }

        private void CircleBombMatch(int curX, int curY, List<Vector2Int> list)
        {
            for (int i = curX - 2; i <= curX + 2; i++)
            {
                for (int j = curY - 2; j <= curY + 2; j++)
                {
                    if (i < 0 || i >= x || j < 0 || j >= y) continue;
                    
                    bool isCorner = (i == curX - 2 || i == curX + 2) && (j == curY - 2 || j == curY + 2);
            
                    if (!isCorner)
                    {
                        list.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        private void ColumnBombMatch(int curX, List<Vector2Int> list)
        {
            for (int j = 0; j < y; j++)
            {
                list.Add(new Vector2Int(curX, j));
            }
        }

        private void RowBombMatch(int curY, List<Vector2Int> list)
        {
            for (int i = 0; i < x; i++)
            {
                list.Add(new Vector2Int(i, curY));
            }
        }

        private void CrossBombMatch(int curX, int curY, List<Vector2Int> list)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (i != curX && j != curY)
                    {
                        continue;
                    }
                    
                    list.Add(new Vector2Int(i, j));
                }
            }
        }

        private void ColorBombMatch(int curX, int curY, List<Vector2Int> list)
        {
            var normalType = _puzzles[curX, curY].GetComponent<SpecialPuzzleObject>().colorBombType;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].puzzleType == PuzzleType.Normal && 
                        (NormalPuzzleType)_puzzles[i, j].GetPuzzleSubType() == normalType)
                    {
                        list.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        public void SpawnObstaclePuzzle()
        {
            StartCoroutine(SpawnObstaclePuzzleCoroutine());
        }
        
        private IEnumerator SpawnObstaclePuzzleCoroutine()
        {
            yield return new WaitUntil(() => !_isProcessing);
            _isProcessing = true;

            List<PuzzleObject> list = new();
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j].puzzleType == PuzzleType.Normal)
                    {
                        list.Add(_puzzles[i, j]);
                    }
                }
            }
            
            var target = list[Random.Range(0, list.Count)];

            Vector3 currentPos = target.transform.localPosition;
            int col = target.column, row = target.row;
            var type = (NormalPuzzleType)_puzzles[col, row].GetPuzzleSubType();
            _puzzles[col, row] = null;
            Destroy(target.gameObject);

            var values = Enum.GetValues(typeof(ObstaclePuzzleType));
            int idx = (int)values.GetValue(Random.Range(0, values.Length));
            
            GameObject newPuzzle = Instantiate(obstaclePuzzlePrefabs[idx], puzzleFrame);
            newPuzzle.transform.localPosition = currentPos;
            newPuzzle.name = $"Puzzle({col + 1},{row + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[col, row] = po;
            po.Init(this, col, row);
            po.isMatched = false;

            if (po is ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed } op)
            {
                op.normalPuzzleType = type;
                newPuzzle.GetComponent<Image>().sprite = normalPuzzleImages[(int)type];
            }
            
            newPuzzle.transform.DOScale(0.6f, 0.2f);
            
            _isProcessing = false;
        }

        private void ObstacleMatch(int curX, int curY, ObstaclePuzzleType type)
        {
            switch (type)
            {
                case ObstaclePuzzleType.DeActivated:
                    DeActivatedMatch(curX, curY);
                    break;
            }
        }

        private void DeActivatedMatch(int curX, int curY)
        {
            var obstacleObj = _puzzles[curX, curY].GetComponent<ObstaclePuzzleObject>();
            NormalPuzzleType targetType = obstacleObj.normalPuzzleType;
            Vector3 currentPos = _puzzles[curX, curY].transform.localPosition;
            
            GameObject oldObject = _puzzles[curX, curY].gameObject;
            _puzzles[curX, curY] = null;
            Destroy(oldObject);
            
            GameObject newPuzzle = Instantiate(normalPuzzlePrefabs[(int)targetType], puzzleFrame);
            newPuzzle.transform.localPosition = currentPos;
            newPuzzle.name = $"Puzzle({curX + 1},{curY + 1})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = po;
            po.Init(this, curX, curY);
            po.isMatched = false;
            
            newPuzzle.transform.DOScale(0.6f, 0.2f);
        }
        #endregion
        
        private void SetEffect(Vector3 pos, GameObject effect)
        {
            GameObject go = Instantiate(effect, pos,  Quaternion.identity);
            var effects = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var e in effects)
            {
                e.Play();
            }
            Destroy(go, 0.7f);
        }
    }
}