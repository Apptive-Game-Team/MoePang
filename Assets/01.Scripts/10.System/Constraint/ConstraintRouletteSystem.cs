using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01.Scripts._10.System.Constraint
{
    public class ConstraintRouletteSystem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<Constraint> constraintList;

        [Header("UI References")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private float itemHeight = 100f;
        
        [Header("Roulette Settings")]
        [SerializeField] private AnimationCurve decelerationCurve;
        [SerializeField] private float spinDuration = 4f;
        [SerializeField] private float maxSpeed = 1500f;

        private List<RectTransform> _spawnedItems = new();
        private List<Constraint> _itemDataMapping = new();
        
        private bool _isSpinning;
        private float _totalLoopHeight;
        private float _bottomThreshold; 

        public void InitializeItems()
        {
            if (constraintList == null || constraintList.Count == 0)
            {
                return;
            }
            
            _bottomThreshold = -itemHeight * 2f; 

            for (int i = 0; i < constraintList.Count; i++)
            {
                GameObject go = Instantiate(itemPrefab, itemContainer);
                RectTransform rect = go.GetComponent<RectTransform>();
                
                Constraint data = constraintList[i % constraintList.Count];
                
                TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                tmp.text = data.constraintDescription;
                
                float startY = (i * itemHeight) + _bottomThreshold;
                rect.anchoredPosition = new Vector2(0, startY);
                
                _spawnedItems.Add(rect);
                _itemDataMapping.Add(data);
            }

            _totalLoopHeight = _spawnedItems.Count * itemHeight;
        }

        public IEnumerator StartRoulette(Action<ConstraintType> onComplete)
        {
            if (_isSpinning)
            {
                yield break;
            }
            
            int targetDataIndex = Random.Range(0, constraintList.Count);
            Constraint targetData = constraintList[targetDataIndex];

            yield return SpinRoutine(targetData, onComplete);
        }

        private IEnumerator SpinRoutine(Constraint targetData, Action<ConstraintType> onComplete)
        {
            _isSpinning = true;
            
            float fastSpinDuration = spinDuration * 0.6f;
            float timer = 0f;
            while (timer < fastSpinDuration)
            {
                timer += Time.unscaledDeltaTime;
                MoveItemsDown(maxSpeed * Time.unscaledDeltaTime);
                yield return null;
            }
            
            RectTransform targetRect = _spawnedItems.Where((_, i) => _itemDataMapping[i] == targetData)
                .FirstOrDefault(t => t.anchoredPosition.y > 0);
            
            if (!targetRect)
            {
                for (int i = 0; i < _spawnedItems.Count; i++)
                {
                    if (_itemDataMapping[i] == targetData)
                    {
                        targetRect = _spawnedItems[i];
                        break;
                    }
                }
            }
            
            float slowdownDuration = spinDuration * 0.4f;
            timer = 0f;
            
            float remainingDistance = targetRect.anchoredPosition.y;
            if (remainingDistance < 0) remainingDistance += _totalLoopHeight;
            
            float lastProgress = 0f;

            while (timer < slowdownDuration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = timer / slowdownDuration;
                
                float curveEval = decelerationCurve.Evaluate(progress);

                float deltaDistance = remainingDistance * (curveEval - lastProgress);
                lastProgress = curveEval;

                MoveItemsDown(deltaDistance);
                yield return null;
            }

            float finalOffset = targetRect.anchoredPosition.y;
            if (Mathf.Abs(finalOffset) > 0.1f)
            {
                float snapTimer = 0f;
                float snapDuration = 0.15f;
                while (snapTimer < snapDuration)
                {
                    snapTimer += Time.unscaledDeltaTime;
                    float t = snapTimer / snapDuration;
                    float currentY = Mathf.Lerp(finalOffset, 0f, t);
                    float diff = finalOffset - currentY;
                    
                    MoveItemsDown(diff);
                    finalOffset = targetRect.anchoredPosition.y;
                    yield return null;
                }
            }
            
            onComplete?.Invoke(targetData.type);
            
            _isSpinning = false;
        }

        private void MoveItemsDown(float distance)
        {
            foreach (RectTransform rect in _spawnedItems)
            {
                Vector2 pos = rect.anchoredPosition;
                
                pos.y -= distance;

                if (pos.y < _bottomThreshold)
                {
                    pos.y += _totalLoopHeight;
                }

                rect.anchoredPosition = pos;
            }
        }
    }
}