using _01.Scripts._01.ThreeMatch;
using System.Collections;
using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/RestartPuzzleConstraint", fileName = "RestartPuzzleConstraint")]
    public class RestartPuzzleConstraint : Constraint
    {
        [SerializeField] private float restartInterval;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("RestartPuzzleConstraint");
            context.Puzzle.StartCoroutine(RestartPuzzle(context.Puzzle));
        }

        private IEnumerator RestartPuzzle(PuzzleGenerator puzzle)
        {
            while (true)
            {
                yield return new WaitForSeconds(restartInterval);

                puzzle.AddTask(puzzle.ResetBoard);
                puzzle.AddTask(puzzle.GenerateBoard);
            }
        }
    }
}
