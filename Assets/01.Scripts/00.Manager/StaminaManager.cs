using _01.Scripts._08.Utility;
using System;
using System.Threading.Tasks;
using UnityEngine;
using _01.Scripts._12.Backend;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;

namespace _01.Scripts._00.Manager
{
    public class StaminaManager : SingletonObject<StaminaManager>
    {
        private const int MaxStamina = 100;
        private const int RecoveryAmount = 5;
        private const int ConsumeAmount = 1;

        private static readonly TimeSpan RecoveryInterval = TimeSpan.FromMinutes(30);

        private DateTime _serverTime;
        private bool _isInitialized;
        private bool _isFirstSceneLoad = true;
        private Coroutine _staminaCoroutine;

        public event Action OnStaminaChanged;

        protected async override void Awake()
        {
            base.Awake();

            if (IsDuplicate)
            {
                return;
            }
            
            await InitializeStamina();
            
            if (_isInitialized)
            {
                StartCoroutine(StaminaRecoveryCoroutine());
            }
        }

        private async void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != SceneInfo.GetSceneName(SceneType.Main))
            {
                return;
            }

            if (_isFirstSceneLoad)
            {
                _isFirstSceneLoad = false;
                return;
            }

            await RecoverStamina();
        }
        
        private async Task InitializeStamina()
        {
            try
            {
                StaminaData data = GameManager.Instance.staminaData;
                
                _serverTime = await GetServerTimeAsync();
                
                RecoverStaminaInternal(data, _serverTime);

                _isInitialized = true;

                Debug.Log($"Stamina Initialized : {data.stamina}/{MaxStamina}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Stamina Initialized Failed : {e}");
            }
        }
        
        private async Task<DateTime> GetServerTimeAsync()
        {
            var result = await SupabaseManager.Instance.Client.Rpc("get_server_time", new { });

            return JsonConvert.DeserializeObject<DateTime>(result.Content).ToUniversalTime();
        }
        
        private void RecoverStaminaInternal(StaminaData data, DateTime serverTime)
        {
            if (data.stamina >= MaxStamina)
            {
                data.stamina = MaxStamina;
                data.lastSavedDateTime = serverTime;
                return;
            }

            DateTime lastRecoveryTime = data.lastSavedDateTime.ToUniversalTime();
            
            if (serverTime.Kind != DateTimeKind.Utc)
            {
                serverTime = serverTime.ToUniversalTime();
            }
            
            if (serverTime < lastRecoveryTime)
            {
                Debug.LogWarning("Server Time < Last Recovery Time. Fuck");

                return;
            }
            
            TimeSpan elapsed = serverTime - lastRecoveryTime;

            int recoveryCount = (int)(elapsed.Ticks / RecoveryInterval.Ticks);

            if (recoveryCount <= 0)
            {
                return;
            }

            int previousStamina = data.stamina;

            data.stamina = Mathf.Min(data.stamina + recoveryCount * RecoveryAmount, MaxStamina);
            
            data.lastSavedDateTime = data.stamina >= MaxStamina 
                ? serverTime 
                : lastRecoveryTime.AddTicks(recoveryCount * RecoveryInterval.Ticks);

            GameManager.Instance.SaveStaminaData();
            
            OnStaminaChanged?.Invoke();

            if (data.stamina != previousStamina)
            {
                Debug.Log($"Stamina Recover: {previousStamina} -> {data.stamina}");
            }
        }
        
        public async Task RecoverStamina()
        {
            if (!_isInitialized)
            {
                return;
            }

            try
            {
                StaminaData data = GameManager.Instance.staminaData;

                _serverTime = await GetServerTimeAsync();

                RecoverStaminaInternal(data, _serverTime);
            }
            catch (Exception e)
            {
                Debug.LogError($"Stamina Recovery Failed : {e}");
            }
        }
        
        private IEnumerator StaminaRecoveryCoroutine()
        {
            while (true)
            {
                if (SceneManager.GetActiveScene().name != SceneInfo.GetSceneName(SceneType.Main))
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }
                
                Task recoveryTask = RecoverStamina();

                yield return new WaitUntil(() => recoveryTask.IsCompleted);
                
                TimeSpan remainingTime = GetRemainingRecoveryTime();
                
                Debug.Log($"Stamina Recovery Remaining Time : {remainingTime}");

                if (remainingTime <= TimeSpan.Zero)
                {
                    yield return null;
                    continue;
                }
                
                yield return new WaitForSecondsRealtime((float)remainingTime.TotalSeconds);
            }
        }
        
        public bool ConsumeStamina()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("StaminaManager Not Initialized");
                return false;
            }

            StaminaData data = GameManager.Instance.staminaData;

            if (data.stamina < ConsumeAmount)
            {
                return false;
            }

            data.stamina -= ConsumeAmount;
            
            if (data.stamina == MaxStamina - ConsumeAmount)
            {
                data.lastSavedDateTime = _serverTime;
            }
            
            GameManager.Instance.SaveStaminaData();
            
            OnStaminaChanged?.Invoke();

            return true;
        }
        
        public TimeSpan GetRemainingRecoveryTime()
        {
            StaminaData data = GameManager.Instance.staminaData;

            if (data.stamina >= MaxStamina)
            {
                return TimeSpan.Zero;
            }

            DateTime serverTime = _serverTime.ToUniversalTime();
            DateTime lastRecoveryTime = data.lastSavedDateTime.ToUniversalTime();

            TimeSpan elapsed = serverTime - lastRecoveryTime;
            TimeSpan remaining = RecoveryInterval - elapsed;

            return remaining > TimeSpan.Zero
                ? remaining
                : TimeSpan.Zero;
        }
    }
}