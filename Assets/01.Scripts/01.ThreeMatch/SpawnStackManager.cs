using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpawnStackManager : MonoBehaviour
    {
        private static readonly int Highlight = Shader.PropertyToID("_Highlight");
        
        [SerializeField] private SpawnStack[] stacks;
        [SerializeField] private UnitSpawner unitSpawner;
        private Material[] _stackMaterials;
        private int _stackLength;
        private Sequence _effectSeq;

        private void Awake()
        {
            _stackLength = stacks.Length;
            _stackMaterials = new Material[_stackLength];
            for (int i = 0;i < stacks.Length;i++)
            {
                Image img = stacks[i].GetComponent<Image>();
                if (img.material != null)
                {
                    Material material = new(img.material);
                    img.material = material;
                    _stackMaterials[i] = material;
                }
            }
            
            foreach (SpawnStack stack in stacks)
            {
                stack.Init(unitSpawner);
            }
        }

        public void AddStack(Habitat type, int num)
        {
            AddStackEffect(type);
            stacks[(int)type].AddStack(num);
        }

        private void AddStackEffect(Habitat type)
        {
            if (_effectSeq != null)
            {
                _effectSeq.Complete();
                _effectSeq = null;
            }

            Transform tr = stacks[(int)type].transform;
            Material mat = _stackMaterials[(int)type];
            
            tr.localScale = Vector3.one;
            mat.SetFloat(Highlight, 0f);

            _effectSeq = DOTween.Sequence();
            
            Tween t1 = tr.DOScale(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo);
            Tween t2 = mat.DOFloat(1f, Highlight, 0.1f).SetLoops(2, LoopType.Yoyo); 

            _effectSeq.Join(t1);
            _effectSeq.Join(t2);

            _effectSeq.OnComplete(() =>
            {
                tr.localScale = Vector3.one;
                mat.SetFloat(Highlight, 0f);
                _effectSeq = null;
            });
        }

        public SpawnStack SetStack(Habitat type)
        {
            return stacks[(int)type];
        }
    }
}
