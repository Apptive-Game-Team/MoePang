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
        public bool googleLoggedIn;

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
            await ExecuteGoogleLoginFlow(false);
        }
        
        public async void SignInAndMigrateLocalSave()
        {
            await ExecuteGoogleLoginFlow(true);
        }
        
        public async Task<bool> ExecuteGoogleLoginFlow(bool migrateLocalSave = false, bool forceMigrate = false)
        {
        #if UNITY_EDITOR
            return false;
        #endif

            if (_isSigningIn)
            {
                return false;
            }

            _isSigningIn = true;

            try
            {
                GoogleSignInUser googleUser =
                    await GoogleSignIn.DefaultInstance.SignIn();

                if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
                {
                    Debug.LogWarning("Google Sign-In cancelled or IdToken is empty.");
                    return false;
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
                        .MigrateLocalDataToSupabase(forceMigrate);

                    if (!migrated)
                    {
                        Debug.LogError(
                            "Local save migration failed. " +
                            "Local save remains available."
                        );

                        return false;
                    }

                    Debug.Log("Local save migration succeeded.");

                    GameManager.Instance.isLoggedIn = true;
                    googleLoggedIn = true;

                    await GameManager.Instance.LoadData();

                    SceneManager.LoadScene(
                        SceneInfo.GetSceneName(SceneType.Main)
                    );
                }
                else
                {
                    await SupabaseLoginManager.Instance.LoginWithGoogle(
                        googleUser.IdToken,
                        nickname,
                        imageUrl
                    );

                    Debug.Log("Google Sign-In and Supabase login succeeded.");

                    GameManager.Instance.isLoggedIn = true;
                    googleLoggedIn = true;

                    await GameManager.Instance.LoadData();
                }
                
                PlayerPrefs.SetInt("LoginType", 1);
                PlayerPrefs.Save();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Google Sign-In Failed: {e}");
                return false;
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
                SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Title));
            }
            catch (Exception e)
            {
                Debug.LogError($"Google Logout Failed: {e}");
            }
        }
    }
}
