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
        [SerializeField] private GameObject[] stacks;
        public Habitat type;
        public bool isBanned;

        public event Action<int> OnStackAdded;

        [SerializeField] private int StackMaxCount = 3;
        
        private static readonly int Highlight = Shader.PropertyToID("_Highlight");
        private static readonly int Full = Shader.PropertyToID("_Full");
        private Material[] _stackMaterials;
        private Material _material;
        private Sequence _effectSeq;
        private UnitSpawner _spawner;
        private int _stackLength;
        private int _stackCount;
        //private const int StackMaxCount = 3;
        
        private readonly Queue<Func<IEnumerator>> _taskQueue = new();
        private bool _isProcessing;

        private void Awake()
        {
            SetupStackSlots();
            
            Image img = GetComponent<Image>();
            if (img.material != null)
            {
                _material = new Material(img.material);
                img.material = _material;
            }

            _stackMaterials = new Material[stacks.Length];

            for (int i = 0; i < stacks.Length; i++)
            {
                Image stackImg = stacks[i].GetComponent<Image>();
                if (stackImg.material != null)
                {
                    Material mat = new(stackImg.material);
                    stackImg.material = mat;
                    _stackMaterials[i] = mat;
                }
            }
        }
        
        private void SetupStackSlots()
        {
            bool isHabitatBattleScene =
                SceneManager.GetActiveScene().name == SceneInfo.GetSceneName(SceneType.HabitatBattle);

            if (!isHabitatBattleScene)
            {
                StackMaxCount = 3;
                _stackLength = stacks.Length;
                return;
            }

            bool isMeadowMode =
                HabitatModeManager.Instance.HabitatMode == HabitatMode.MeadowMode;

            StackMaxCount = isMeadowMode ? 6 : 3;
            _stackLength = StackMaxCount;

            if (!isMeadowMode)
            {
                return;
            }

            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i].SetActive(true);
            }
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

            for (int i = 0; i < stacks.Length; i++)
            {
                if (!stacks[i].activeSelf)
                {
                    continue;
                }

                _stackMaterials[i].SetFloat(Full, filledCount < num ? 1 : 0);
                filledCount++;
            }
        }
        
        /// <summary>
        /// 스택 Max 갯수 변경 (집을 지켜라 모드에서 사용)
        /// </summary>
        public void SetStackMaxCount(int count)
        {
            StackMaxCount = count;
            FillStack(_stackCount);
        }
    }
}