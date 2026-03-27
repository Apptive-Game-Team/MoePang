using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpawnStack : MonoBehaviour
    {
        [SerializeField] private GameObject[] stacks;
        
        private static readonly int Full = Shader.PropertyToID("_Full");
        private Material[] _stackMaterials;
        private UnitSpawner _spawner;
        private int _stackLength;
        private int _stackCount;
        private const int StackMaxCount = 3;

        private void Awake()
        {
            _stackLength = stacks.Length;
            _stackMaterials = new Material[_stackLength];
            for (int i = 0;i < _stackLength;i++)
            {
                Image img = stacks[i].GetComponent<Image>();
                if (img.material != null)
                {
                    Material material = new(img.material);
                    img.material = material;
                    _stackMaterials[i] = material;
                }
            }
        }

        public void Init(UnitSpawner spawner)
        {
            _spawner = spawner;
        }

        public void AddStack(int num)
        {
            _stackCount += num;

            int spawnCount = 0;
            if (_stackCount >= StackMaxCount)
            {
                spawnCount = _stackCount / StackMaxCount;
                _stackCount %= StackMaxCount;
            }

            FillStack(_stackCount);
            Spawn(spawnCount);
        }

        private void Spawn(int num)
        {
            for (int i = 0; i < num; i++)
            {
                _spawner.FriendlySpawn();
            }
        }

        private void FillStack(int num)
        {
            for (int i = 0; i < num; i++)
            {
                _stackMaterials[i].SetFloat(Full, 1);
            }

            for (int i = num; i < _stackLength; i++)
            {
                _stackMaterials[i].SetFloat(Full, 0);
            }
        }
    }
}
