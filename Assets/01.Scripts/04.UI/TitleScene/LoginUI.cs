using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using _01.Scripts._12.Backend;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._04.UI.TitleScene
{
    public class LoginUI : MonoBehaviour
    {
        private GoogleSignInTest _googleSignIn;

        private void Start()
        {
            _googleSignIn = FindAnyObjectByType<GoogleSignInTest>();
        }
        
        public void GoogleLogin()
        {
            _googleSignIn.SignIn();
        }

        public async void EmailLogin()
        {
            await SupabaseLoginManager.Instance.StartGame();
            
            await Task.Delay(1000);

            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }

        public async void LocalLogin()
        {
            await GameManager.Instance.LoadData();
            
            await Task.Delay(1000);

            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }
    }
}
