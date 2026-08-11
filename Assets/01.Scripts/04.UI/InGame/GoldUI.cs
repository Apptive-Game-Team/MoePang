using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace _01.Scripts._04.UI.InGame
{
    public class GoldUI : MonoBehaviour
    {
        private static readonly int Highlight = Shader.PropertyToID("_Highlight");
        
        private Image _goldImage;
        public Vector3 GoldImagePos => _goldImage.transform.position;
        private TextMeshProUGUI _text;
        
        private Material _material;
        private Tween _moveInTween;
        private Vector3 _goldScale;
        private Sequence _effectSeq;
        
        private RectTransform _rectTransform;
        private Vector2 _originPos;
        private Vector2 _hidePos;
        

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _goldImage = GetComponentsInChildren<Image>().FirstOrDefault(go => go.gameObject != gameObject);
            _text = GetComponentInChildren<TextMeshProUGUI>();
            
            _originPos = _rectTransform.anchoredPosition;
            _hidePos = _originPos + new Vector2(400f, 0f);

            if (_goldImage != null)
            {
                _goldScale = _goldImage.transform.localScale;
                
                Image img = _goldImage.GetComponent<Image>();
                if (img.material != null)
                {
                    _material = new Material(img.material);
                    img.material = _material;
                }
            }
        }

        private void Start()
        {
            _rectTransform.anchoredPosition = _hidePos;
            UpdateGold();
        }

        private void OnEnable()
        {
            GoldManager.Instance.OnGoldChanged += UpdateGold;
        }

        private void OnDisable()
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGold;
        }

        public void ShowUI(bool show = true)
        {
            if (_moveInTween is { active: true })
            {
                return;
            }
    
            if (show)
            {
                _moveInTween = _rectTransform.DOAnchorPos(_originPos, 0.15f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        _moveInTween = null;
                    });
            }
            else
            {
                _rectTransform.DOAnchorPos(_hidePos, 0.15f)
                    .SetEase(Ease.OutBack);
            }
        }

        public void UpdateGold()
        {
            _text.text = GoldManager.Instance.Gold.ToString("N0");
        }

        public void AddGoldEffect()
        {
            if (_effectSeq != null)
            {
                _effectSeq.Complete();
                _effectSeq = null;
            }

            _goldImage.transform.localScale = _goldScale;
            _material.SetFloat(Highlight, 0f);

            _effectSeq = DOTween.Sequence();
            
            Tween t1 = _goldImage.transform.DOScale(0.8f, 0.1f).SetLoops(2, LoopType.Yoyo);
            Tween t2 = _material.DOFloat(1f, Highlight, 0.1f).SetLoops(2, LoopType.Yoyo);

            _effectSeq.Join(t1);
            _effectSeq.Join(t2);
            
            _effectSeq.OnComplete(() =>
            {
                _goldImage.transform.localScale = Vector3.one;
                _material.SetFloat(Highlight, 0f);
                _effectSeq = null;
            });
        }
    }
}
