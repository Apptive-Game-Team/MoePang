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
        public ProfileRepository ProfileRepository;
        private SupabaseDataRepository _dataRepository;

        public Profile profile;

        public bool IsAuthenticated { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            _authRepository = new AuthRepository();
            ProfileRepository = new ProfileRepository();
            _dataRepository = new SupabaseDataRepository();
        }

        public async Task StartGame()
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
                
                profile = await ProfileRepository.GetProfile();

                if (profile != null && profile.Nickname != nickName)
                {
                    await ProfileRepository.UpdateNickname(nickName);
                    Debug.Log($"Nickname updated: {profile.Nickname} -> {nickName}");
                }
                
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
                    
                    await ProfileRepository.CreateProfile(nickName);
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
        
        public async Task LoginWithGoogle(string idToken, string nickname, string imageUrl)
        {
            try
            {
                await SupabaseManager.Instance.InitializationTask;

                Debug.Log("Google Supabase login started.");

                await _authRepository.LoginWithGoogle(idToken);

                if (!_authRepository.IsLoggedIn())
                {
                    IsAuthenticated = false;

                    throw new Exception(
                        "Google authentication failed."
                    );
                }

                IsAuthenticated = true;

                string userId = _authRepository.GetCurrentUserId();

                Debug.Log(
                    $"Google authentication success. UserId: {userId}"
                );

                bool profileExists = await ProfileRepository.Exists();

                if (!profileExists)
                {
                    Debug.Log("Profile does not exist. Creating profile...");

                    bool profileCreated =
                        await ProfileRepository.CreateProfile(nickname, imageUrl);

                    if (!profileCreated)
                    {
                        throw new Exception(
                            "Failed to create Google user profile."
                        );
                    }

                    Debug.Log("Google user profile created.");
                }
                else
                {
                    Debug.Log("Profile already exists. Skip profile creation.");
                }
                
                profile = await ProfileRepository.GetProfile();

                if (profile == null)
                {
                    throw new Exception(
                        "Failed to load Google user profile."
                    );
                }

                if (profile.GoogleAvatarUrl == null)
                {
                    await SupabaseManager.Instance.Client
                        .From<Profile>()
                        .Set(x => x.GoogleAvatarUrl, imageUrl)
                        .Where(x => x.Id == userId)
                        .Update();
                    
                    profile.GoogleAvatarUrl = imageUrl;
                }

                Debug.Log(
                    $"Profile loaded. Nickname: {profile.Nickname}, AvatarId: {profile.AvatarId}"
                );

                await _dataRepository.CreateInitialGameData();

                Debug.Log(
                    "Google authentication and data loading completed."
                );
            }
            catch (Exception e)
            {
                IsAuthenticated = false;

                Debug.LogError(
                    $"Google login failed.\n{e}"
                );

                throw;
            }
        }
        
        public string GetCurrentUserId()
        {
            return _authRepository.GetCurrentUserId();
        }

        public async Task Logout()
        {
            await _authRepository.Logout();
            IsAuthenticated = false;
        }
    }
}