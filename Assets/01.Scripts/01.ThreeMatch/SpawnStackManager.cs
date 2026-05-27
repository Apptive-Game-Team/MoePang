using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

        public void AddStack(Habitat type, int num)
        {
            stacks.First(s => s.type == type).AddStack(num);
        }

        public SpawnStack SetStack(Habitat type)
        {
            return stacks.First(s => s.type == type);
        }
        
        /// <summary>
        /// SpawnStack 최대 스택 수 한번에 변경 (집을 지켜라 모드에서 사용)
        /// </summary>
        public void SetAllStackMaxCount(int count)
        {
            foreach (SpawnStack stack in stacks)
            {
                stack.SetStackMaxCount(count);
            }
        }
    }
}
