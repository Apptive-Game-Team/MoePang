using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.Serialization;
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
        Bomb3X3,
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
        
        private PuzzleObject[,] _puzzles;
        private bool _isProcessing;

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
                    puzzle.name = $"Puzzle({j},{i})";
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
            GameObject puzzle;
            
            if (Random.Range(0, 100) > normalProbability)
            {
                if (Random.Range(0, 100) < specialProbability)
                {
                    var values = Enum.GetValues(typeof(SpecialPuzzleType));
                    var randomType = (SpecialPuzzleType)values.GetValue(Random.Range(0, values.Length));
                    puzzle = Instantiate(specialPuzzlePrefabs[(int)randomType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
                    
                    if (randomType == SpecialPuzzleType.ColorBomb)
                    {
                        var idx = Enum.GetValues(typeof(NormalPuzzleType)).GetValue(Random.Range(0, values.Length));
                        puzzle.GetComponent<SpecialPuzzleObject>().colorBombType = (NormalPuzzleType)idx;
                        puzzle.GetComponent<Image>().sprite = normalPuzzleImages[(int)idx];
                    }
                }
                else
                {
                    var values = Enum.GetValues(typeof(ObstaclePuzzleType));
                    var randomType = (ObstaclePuzzleType)values.GetValue(Random.Range(0, values.Length));
                    puzzle = Instantiate(obstaclePuzzlePrefabs[(int)randomType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
                    
                    var randomNormalType = (NormalPuzzleType)Enum.GetValues(typeof(NormalPuzzleType)).GetValue(Random.Range(0, Enum.GetValues(typeof(NormalPuzzleType)).Length));
                    puzzle.GetComponent<ObstaclePuzzleObject>().normalPuzzleType = randomNormalType;

                    if (randomType == ObstaclePuzzleType.Fixed)
                    {
                        puzzle.GetComponent<Image>().sprite = normalPuzzleImages[(int)randomNormalType];
                    }
                }
            }
            else
            {
                int randomType = (int)GetValidRandomType(col, row);
                puzzle = Instantiate(normalPuzzlePrefabs[randomType], CalculateDropPos(col, row), Quaternion.identity, puzzleFrame);
            }

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
            
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
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
            }

            _isProcessing = false;
        }

        private bool CheckAnyMatches()
        {
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
                        _puzzles[i + 1, j].isMatched = true;
                        _puzzles[i + 2, j].isMatched = true;
                        CheckAnySpecialPuzzle(i, j);
                        CheckAnySpecialPuzzle(i + 1, j);
                        CheckAnySpecialPuzzle(i + 2, j);
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
                        _puzzles[i, j + 1].isMatched = true;
                        _puzzles[i, j + 2].isMatched = true;
                        CheckAnySpecialPuzzle(i, j);
                        CheckAnySpecialPuzzle(i, j + 1);
                        CheckAnySpecialPuzzle(i, j + 2);
                        hasMatch = true;
                    }
                }
            }

            return hasMatch;
        }
        
        private void CheckAnySpecialPuzzle(int i, int j)
        {
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int d = 0; d < 4; d++)
            {
                int ni = i + dx[d];
                int nj = j + dy[d];
                
                if (ni >= 0 && ni < x && nj >= 0 && nj < y)
                {
                    if (_puzzles[ni, nj].puzzleType != PuzzleType.Normal &&
                        _puzzles[ni, nj] is not ObstaclePuzzleObject { obstaclePuzzleType: ObstaclePuzzleType.Fixed })
                    {
                        _puzzles[ni, nj].isMatched = true;
                    }
                }
            }
        }
        
        private IEnumerator MatchPuzzle()
        {
            Sequence seq = DOTween.Sequence();
            
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
                        
                        if (_puzzles[i, j] is SpecialPuzzleObject sp)
                        {
                            SpecialMatch(i, j, sp.specialPuzzleType);
                        }
                    }
                }
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].isMatched)
                    {
                        int row = i, col = j;
                        Tween t = _puzzles[row, col].transform.DOScale(0, 0.2f)
                            .OnComplete(() =>
                            {
                                // todo : 매치 시 유닛 소환 효과 함수 추가
                                Destroy(_puzzles[row, col].gameObject);
                                _puzzles[row, col] = null;
                            });
                        seq.Join(t);
                    }
                }
            }
            
            yield return seq.WaitForCompletion();
            yield return new WaitForSeconds(0.2f);
            
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

                                _puzzles[i, j].gameObject.name = $"Puzzle({i},{j})";
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
                        puzzle.name = $"Puzzle({i},{j})";
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
        
        #region Abnormal Puzzle Effect
        private void SpecialMatch(int curX, int curY, SpecialPuzzleType type)
        {
            switch(type)
            {
                case SpecialPuzzleType.Bomb3X3:
                    Bomb3X3Match(curX, curY);
                    break;
                case SpecialPuzzleType.ColumnBomb:
                    ColumnBombMatch(curX);
                    break;
                case SpecialPuzzleType.RowBomb:
                    RowBombMatch(curY);
                    break;
                case SpecialPuzzleType.CrossBomb:
                    CrossBombMatch(curX, curY);
                    break;
                case SpecialPuzzleType.ColorBomb:
                    ColorBombMatch(curX, curY);
                    break;
            }
        }

        private void Bomb3X3Match(int curX, int curY)
        {
            int startX = Mathf.Max(0, curX - 1);
            int endX = Mathf.Min(x - 1, curX + 1);
    
            int startY = Mathf.Max(0, curY - 1);
            int endY = Mathf.Min(y - 1, curY + 1);
            
            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    if (_puzzles[i, j] != null && !_puzzles[i, j].isMatched)
                    {
                        _puzzles[i, j].isMatched = true;
                    }
                }
            }
        }

        private void ColumnBombMatch(int curX)
        {
            for (int j = 0; j < y; j++)
            {
                if (_puzzles[curX, j] != null && !_puzzles[curX, j].isMatched)
                {
                    _puzzles[curX, j].isMatched = true;
                }
            }
        }

        private void RowBombMatch(int curY)
        {
            for (int i = 0; i < x; i++)
            {
                if (_puzzles[i, curY] != null && !_puzzles[i, curY].isMatched)
                {
                    _puzzles[i, curY].isMatched = true;
                }
            }
        }

        private void CrossBombMatch(int curX, int curY)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (i != curX && j != curY)
                    {
                        continue;
                    }
                    
                    if (_puzzles[i, j] != null && !_puzzles[i, j].isMatched)
                    {
                        _puzzles[i, j].isMatched = true;
                    }
                }
            }
        }

        private void ColorBombMatch(int curX, int curY)
        {
            var normalType = _puzzles[curX, curY].GetComponent<SpecialPuzzleObject>().colorBombType;

            for (int i = 0; i < x - 1; i++)
            {
                for (int j = 0; j < y - 1; j++)
                {
                    if (_puzzles[i, j] != null && !_puzzles[i, j].isMatched &&
                        _puzzles[i, j].puzzleType == PuzzleType.Normal && 
                        (NormalPuzzleType)_puzzles[i, j].GetPuzzleSubType() == normalType)
                    {
                        _puzzles[i, j].isMatched = true;
                    }
                }
            }
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
            newPuzzle.name = $"Puzzle({curX},{curY})";
            newPuzzle.transform.localScale = Vector3.zero;
            
            PuzzleObject po = newPuzzle.GetComponent<PuzzleObject>();
            _puzzles[curX, curY] = po;
            po.Init(this, curX, curY);
            po.isMatched = false;
            
            newPuzzle.transform.DOScale(0.8f, 0.2f);
        }
        #endregion
    }
}