using DG.Tweening;
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
        private Vector3 _originPos;
        private Vector3 _goldScale;
        private Sequence _effectSeq;
        

        private void Awake()
        {
            _goldImage = GetComponentsInChildren<Image>().FirstOrDefault(go => go.gameObject != gameObject);
            _text = GetComponentInChildren<TextMeshProUGUI>();

            if (_goldImage != null)
            {
                _originPos = _goldImage.transform.position;
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
            UpdateGold();
        }

        public void ShowUI(bool show = true)
        {
            if (_moveInTween is { active: true })
            {
                return;
            }
            
            if (show)
            {
                _moveInTween = transform.DOMove(_originPos - new Vector3(1, 0, 0), 0.15f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        _moveInTween = null;
                    });
            }
            else
            {
                transform.DOMove(_originPos + new Vector3(1, 0, 0), 0.15f)
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
