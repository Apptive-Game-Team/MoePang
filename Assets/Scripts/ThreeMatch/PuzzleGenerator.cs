using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

namespace ThreeMatch
{
    public class PuzzleGenerator : MonoBehaviour
    {
        public enum PuzzleType
        {
            Flower,
            Leaf,
            Sand,
            Snow,
            Water,
        }
        
        [Header("Puzzle Settings")]
        [SerializeField] private RectTransform puzzleFrame;
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private float space;

        [Header("Puzzle Prefabs")]
        [SerializeField] private GameObject[] puzzlePrefabs;
        
        private PuzzleObject[,] _puzzles;
        private bool _isProcessing;

        private void Start()
        {
            StartCoroutine(GenerateBoard());
        }
        
        /// <summary>
        /// 시작 퍼즐 관련 메서드 (시작 시 매치가 안 일어나게 설정)
        /// </summary>
        /// <returns></returns>
        #region StartPuzzle
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
                    int randomType = (int)GetValidRandomType(j, i);
                    
                    GameObject puzzle = Instantiate(puzzlePrefabs[randomType], CalculateDropPos(j, i), Quaternion.identity, puzzleFrame);
                    
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

        private PuzzleType GetValidRandomType(int curX, int curY)
        {
            var types = Enum.GetValues(typeof(PuzzleType));
            var randomType = (PuzzleType)types.GetValue(Random.Range(0, types.Length));

            while (IsStartingMatch(curX, curY, randomType))
            {
                randomType = (PuzzleType)types.GetValue(Random.Range(0, types.Length));
            }

            return randomType;
        }

        private bool IsStartingMatch(int curX, int curY, PuzzleType type)
        {
            if (curX > 1)
            {
                if (_puzzles[curX - 1, curY].puzzleType == type && 
                    _puzzles[curX - 2, curY].puzzleType == type)
                {
                    return true;
                }
            }
            
            if (curY > 1)
            {
                if (_puzzles[curX, curY - 1].puzzleType == type && 
                    _puzzles[curX, curY - 2].puzzleType == type)
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
        #region PuzzlePosition
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
        /// 퍼즐을 옮겼을 때 완성 되는지 확인하는 메서드 및 퍼즐을 맞추고, 퍼즐이 사라지고, 내려오고, 채워지는 메서드
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        #region SwapAndMatchPuzzle
        public void TrySwapPuzzles(int x1, int y1, int x2, int y2)
        {
            if (_isProcessing) return;
            if (x2 < 0 || x2 >= x || y2 < 0 || y2 >= y) return;

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
                    PuzzleType currentType = _puzzles[i, j].puzzleType;
                    
                    if (_puzzles[i + 1, j].puzzleType == currentType && 
                        _puzzles[i + 2, j].puzzleType == currentType)
                    {
                        _puzzles[i, j].isMatched = true;
                        _puzzles[i + 1, j].isMatched = true;
                        _puzzles[i + 2, j].isMatched = true;
                        hasMatch = true;
                    }
                }
            }
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y - 2; j++)
                {
                    PuzzleType currentType = _puzzles[i, j].puzzleType;
                    
                    if (_puzzles[i, j + 1].puzzleType == currentType && 
                        _puzzles[i, j + 2].puzzleType == currentType)
                    {
                        _puzzles[i, j].isMatched = true;
                        _puzzles[i, j + 1].isMatched = true;
                        _puzzles[i, j + 2].isMatched = true;
                        hasMatch = true;
                    }
                }
            }

            return hasMatch;
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
                            if (_puzzles[i, k] != null)
                            {
                                _puzzles[i, j] = _puzzles[i, k];
                                _puzzles[i, k] = null;

                                Tween t = _puzzles[i, j].transform.DOMove(CalculatePos(i, j), 0.3f);
                                seq.Join(t);

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
                        int randomType = Random.Range(0, puzzlePrefabs.Length);
                        
                        GameObject puzzle = Instantiate(puzzlePrefabs[randomType], CalculateDropPos(i, j), Quaternion.identity, puzzleFrame);
                        
                        PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                        puzzle.name = $"Puzzle({j},{i})";
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
    }
}