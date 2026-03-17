using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpawnStackManager : MonoBehaviour
    {
        [SerializeField] private SpawnStack[] stacks;
        [SerializeField] private UnitSpawner unitSpawner;

        private void Awake()
        {
            foreach (SpawnStack stack in stacks)
            {
                stack.Init(unitSpawner);
            }
        }

        public void AddStack(NormalPuzzleType type, int num)
        {
            stacks[(int)type].AddStack(num);
        }

        public SpawnStack SetStack(NormalPuzzleType type)
        {
            return stacks[(int)type];
        }
    }
}
