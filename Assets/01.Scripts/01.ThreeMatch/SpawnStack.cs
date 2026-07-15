using _01.Scripts._02.Unit;
using _01.Scripts._08.Utility;
using _01.Scripts._11.HabitatMode;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpawnStack : MonoBehaviour
    {
        [SerializeField] private GameObject stackPrefab;
        [SerializeField] private RectTransform stackParent;
        [SerializeField] private RectTransform stackCenter;
        [SerializeField] private float stackRadius = 35f;
        [SerializeField] private float startAngle = 90f;
        public Habitat type;
        public bool isBanned;

        public event Action<int> OnStackAdded;

        [SerializeField] private int StackMaxCount = 3;
        
        private static readonly int Highlight = Shader.PropertyToID("_Highlight");
        private static readonly int Full = Shader.PropertyToID("_Full");
        private Material _material;
        private Sequence _effectSeq;
        private UnitSpawner _spawner;
        private int _stackLength;
        private int _stackCount;
        //private const int StackMaxCount = 3;
        
        private readonly List<GameObject> _stacks = new();
        private readonly List<Material> _stackMaterials = new();
        private readonly Queue<Func<IEnumerator>> _taskQueue = new();
        private bool _isProcessing;

        private void Awake()
        {
            Image img = GetComponent<Image>();
            if (img != null && img.material != null)
            {
                _material = new Material(img.material);
                img.material = _material;
            }

            SetupStackSlots();
        }
        
        private void SetupStackSlots()
        {
            if (!HabitatModeManager.Instance.IsHabitatBattle)
            {
                StackMaxCount = 3;
                EnsureStackSlotCount(StackMaxCount);
                _stackLength = Mathf.Min(StackMaxCount, _stacks.Count);
                LayoutStackSlots();
                return;
            }

            bool isMeadowMode =
                HabitatModeManager.Instance.HabitatMode == HabitatMode.MeadowMode;

            StackMaxCount = isMeadowMode ? 6 : 3;
            EnsureStackSlotCount(StackMaxCount);
            _stackLength = Mathf.Min(StackMaxCount, _stacks.Count);
            LayoutStackSlots();
        }

        public void Init(UnitSpawner spawner)
        {
            _spawner = spawner;
        }

        public void BanStack()
        {
            isBanned = true;
            GetComponent<Image>().color = Color.gray;
        }
        
        public void AddStack(int num)
        {
            if (isBanned)
            {
                return;
            }
            
            _taskQueue.Enqueue(() => AddStackProcess(num));

            if (!_isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        private IEnumerator ProcessQueue()
        {
            _isProcessing = true;
            while (_taskQueue.Count > 0)
            {
                var task = _taskQueue.Dequeue();
                yield return task();
            }
            _isProcessing = false;
        }
        
        private IEnumerator AddStackProcess(int num)
        {
            AddStackEffect();
            
            int totalNewCount = _stackCount + num;

            OnStackAdded?.Invoke(num);
            
            if (totalNewCount >= StackMaxCount)
            {
                FillStack(StackMaxCount);
                
                yield return new WaitForSeconds(0.2f);
                
                int spawnCount = totalNewCount / StackMaxCount;
                _stackCount = totalNewCount % StackMaxCount;
                
                // todo : Add Spawn Effect
                
                Spawn(spawnCount);
                FillStack(_stackCount);
            }
            else
            {
                _stackCount = totalNewCount;
                FillStack(_stackCount);
            }

            yield return new WaitForSeconds(0.1f);
        }
        
        private void AddStackEffect()
        {
            if (_effectSeq != null)
            {
                _effectSeq.Complete();
                _effectSeq = null;
            }

            transform.localScale = Vector3.one;
            if (_material == null)
            {
                return;
            }

            _material.SetFloat(Highlight, 0f);

            _effectSeq = DOTween.Sequence();
            
            Tween t1 = transform.DOScale(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo);
            Tween t2 = _material.DOFloat(1f, Highlight, 0.1f).SetLoops(2, LoopType.Yoyo);

            _effectSeq.Join(t1);
            _effectSeq.Join(t2);
            
            _effectSeq.OnComplete(() =>
            {
                transform.localScale = Vector3.one;
                _material.SetFloat(Highlight, 0f);
                _effectSeq = null;
            });
        }

        private void Spawn(int num)
        {
            for (int i = 0; i < num; i++)
            {
                _spawner.SpawnFriendly(type);
            }
        }

        private void FillStack(int num)
        {
            int filledCount = 0;

            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i] == null)
                {
                    continue;
                }

                if (!_stacks[i].activeSelf)
                {
                    continue;
                }

                if (i >= _stackMaterials.Count)
                {
                    continue;
                }

                if (_stackMaterials[i] == null)
                {
                    continue;
                }

                _stackMaterials[i].SetFloat(Full, filledCount < num ? 1 : 0);
                filledCount++;
            }
        }

        private void EnsureStackSlotCount(int count)
        {
            if (stackPrefab == null)
            {
                Debug.LogWarning($"{name} has no stack prefab assigned.", this);
                return;
            }

            if (HasInvalidStackPrefab())
            {
                return;
            }

            Transform parent = GetStackParent();
            while (_stacks.Count < count)
            {
                GameObject slot = Instantiate(stackPrefab, parent);
                slot.name = $"{stackPrefab.name}_{_stacks.Count + 1}";
                slot.SetActive(true);

                _stacks.Add(slot);
                _stackMaterials.Add(CreateStackMaterial(slot));
            }

            for (int i = 0; i < _stacks.Count; i++)
            {
                _stacks[i].SetActive(i < count);
            }

            if (stackPrefab.scene.IsValid())
            {
                stackPrefab.SetActive(false);
            }
        }

        private bool HasInvalidStackPrefab()
        {
            if (stackPrefab == gameObject)
            {
                Debug.LogError(
                    $"{name} cannot use itself as stack prefab. Assign only the small stack slot Image prefab.",
                    this);
                return true;
            }

            if (stackPrefab.GetComponentInChildren<SpawnStack>(true) != null)
            {
                Debug.LogError(
                    $"{name} stack prefab contains SpawnStack. Assign only the small stack slot Image prefab, not the SpawnStack root.",
                    stackPrefab);
                return true;
            }

            return false;
        }

        private Transform GetStackParent()
        {
            if (stackParent != null)
            {
                return stackParent;
            }

            if (stackPrefab.transform.parent != null)
            {
                return stackPrefab.transform.parent;
            }

            return transform;
        }

        private Material CreateStackMaterial(GameObject slot)
        {
            Image stackImg = slot.GetComponent<Image>();
            if (stackImg == null || stackImg.material == null)
            {
                return null;
            }

            Material mat = new(stackImg.material);
            stackImg.material = mat;
            return mat;
        }

        private void LayoutStackSlots()
        {
            if (_stacks.Count == 0 || _stackLength <= 0)
            {
                return;
            }

            if (StackMaxCount > _stacks.Count)
            {
                Debug.LogWarning(
                    $"{name} needs {StackMaxCount} stack slots, but only {_stacks.Count} were created.",
                    this);
            }

            Vector2 centerPosition = GetStackCenterAnchoredPosition();
            float angleStep = 360f / _stackLength;

            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i] == null)
                {
                    continue;
                }

                bool isActiveSlot = i < _stackLength;
                _stacks[i].SetActive(isActiveSlot);

                if (!isActiveSlot)
                {
                    continue;
                }

                if (!_stacks[i].TryGetComponent(out RectTransform rectTransform))
                {
                    continue;
                }

                float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                rectTransform.anchoredPosition = centerPosition + direction * stackRadius;
            }
        }

        private Vector2 GetStackCenterAnchoredPosition()
        {
            if (stackCenter == null)
            {
                return Vector2.zero;
            }

            RectTransform firstStack = _stacks[0].GetComponent<RectTransform>();
            if (firstStack != null && stackCenter.parent == firstStack.parent)
            {
                return stackCenter.anchoredPosition;
            }

            if (firstStack != null && firstStack.parent is RectTransform parent)
            {
                Vector3 worldPosition = stackCenter.TransformPoint(stackCenter.rect.center);
                Vector3 localPosition = parent.InverseTransformPoint(worldPosition);
                return localPosition;
            }

            return stackCenter.anchoredPosition;
        }
        
        /// <summary>
        /// 스택 Max 갯수 변경 (집을 지켜라 모드에서 사용)
        /// </summary>
        public void SetStackMaxCount(int count)
        {
            StackMaxCount = Mathf.Max(1, count);
            EnsureStackSlotCount(StackMaxCount);
            _stackLength = Mathf.Min(StackMaxCount, _stacks.Count);
            LayoutStackSlots();
            FillStack(_stackCount);
        }
    }
}
