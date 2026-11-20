using System;
using System.Collections;
using System.Threading.Tasks;
using _01.Scripts._12.Backend;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _01.Scripts._04.UI;
using _01.Scripts._04.UI.MainScene.Stamina;
using _01.Scripts._08.Utility;

namespace _01.Scripts._00.Manager
{
    public class StaminaManager : SingletonObject<StaminaManager>
    {
        public const int MaxStamina = StaminaRules.MaxStamina;

        private DateTime _serverTime;
        private double _sampledAt;
        private double _nextSync;
        private Task _syncTask;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;
        public bool IsEnteringStage { get; private set; }

        public event Action OnStaminaChanged;
        public event Action<int, int> OnNaturalStaminaConsumed;

        private DateTime EstimatedServerTime => _serverTime.AddSeconds(
            Math.Max(0, Time.realtimeSinceStartupAsDouble - _sampledAt));

        private IEnumerator Start()
        {
            if (IsDuplicate)
            {
                yield break;
            }

            while (GameManager.Instance == null)
            {
                yield return null;
            }

            if (!GameManager.Instance.IsDataLoaded)
            {
                Task load = GameManager.Instance.LoadData();
                yield return new WaitUntil(() => load.IsCompleted);

                if (load.IsFaulted)
                {
                    Debug.LogException(load.Exception);
                }
            }

            while (true)
            {
                if (Time.realtimeSinceStartupAsDouble >= _nextSync)
                {
                    Task sync = RecoverStamina();
                    yield return new WaitUntil(() => sync.IsCompleted);
                }

                if (_isInitialized)
                {
                    ApplyRecovery();
                }

                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        public Task RecoverStamina()
        {
            if (_syncTask == null || _syncTask.IsCompleted)
            {
                _syncTask = SyncServerTime();
            }

            return _syncTask;
        }

        private async Task SyncServerTime()
        {
            try
            {
                var response = await SupabaseManager.Instance.Client.Rpc("get_server_time", new { });
                DateTime now = JsonConvert.DeserializeObject<DateTime>(response.Content).ToUniversalTime();

                if (now == DateTime.MinValue)
                {
                    throw new InvalidOperationException("Empty server time.");
                }

                if (this == null)
                {
                    return;
                }

                _serverTime = now;
                _sampledAt = Time.realtimeSinceStartupAsDouble;
                _nextSync = _sampledAt + 60;

                bool first = !_isInitialized;
                _isInitialized = true;

                ApplyRecovery();

                if (first)
                {
                    OnStaminaChanged?.Invoke();
                }
            }
            catch (Exception e)
            {
                if (this == null)
                {
                    return;
                }

                _nextSync = Time.realtimeSinceStartupAsDouble + 5;
                Debug.LogWarning($"Stamina server time sync failed; retrying in 5 seconds. {e.Message}");
            }
        }

        private void ApplyRecovery()
        {
            if (!StaminaRules.Recover(GameManager.Instance.staminaData, EstimatedServerTime))
            {
                return;
            }

            GameManager.Instance.SaveStaminaData();
            OnStaminaChanged?.Invoke();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused && _isInitialized)
            {
                _nextSync = 0;
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused && _isInitialized)
            {
                _nextSync = 0;
            }
        }

        public bool ConsumeStamina(int amount = StaminaRules.NormalEntryCost)
        {
            if (!_isInitialized || amount <= 0)
            {
                return false;
            }

            ApplyRecovery();
            int previousNatural = GameManager.Instance.staminaData.stamina;

            if (!StaminaRules.TryConsume(GameManager.Instance.staminaData, amount, EstimatedServerTime))
            {
                return false;
            }

            GameManager.Instance.SaveStaminaData();

            int remainingNatural = GameManager.Instance.staminaData.stamina;

            if (previousNatural > remainingNatural)
            {
                OnNaturalStaminaConsumed?.Invoke(remainingNatural, previousNatural - remainingNatural);
            }

            OnStaminaChanged?.Invoke();

            return true;
        }

        public bool TryEnterStage(int amount, Action enterStage)
        {
            if (IsEnteringStage || enterStage == null)
            {
                return false;
            }

            IsEnteringStage = true;
            GameObject blocker = null;

            try
            {
                var bar = FindFirstObjectByType<StaminaBarUI>();

                if (bar && !bar.isActiveAndEnabled)
                {
                    bar = null;
                }

                if (!ConsumeStamina(amount))
                {
                    IsEnteringStage = false;

                    return false;
                }

                float duration = bar ? bar.SpendDuration : 0.45f;
                blocker = CreateEntryInputBlocker();
                StartCoroutine(FinishStageEntry(enterStage, bar, duration, blocker, SceneManager.GetActiveScene().handle));

                return true;
            }
            catch
            {
                IsEnteringStage = false;

                if (blocker)
                {
                    Destroy(blocker);
                }

                throw;
            }
        }

        private static GameObject CreateEntryInputBlocker()
        {
            var root = new GameObject("StaminaEntryInputBlocker", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;

            var imageObject = new GameObject("BlockInput", typeof(RectTransform), typeof(Image), typeof(Button));
            imageObject.transform.SetParent(root.transform, false);

            var image = imageObject.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;

            var button = imageObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            if (UnityEngine.EventSystems.EventSystem.current)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(imageObject);
            }

            return root;
        }

        private IEnumerator FinishStageEntry(Action enterStage, StaminaBarUI bar, float duration, GameObject blocker, int sceneHandle)
        {
            try
            {
                yield return new WaitForSecondsRealtime(duration);

                while (bar && bar.IsSpending)
                {
                    yield return null;
                }

                if (SceneManager.GetActiveScene().handle == sceneHandle)
                {
                    enterStage();
                }
            }
            finally
            {
                IsEnteringStage = false;

                if (blocker)
                {
                    blocker.SetActive(false);
                    Destroy(blocker);
                }
            }
        }

        public bool AddAdditionalStamina(int amount)
        {
            if (!_isInitialized || amount <= 0)
            {
                return false;
            }

            var data = GameManager.Instance.staminaData;

            if ((long)data.additionalStamina + amount > int.MaxValue - MaxStamina)
            {
                return false;
            }

            data.additionalStamina += amount;
            GameManager.Instance.SaveStaminaData();
            OnStaminaChanged?.Invoke();

            return true;
        }

        public TimeSpan GetRemainingRecoveryTime() => !_isInitialized
            ? StaminaRules.RecoveryInterval
            : StaminaRules.Remaining(GameManager.Instance.staminaData, EstimatedServerTime);

        public float RecoveryProgress => !_isInitialized || GameManager.Instance.staminaData.stamina >= MaxStamina
            ? 0f : Mathf.Clamp01(1f - (float)(GetRemainingRecoveryTime().TotalSeconds / StaminaRules.RecoveryInterval.TotalSeconds));
    }
}
