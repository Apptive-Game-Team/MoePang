using _01.Scripts._00.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene
{
    [DisallowMultipleComponent]
    public sealed class MeadowLobbyBackground : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float holdSeconds = 6f;
        [SerializeField, Min(0.01f)] private float transitionSeconds = 2f;

        private static readonly string[] PhaseNames = { "Morning", "Day", "Sunset", "Night" };

        private readonly Sprite[] _backgrounds = new Sprite[PhaseNames.Length];
        private readonly Sprite[] _ambience = new Sprite[PhaseNames.Length];
        private Image _background;
        private Image _incomingBackground;
        private Image _currentAmbient;
        private Image _incomingAmbient;
        private Sprite _originalSprite;
        private bool _originalPreserveAspect;
        private float _elapsed;
        private bool _showingMeadow;

        private void Update()
        {
            var stageManager = StageManager.Instance;
            if (stageManager == null || (stageManager.CurrentStage / 10) % 5 != 0)
            {
                RestoreBackground();
                return;
            }

            if (_background == null && !TryInitialize())
            {
                return;
            }

            _showingMeadow = true;
            RenderPhase();
            _elapsed += Time.unscaledDeltaTime;
        }

        private bool TryInitialize()
        {
            foreach (Image image in FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                if (image.gameObject.scene == gameObject.scene && image.sprite != null &&
                    image.sprite.name == "mainbackground_0")
                {
                    _background = image;
                    break;
                }
            }

            if (_background == null)
            {
                return false;
            }

            for (int index = 0; index < PhaseNames.Length; index++)
            {
                string path = "MeadowLobby/" + PhaseNames[index];
                _backgrounds[index] = LoadSprite(path);
                _ambience[index] = LoadSprite(path + "Ambient");

                if (_backgrounds[index] == null || _ambience[index] == null)
                {
                    enabled = false;
                    return false;
                }
            }

            _originalSprite = _background.sprite;
            _originalPreserveAspect = _background.preserveAspect;
            _incomingBackground = CreateLayer("Incoming sky");
            _currentAmbient = CreateLayer("Current ambience");
            _incomingAmbient = CreateLayer("Incoming ambience");
            return true;
        }

        private static Sprite LoadSprite(string path)
        {
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        private Image CreateLayer(string layerName)
        {
            var layer = new GameObject(layerName, typeof(RectTransform), typeof(Image));
            var image = layer.GetComponent<Image>();
            RectTransform rect = image.rectTransform;
            rect.SetParent(_background.rectTransform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.raycastTarget = false;
            return image;
        }

        private void RenderPhase()
        {
            float phaseSeconds = holdSeconds + transitionSeconds;
            _elapsed = Mathf.Repeat(_elapsed, phaseSeconds * PhaseNames.Length);
            int current = Mathf.FloorToInt(_elapsed / phaseSeconds);
            int next = (current + 1) % PhaseNames.Length;
            float progress = Mathf.Clamp01((_elapsed - current * phaseSeconds - holdSeconds) / transitionSeconds);
            float blend = Mathf.SmoothStep(0f, 1f, progress);

            _background.sprite = _backgrounds[current];
            _background.preserveAspect = false;
            _incomingBackground.sprite = _backgrounds[next];
            _incomingBackground.color = new Color(1f, 1f, 1f, blend);
            _incomingBackground.enabled = blend > 0f;
            RenderAmbient(_currentAmbient, current, 1f - blend);
            RenderAmbient(_incomingAmbient, next, blend);
        }

        private void RenderAmbient(Image image, int phase, float opacity)
        {
            image.sprite = _ambience[phase];
            float motion = Time.unscaledTime * (phase == 3 ? 0.8f : 0.47f) + phase;
            float alpha = phase == 3 ? 0.7f + 0.3f * Mathf.Sin(motion * 2f) : 1f;
            image.color = new Color(1f, 1f, 1f, alpha * opacity);
            image.rectTransform.anchoredPosition = new Vector2(14f * Mathf.Sin(motion), 8f * Mathf.Cos(motion));
            image.enabled = opacity > 0f;
        }

        private void RestoreBackground()
        {
            if (!_showingMeadow || _background == null)
            {
                return;
            }

            _background.sprite = _originalSprite;
            _background.preserveAspect = _originalPreserveAspect;
            _incomingBackground.enabled = false;
            _currentAmbient.enabled = false;
            _incomingAmbient.enabled = false;
            _elapsed = 0f;
            _showingMeadow = false;
        }

        private void OnDisable()
        {
            RestoreBackground();
        }

        private void OnDestroy()
        {
            for (int index = 0; index < PhaseNames.Length; index++)
            {
                Destroy(_backgrounds[index]);
                Destroy(_ambience[index]);
            }
        }
    }
}

