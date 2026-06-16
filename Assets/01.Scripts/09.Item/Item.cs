using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._06.Shop;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

namespace _01.Scripts._09.Item
{
    public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ItemType itemType;

        private PuzzleGenerator _generator;
        private UnitSpawner _spawner;
        private int _itemAmount;
        private TextMeshProUGUI _itemAmountText;
        private GameObject _draggedIcon;
        private Image _originalImage;
        private PuzzleObject _targetTile;
        private Color _targetTileColor;
        private bool _isItemApplying;

        private void Awake()
        {
            _originalImage = GetComponent<Image>();
            _itemAmountText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Init(ItemType type, PuzzleGenerator generator, UnitSpawner spawner)
        {
            itemType = type;
            _generator = generator;
            _spawner = spawner;
            
            SetItemAmountText();
            SetRaiseSpawnProbItem();
        }

        private void SetItemAmountText()
        {
            _itemAmount = GameManager.Instance.itemData.ItemAmounts[itemType];
            _itemAmountText.text = _itemAmount.ToString();
        }

        private void SetRaiseSpawnProbItem()
        {
            if (itemType == ItemType.RaiseSpawnProb)
            {
                Button button = GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (_itemAmount == 0 || _isItemApplying)
                    {
                        return;
                    }
                    ApplyItemEffect();
                });
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_itemAmount == 0)
            {
                return;
            }

            if (itemType == ItemType.RaiseSpawnProb)
            {
                return;
            }
            
            Color c = _originalImage.color;
            c.a = 0.5f;
            
            _draggedIcon = new GameObject("DraggedItem");
            _draggedIcon.transform.SetParent(GetComponentInParent<Canvas>().transform);
            _draggedIcon.transform.SetAsLastSibling();
            
            Image iconImage = _draggedIcon.AddComponent<Image>();
            iconImage.sprite = _originalImage.sprite;
            iconImage.color = c;
            iconImage.raycastTarget = false;
            
            RectTransform iconRect = _draggedIcon.GetComponent<RectTransform>();
            iconRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (Time.timeScale <= 0) 
            {
                CancelDrag();
                return;
            }
            
            if (_itemAmount == 0)
            {
                return;
            }
            
            if (itemType == ItemType.RaiseSpawnProb)
            {
                return;
            }
            
            if (_draggedIcon != null)
            {
                RectTransform canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
                Camera worldCamera = eventData.pressEventCamera;
                
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, eventData.position, worldCamera, out Vector3 worldPoint))
                {
                    _draggedIcon.transform.position = worldPoint;
                    
                    Vector3 localPos = _draggedIcon.transform.localPosition;
                    localPos.z = 0f;
                    _draggedIcon.transform.localPosition = localPos;
                }
            }
            
            HighlightNearestTile();
        }
        
        private void HighlightNearestTile()
        {
            if (_targetTile != null)
            {
                _targetTile.GetComponent<Image>().color = _targetTileColor;
            }

            PointerEventData pointerData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                PuzzleObject tile = result.gameObject.GetComponent<PuzzleObject>();
                if (tile != null && CheckTileType(tile))
                {
                    _targetTile = tile;
                    _targetTileColor = _targetTile.GetComponent<Image>().color;
                    _targetTile.GetComponent<Image>().color = Color.gray;
                    return;
                }
            }
            
            _targetTile = null;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_draggedIcon != null)
            {
                Destroy(_draggedIcon);
            }

            if (_targetTile != null)
            {
                ApplyItemEffect(_targetTile);
                _targetTile.GetComponent<Image>().color = Color.white;
            }
        }
        
        private void CancelDrag()
        {
            if (_draggedIcon != null)
            {
                Destroy(_draggedIcon);
                _draggedIcon = null;
            }
            
            if (_targetTile != null)
            {
                _targetTile.GetComponent<Image>().color = _targetTileColor;
                _targetTile = null;
            }
        }

        private bool CheckTileType(PuzzleObject po)
        {
            switch (itemType)
            {
                case ItemType.Joker:
                    if (po is not NormalPuzzleObject)
                    {
                        return false;
                    }
                    break;
                case ItemType.DestroyObstacle:
                    if (po is not ObstaclePuzzleObject)
                    {
                        return false;
                    }
                    break;
                case ItemType.CreateLineBomb:
                    if (po is not NormalPuzzleObject)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        private void ApplyItemEffect(PuzzleObject tile = null)
        {
            switch (itemType)
            {
                case ItemType.Joker:
                    StartCoroutine(_generator.UseJokerItem(tile));
                    break;
                case ItemType.DestroyObstacle:
                    StartCoroutine(_generator.UseDestroyObstacleItem(tile));
                    break;
                case ItemType.CreateLineBomb:
                    StartCoroutine(_generator.UseCreateLineBombItem(tile));
                    break;
                case ItemType.RaiseSpawnProb:
                    StartCoroutine(ApplyRaiseSpawnProbItem());
                    break;
            }

            _itemAmount--;
            _itemAmountText.text = _itemAmount.ToString();
            GameManager.Instance.itemData.ItemAmounts[itemType] = _itemAmount;
            GameManager.Instance.SaveItemData();
        }

        private IEnumerator ApplyRaiseSpawnProbItem()
        {
            _isItemApplying = true;
            
            List<List<int>> originProb = _spawner.friendlySpawnWeights;
            List<List<int>> raisedProb = new()
            {
                new List<int> { 100 },
                new List<int> { 50, 50 },
                new List<int> { 40, 30, 30 },
                new List<int> { 30, 20, 30, 20 },
                new List<int> { 10, 10, 30, 25, 25 },
            };
            _spawner.friendlySpawnWeights = raisedProb;
            print("아군 고등급 기물 소환 확률 증가");
            
            yield return new WaitForSeconds(10f);

            _spawner.friendlySpawnWeights = originProb;
            print("소환 확률 초기화");
            
            _isItemApplying = false;
        }
    }
}