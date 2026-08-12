using System;
using System.Collections;
using UnityEngine;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class InfectionPuzzleObject : ObstaclePuzzleObject
    {
        [SerializeField] private float infectionInterval;
        private PuzzleGenerator _generator;
        private const int MaxCount = 8;
        private static int _currentCount;
        
        public static bool CanCreate()
        {
            return _currentCount < MaxCount;
        }

        public void Init(PuzzleGenerator generator)
        {
            _generator = generator;
            _currentCount++;

            StartCoroutine(InfectCoroutine());
        }

        private IEnumerator InfectCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(infectionInterval);

                if (_currentCount < MaxCount)
                {
                    yield return _generator.Infect(this);
                }
            }
        }

        private void OnDestroy()
        {
            _currentCount--;
        }
    }
}
