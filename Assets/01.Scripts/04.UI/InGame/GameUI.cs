using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts._04.UI.InGame
{
    public abstract class GameUI : MonoBehaviour
    {
        protected DepthOfField DepthOfField;

        private void Awake()
        {
            if (Camera.main.GetComponent<Volume>().profile.TryGet(out DepthOfField dof))
            {
                DepthOfField = dof;
            }
            gameObject.SetActive(false);
        }

        protected abstract void OnEnable();
        
        private void OnDisable()
        {
            Time.timeScale = 1;
            DepthOfField.active = false;
        }
    }
}
