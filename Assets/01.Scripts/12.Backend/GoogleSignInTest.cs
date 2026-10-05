using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using System.Threading.Tasks;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._12.Backend
{
    public class GoogleSignInTest : MonoBehaviour
    {
        [SerializeField] private string webClientId;

        private GoogleSignInConfiguration _configuration;
        private Task<GoogleSignInUser> _googleSignInTask;

        private void Awake()
        {
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

        private void Update()
        {
#if UNITY_EDITOR
            return;
#endif
            
            if (_googleSignInTask is not { IsCompleted: true })
            {
                return;
            }

            Task<GoogleSignInUser> task = _googleSignInTask;
            _googleSignInTask = null;

            if (task.IsCanceled)
            {
                return;
            }

            if (task.IsFaulted)
            {
                return;
            }

            _ = LoginToSupabase(task.Result);
        }

        public void SignIn()
        {
#if UNITY_EDITOR
            return;
#endif
            
            if (_googleSignInTask is { IsCompleted: false })
            {
                return;
            }

            _googleSignInTask = GoogleSignIn.DefaultInstance.SignIn();
        }

        private async Task LoginToSupabase(GoogleSignInUser googleUser)
        {
            try
            {
                if (googleUser == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(googleUser.IdToken))
                {
                    return;
                }


                string nickname = string.IsNullOrEmpty(googleUser.DisplayName)
                    ? googleUser.Email
                    : googleUser.DisplayName;

                await SupabaseLoginManager.Instance.LoginWithGoogle(
                    googleUser.IdToken,
                    nickname
                );

                string userId = SupabaseLoginManager.Instance
                    .GetCurrentUserId();

                await GameManager.Instance.LoadData();
                
                await Task.Delay(1000);

                SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Google Login Failed : {e}");
            }
        }

        public void SignOut()
        {
            GoogleSignIn.DefaultInstance.SignOut();
            _ = SignOutFromSupabase();
        }

        private async Task SignOutFromSupabase()
        {
            try
            {
                await SupabaseLoginManager.Instance.Logout();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Google Logout Failed : {e}");
            }
        }
    }
}