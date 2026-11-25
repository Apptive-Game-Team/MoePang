using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _01.Scripts._08.Utility
{
    [DisallowMultipleComponent]
    public sealed class SceneFade : MonoBehaviour
    {
        private const float CoverSeconds = 0.25f;
        private const float RevealSeconds = 0.30f;

        private static readonly Color FadeColor = new Color32(166, 204, 142, 255);

        private static SceneFade _instance;
        private CanvasGroup _cover;
        private RawImage[] _foliage;
        private float _foliageProgress;

        private static readonly Vector2[] FoliageOrigins =
        {
            new Vector2(-0.06f, 0.83f), new Vector2(0.03f, 0.68f),
            new Vector2(-0.09f, 0.43f), new Vector2(0.07f, 0.22f),
            new Vector2(0.18f, 0.91f), new Vector2(0.65f, 0.82f),
            new Vector2(0.74f, 0.56f), new Vector2(0.70f, 0.31f),
            new Vector2(0.54f, 0.13f), new Vector2(0.81f, 0.08f),
        };

        public static bool IsTransitioning => _instance != null && _instance._cover.blocksRaycasts;

        public static bool Load(string sceneName, Action<Scene, LoadSceneMode> onLoaded = null, Action onFailed = null)
        {
            if (IsTransitioning)
            {
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new ArgumentException($"Scene '{sceneName}' is not enabled in Build Settings.", nameof(sceneName));
            }

            if (_instance == null)
            {
                var root = new GameObject("Scene Fade", typeof(RectTransform), typeof(Canvas),
                    typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(SceneFade));
                _instance = root.GetComponent<SceneFade>();
                DontDestroyOnLoad(root);
            }

            _instance._foliageProgress = 0f;
            _instance._cover.blocksRaycasts = true;
            _instance.UpdateFoliage();
            _instance.StartCoroutine(_instance.Transition(sceneName, onLoaded, onFailed));
            return true;
        }

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            _cover = GetComponent<CanvasGroup>();
            _cover.alpha = 0f;
            _cover.blocksRaycasts = false;

            var surface = new GameObject("Fade Surface", typeof(RectTransform), typeof(Image));
            var image = surface.GetComponent<Image>();
            RectTransform rect = image.rectTransform;
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.color = FadeColor;
            CreateFoliage();
        }

        private void CreateFoliage()
        {
            var leaf = Resources.Load<Texture2D>("SceneFade/Leaf");
            var grass = Resources.Load<Texture2D>("SceneFade/Grass");
            if (leaf == null || grass == null)
            {
                return;
            }

            _foliage = new RawImage[FoliageOrigins.Length];
            for (int index = 0; index < _foliage.Length; index++)
            {
                var item = new GameObject("Wind Foliage", typeof(RectTransform), typeof(RawImage));
                item.transform.SetParent(transform, false);

                RawImage graphic = item.GetComponent<RawImage>();
                graphic.texture = index % 3 == 1 ? grass : leaf;
                graphic.raycastTarget = false;
                _foliage[index] = graphic;
            }
        }

        private void Update()
        {
            if (!_cover.blocksRaycasts)
            {
                return;
            }

            _foliageProgress = Mathf.Min(1f, _foliageProgress + Time.unscaledDeltaTime / 1.15f);
            UpdateFoliage();
        }

        private void UpdateFoliage()
        {
            if (_foliage == null)
            {
                return;
            }

            float screenHeight = ((RectTransform)transform).rect.height;
            for (int index = 0; index < _foliage.Length; index++)
            {
                RawImage graphic = _foliage[index];
                RectTransform rect = graphic.rectTransform;
                float t = _foliageProgress;
                float angle = index * 1.37f + t * 8f;
                Vector2 point = FoliageOrigins[index] + new Vector2(
                    t * (0.30f + index % 3 * 0.075f),
                    t * 0.12f + Mathf.Sin(angle) * 0.023f);

                rect.anchorMin = point;
                rect.anchorMax = point;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.one * screenHeight * (0.018f + index % 4 * 0.0045f);
                rect.localRotation = Quaternion.Euler(0f, 0f, -37f + Mathf.Sin(angle) * 65f);

                Color color = index % 3 == 0 ? new Color32(245, 245, 192, 255) : new Color32(80, 132, 77, 255);
                color.a = (index % 3 == 0 ? 0.64f : 0.50f) * (1f - Mathf.SmoothStep(0f, 1f, (t - 0.7f) / 0.3f));
                graphic.color = color;
            }
        }

        private IEnumerator Transition(string sceneName, Action<Scene, LoadSceneMode> onLoaded, Action onFailed)
        {
            yield return Fade(0f, 1f, CoverSeconds);
            yield return null;

            string expectedName = System.IO.Path.GetFileNameWithoutExtension(sceneName.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                ? sceneName : sceneName + ".unity");

            void HandleLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name == expectedName && mode == LoadSceneMode.Single)
                {
                    onLoaded?.Invoke(scene, mode);
                }
            }

            SceneManager.sceneLoaded += HandleLoaded;
            try
            {
                AsyncOperation operation = BeginLoad(sceneName);
                if (operation == null)
                {
                    onFailed?.Invoke();
                }
                else
                {
                    yield return operation;
                    yield return null;
                }

                yield return Fade(1f, 0f, RevealSeconds);
            }
            finally
            {
                SceneManager.sceneLoaded -= HandleLoaded;
                _cover.alpha = 0f;
                _cover.blocksRaycasts = false;
            }
        }

        private static AsyncOperation BeginLoad(string sceneName)
        {
            try
            {
                return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }

        private IEnumerator Fade(float start, float end, float seconds)
        {
            float elapsed = 0f;
            _cover.alpha = start;

            while (elapsed < seconds)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
                _cover.alpha = Mathf.SmoothStep(start, end, Mathf.Clamp01(elapsed / seconds));
            }

            _cover.alpha = end;
        }
    }
}
