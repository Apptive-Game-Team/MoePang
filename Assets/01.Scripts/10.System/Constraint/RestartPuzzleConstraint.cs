using _01.Scripts._01.ThreeMatch;
using System.Collections;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/RestartPuzzleConstraint", fileName = "RestartPuzzleConstraint")]
    public class RestartPuzzleConstraint : Constraint
    {
        [SerializeField] private float restartInterval;
        private Coroutine _restartPuzzleCoroutine;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("RestartPuzzleConstraint");
            _restartPuzzleCoroutine = context.Puzzle.StartCoroutine(RestartPuzzle(context.Puzzle));
        }

        public override void CancelConstraint(ConstraintContext context)
        {
            context.Puzzle.StopCoroutine(_restartPuzzleCoroutine);
            _restartPuzzleCoroutine = null;
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
