using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Stamina
{
    public class StaminaBarUI : MonoBehaviour
    {
        [SerializeField] private Image[] apples = new Image[5];
        [SerializeField] private Image[] recoveryFills = new Image[5];
        [SerializeField] private TextMeshProUGUI additionalText;
        [SerializeField] private Material grayscaleMaterial;

        private readonly Vector3[] _baseScales = new Vector3[5];
        private readonly Color[] _baseColors = new Color[5];
        private readonly Coroutine[] _bounces = new Coroutine[5];
        private int _previous = -1;
        private int _previousExtra = -1;
        private int _entryRecoveryIndex = -1;
        private StaminaManager _subscribedManager;
        private StaminaSpendEffect _spendEffect;

        public float SpendDuration => Mathf.Max(0.1f, spendDuration);
        public bool IsSpending => _spendEffect && !_spendEffect.IsComplete;

        [Header("Consumption Effect")]
        [SerializeField, Min(0.1f)] private float spendDuration = 0.45f;
        [SerializeField, Range(0f, 2f)] private float spendRiseHeight = 0.55f;

        private void Awake()
        {
            for (int i = 0; i < apples.Length; i++)
            {
                if (!apples[i])
                {
                    continue;
                }

                _baseScales[i] = apples[i].transform.localScale;
                _baseColors[i] = apples[i].color;
            }
        }

        private void OnEnable()
        {
            _previous = -1;
            _previousExtra = -1;
            _entryRecoveryIndex = -1;

            SubscribeToConsumption();
        }

        private void SubscribeToConsumption()
        {
            StaminaManager manager = StaminaManager.Instance;

            if (_subscribedManager == manager)
            {
                return;
            }

            if (_subscribedManager)
            {
                _subscribedManager.OnNaturalStaminaConsumed -= PlaySpendEffect;
            }

            _subscribedManager = manager;

            if (_subscribedManager)
            {
                _subscribedManager.OnNaturalStaminaConsumed += PlaySpendEffect;
            }
        }

        private void PlaySpendEffect(int firstIndex, int count)
        {
            if (_entryRecoveryIndex < 0 && _subscribedManager && _subscribedManager.IsEnteringStage)
            {
                _entryRecoveryIndex = Mathf.Clamp(firstIndex + count, 0, StaminaRules.MaxStamina);
            }

            for (int i = firstIndex; i < firstIndex + count && i < apples.Length; i++)
            {
                if (!apples[i])
                {
                    continue;
                }

                if (_bounces[i] != null)
                {
                    StopCoroutine(_bounces[i]);
                }

                _bounces[i] = null;
                apples[i].transform.localScale = _baseScales[i];
            }

            _spendEffect = StaminaSpendEffect.Play(apples, _baseColors, firstIndex, count, SpendDuration, spendRiseHeight);
        }

        private void Update()
        {
            SubscribeToConsumption();

            GameManager game = GameManager.Instance;

            if (!game || game.staminaData == null)
            {
                return;
            }

            StaminaManager manager = StaminaManager.Instance;
            bool ready = manager && manager.IsInitialized;
            int count = Mathf.Clamp(game.staminaData.stamina, 0, StaminaRules.MaxStamina);
            float progress = ready ? manager.RecoveryProgress : 0f;
            int extra = Mathf.Max(0, game.staminaData.additionalStamina);
            int recoveryIndex = _entryRecoveryIndex >= 0 ? _entryRecoveryIndex : count;

            if (additionalText && extra != _previousExtra)
            {
                additionalText.text = $"+{extra}";
                _previousExtra = extra;
            }

            for (int i = 0; i < apples.Length; i++)
            {
                if (!apples[i])
                {
                    continue;
                }

                bool full = i < count;
                apples[i].material = full ? null : grayscaleMaterial;
                apples[i].color = full ? _baseColors[i] : new Color(0.58f, 0.58f, 0.58f, _baseColors[i].a);

                bool charging = ready && i == recoveryIndex && count < StaminaRules.MaxStamina;

                if (recoveryFills[i])
                {
                    recoveryFills[i].enabled = charging;
                    recoveryFills[i].fillAmount = charging ? progress : 0f;
                }

                if (ready && _previous >= 0 && i >= _previous && i < count)
                {
                    if (_bounces[i] != null)
                    {
                        StopCoroutine(_bounces[i]);
                    }

                    _bounces[i] = StartCoroutine(Bounce(i));
                }
            }

            if (ready)
            {
                _previous = count;
            }
        }

        private IEnumerator Bounce(int index)
        {
            const float duration = 0.4f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * (1f - t) * 0.25f;
                apples[index].transform.localScale = _baseScales[index] * scale;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            apples[index].transform.localScale = _baseScales[index];
            _bounces[index] = null;
        }

        private void OnDisable()
        {
            if (_subscribedManager)
            {
                _subscribedManager.OnNaturalStaminaConsumed -= PlaySpendEffect;
            }

            _subscribedManager = null;

            StopAllCoroutines();

            for (int i = 0; i < apples.Length; i++)
            {
                if (apples[i])
                {
                    apples[i].transform.localScale = _baseScales[i];
                }

                _bounces[i] = null;
            }
        }
    }
}
