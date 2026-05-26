using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    [RequireComponent(typeof(Image))]
    public class UIImageGlowRect : MonoBehaviour
    {
        [SerializeField] private Color startColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color endColor = Color.white;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private bool loop = false;

        private Image image;
        private float timer;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            timer = 0f;
            image.color = startColor;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            float t = loop
                ? Mathf.PingPong(timer / duration, 1f)
                : Mathf.Clamp01(timer / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            image.color = Color.Lerp(startColor, endColor, t);
        }
    }
}
