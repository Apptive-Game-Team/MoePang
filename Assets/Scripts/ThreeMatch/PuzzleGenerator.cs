using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ThreeMatch
{
    public class PuzzleGenerator : MonoBehaviour
    {
        public enum PuzzleType
        {
            White,
            Red,
            Blue,
            Green,
            Purple,
            Yellow,
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
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            _puzzles = new PuzzleObject[x, y];
            
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    int randomType = (int)GetValidRandomType(j, i);
                    
                    GameObject puzzle = Instantiate(puzzlePrefabs[randomType], puzzleFrame);
                    PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                    
                    po.Init(this, j, i);
                    puzzle.transform.localPosition = CalculatePos(j, i);
                    puzzle.name = $"Puzzle({j},{i})";
                    _puzzles[j, i] = po;
                }
            }
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
        
        private Vector3 CalculatePos(int i, int j)
        {
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;
            
            return new Vector3(i * space - offsetX, j * space - offsetY, 0f);
        }
        
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
            
            // todo : 부드러운 움직임 추가
            Vector3 pos1 = p1.transform.localPosition;
            Vector3 pos2 = p2.transform.localPosition;
            p1.transform.localPosition = pos2;
            p2.transform.localPosition = pos1;
            
            p1.Init(this, x2, y2);
            p2.Init(this, x1, y1);
            
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
                _puzzles[x1, y1] = p1;
                _puzzles[x2, y2] = p2;
                p1.transform.localPosition = pos1;
                p2.transform.localPosition = pos2;
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
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] != null && _puzzles[i, j].isMatched)
                    {
                        Destroy(_puzzles[i, j].gameObject);
                        _puzzles[i, j] = null;
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
            
            yield return DropBlocks();
        }
        
        private IEnumerator DropBlocks()
        {
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
                                
                                _puzzles[i, j].transform.localPosition = CalculatePos(i, j);
                                
                                _puzzles[i, j].Init(this, i, j);
                                
                                break;
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
            
            yield return RefillBlocks();
        }
        
        private IEnumerator RefillBlocks()
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (_puzzles[i, j] == null)
                    {
                        int randomType = Random.Range(0, puzzlePrefabs.Length);
                        
                        GameObject puzzle = Instantiate(puzzlePrefabs[randomType], puzzleFrame);
                        PuzzleObject po = puzzle.GetComponent<PuzzleObject>();
                        
                        po.Init(this, i, j);
                        po.transform.localPosition = CalculatePos(i, j);
                        puzzle.name = $"Puzzle({i},{j})";
                        _puzzles[i, j] = po;
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);
            
            if (CheckAnyMatches())
            {
                yield return MatchPuzzle();
            }
        }
    }
}