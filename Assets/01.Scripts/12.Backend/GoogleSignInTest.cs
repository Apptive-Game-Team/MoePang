using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using System;
using System.Threading.Tasks;
using Google;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._12.Backend
{
    public class GoogleSignInTest : SingletonObject<GoogleSignInTest>
    {
        [SerializeField] private string webClientId;

        private GoogleSignInConfiguration _configuration;
        private bool _isSigningIn;

        protected override void Awake()
        {
            base.Awake();
            
#if UNITY_EDITOR
            return;
#endif
            _configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true,
                RequestEmail = true,
                UseGameSignIn = false
            };

            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.DefaultInstance.EnableDebugLogging(true);
        }
        
        public async void SignIn()
        {
            await ExecuteGoogleFlow(false);
        }
        
        public async void SignInAndMigrateLocalSave()
        {
            await ExecuteGoogleFlow(true);
        }
        
        private async Task ExecuteGoogleFlow(bool migrateLocalSave)
        {
#if UNITY_EDITOR
            return;
#endif

            if (_isSigningIn)
            {
                return;
            }

            _isSigningIn = true;

            try
            {
                GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

                if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
                {
                    Debug.LogError("Google Sign-In failed: IdToken is empty.");
                    return;
                }

                string nickname = string.IsNullOrEmpty(googleUser.DisplayName)
                    ? googleUser.Email
                    : googleUser.DisplayName;

                string imageUrl = googleUser.ImageUrl?.ToString();
                
                if (migrateLocalSave)
                {
                    await SupabaseLoginManager.Instance
                        .LinkGoogleForLocalMigration(
                            googleUser.IdToken,
                            nickname,
                            imageUrl
                        );

                    bool migrated = await GameManager.Instance
                        .MigrateLocalDataToSupabase();

                    if (!migrated)
                    {
                        Debug.LogError(
                            "Local save migration failed. " +
                            "Local save remains available."
                        );
                        return;
                    }

                    Debug.Log("Local save migration succeeded.");
                    
                    GameManager.Instance.isLoggedIn = true;
                }
                else
                {
                    await SupabaseLoginManager.Instance.LoginWithGoogle(
                        googleUser.IdToken,
                        nickname,
                        imageUrl
                    );

                    Debug.Log("Google Sign-In and Supabase login succeeded.");
                }

                await GameManager.Instance.LoadData();
                await Task.Delay(1000);
                SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
            }
            catch (Exception e)
            {
                Debug.LogError($"Google Sign-In Failed: {e}");
            }
            finally
            {
                _isSigningIn = false;
            }
        }

        public void SignOut()
        {
#if !UNITY_EDITOR
            GoogleSignIn.DefaultInstance.SignOut();
#endif
            _ = SignOutFromSupabase();
        }

        private async Task SignOutFromSupabase()
        {
            try
            {
                await SupabaseLoginManager.Instance.Logout();
            }
            catch (Exception e)
            {
                Debug.LogError($"Google Logout Failed: {e}");
            }
        }
    }
}
