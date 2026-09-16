using System;
using System.Threading.Tasks;
using UnityEngine;
using _01.Scripts._00.Manager;

namespace _01.Scripts._12.Backend
{
    public class SupabaseLoginManager : SingletonObject<SupabaseLoginManager>
    {
        [SerializeField] private string loginId;
        [SerializeField] private string password;
        [SerializeField] private string nickName;

        private AuthRepository _authRepository;
        private ProfileRepository _profileRepository;
        private SupabaseDataRepository _dataRepository;

        public bool IsAuthenticated { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            _authRepository = new AuthRepository();
            _profileRepository = new ProfileRepository();
            _dataRepository = new SupabaseDataRepository();
        }

        private async void Start()
        {
            await StartGame();
        }

        private async Task StartGame()
        {
            try
            {
                await SupabaseManager.Instance.InitializationTask;
                
                Debug.Log("Supabase login process started.");

                await LoginOrRegister(loginId, password);

                if (!IsAuthenticated)
                {
                    Debug.LogError("Authentication failed.");
                    return;
                }

                Debug.Log("Supabase authentication and data loading completed.");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Game start failed.\n{e}"
                );
            }
        }

        public async Task LoginOrRegister(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is empty.");
            }

            try
            {
                await _authRepository.Login(
                    email,
                    password
                );

                Debug.Log("Existing account login success.");

                IsAuthenticated = true;
                
                await LoadGameData();
            }
            catch (Exception loginException)
            {
                Debug.LogWarning(
                    $"Login failed. Try sign up.\n{loginException}"
                );

                try
                {
                    await _authRepository.SignUp(
                        email,
                        password
                    );

                    Debug.Log("New account sign up success.");

                    IsAuthenticated = true;
                    
                    await _profileRepository.CreateProfile(nickName);
                    await _dataRepository.CreateInitialGameData();
                    
                    await LoadGameData();
                }
                catch (Exception signUpException)
                {
                    IsAuthenticated = false;

                    Debug.Log(
                        "Login and sign up both failed.\n" +
                        $"Login Error:\n{loginException}\n\n" +
                        $"Sign Up Error:\n{signUpException}" + 
                        "Change to Local Data"
                    );
                    
                    await LoadGameData();

                    throw;
                }
            }
        }

        private async Task LoadGameData()
        {
            await GameManager.Instance.LoadData();

            Debug.Log("Game data loaded successfully.");
        }
    }
}