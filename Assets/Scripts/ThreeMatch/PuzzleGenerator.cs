using System;
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
        
        private GameObject[,] _puzzles;

        private void Start()
        {
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            _puzzles = new GameObject[x, y];
            
            float offsetX = (x - 1) * space / 2f;
            float offsetY = (y - 1) * space / 2f;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    int randomType = (int)GetValidRandomType(j, i);
                    
                    Vector3 pos = new Vector3(j * space - offsetX, i * space - offsetY, 0f);
                    GameObject puzzle = Instantiate(puzzlePrefabs[randomType], puzzleFrame);
                    puzzle.transform.localPosition = pos;

                    puzzle.name = $"Puzzle({i},{j})";
                    _puzzles[j, i] = puzzle;
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
                if (GetPuzzleType(_puzzles[curX - 1, curY]) == type && 
                    GetPuzzleType(_puzzles[curX - 2, curY]) == type)
                {
                    return true;
                }
            }
            
            if (curY > 1)
            {
                if (GetPuzzleType(_puzzles[curX, curY - 1]) == type && 
                    GetPuzzleType(_puzzles[curX, curY - 2]) == type)
                {
                    return true;
                }
            }

            return false;
        }
        
        private PuzzleType GetPuzzleType(GameObject puzzle)
        {
            return puzzle.GetComponent<PuzzleObject>().puzzleType;
        }
    }
}