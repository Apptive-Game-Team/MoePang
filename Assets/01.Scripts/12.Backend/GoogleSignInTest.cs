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
        [SerializeField] private TextMeshProUGUI resultText;

        private GoogleSignInConfiguration _configuration;
        private Task<GoogleSignInUser> _googleSignInTask;

        private void Awake()
        {
            _configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true,
                RequestEmail = true,
                UseGameSignIn = false
            };

            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.DefaultInstance.EnableDebugLogging(true);

            resultText.text = "Google Sign-In Ready";
        }

        private void Update()
        {
            if (_googleSignInTask == null || !_googleSignInTask.IsCompleted)
                return;

            Task<GoogleSignInUser> task = _googleSignInTask;
            _googleSignInTask = null;

            if (task.IsCanceled)
            {
                resultText.text = "Google Sign-In Canceled";
                return;
            }

            if (task.IsFaulted)
            {
                resultText.text = $"Google Sign-In Failed\n\n{task.Exception}";
                return;
            }

            _ = LoginToSupabase(task.Result);
        }

        public void SignIn()
        {
            if (_googleSignInTask != null && !_googleSignInTask.IsCompleted)
            {
                resultText.text = "Already Google Logined.";
                return;
            }

            resultText.text = "Google Sign-In Start...";

            _googleSignInTask = GoogleSignIn.DefaultInstance.SignIn();
        }

        private async Task LoginToSupabase(GoogleSignInUser googleUser)
        {
            try
            {
                if (googleUser == null)
                {
                    resultText.text = "Google User is Null.";
                    return;
                }

                if (string.IsNullOrEmpty(googleUser.IdToken))
                {
                    resultText.text = "Google IdToken is Empty.";
                    return;
                }

                resultText.text = "Google Login Success\nSupabase Logining...";

                string nickname = string.IsNullOrEmpty(googleUser.DisplayName)
                    ? googleUser.Email
                    : googleUser.DisplayName;

                await SupabaseLoginManager.Instance.LoginWithGoogle(
                    googleUser.IdToken,
                    nickname
                );

                string userId = SupabaseLoginManager.Instance
                    .GetCurrentUserId();

                resultText.text =
                    "===== Google Login Success =====\n" +
                    $"Email : {googleUser.Email}\n" +
                    $"Nickname : {nickname}\n" +
                    $"Supabase UID : {userId}\n" +
                    "Profile : Created\n" +
                    "==============================";
                
                await Task.Delay(3000);

                SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Title));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Google Login Failed : {e}");

                resultText.text =
                    "Google/Supabase login failed\n\n" +
                    e;
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

                resultText.text = "Google + Supabase Sign-Out Completed";
            }
            catch (System.Exception e)
            {
                resultText.text =
                    $"Sign-Out Failed\n\n{e}";
            }
        }
    }
}