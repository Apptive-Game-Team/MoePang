using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Stamina
{
    public class StaminaSpendEffect : MonoBehaviour
    {
        public bool IsComplete { get; private set; }

        public static StaminaSpendEffect Play(Image[] apples, Color[] colors, int firstIndex, int count,
            float duration, float riseHeight)
        {
            if (count <= 0 || !Application.isPlaying)
            {
                return null;
            }

            var root = new GameObject("StaminaSpendEffect", typeof(RectTransform), typeof(Canvas));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;

            var copies = new Image[count];
            var starts = new Vector2[count];
            var rises = new float[count];
            var corners = new Vector3[4];

            for (int n = 0; n < count; n++)
            {
                int index = firstIndex + n;

                if (index < 0 || index >= apples.Length || !apples[index])
                {
                    continue;
                }

                Image source = apples[index];
                Canvas sourceCanvas = source.canvas;
                Camera camera = sourceCanvas && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? sourceCanvas.worldCamera : null;
                source.rectTransform.GetWorldCorners(corners);

                Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                Vector2 topRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                Vector2 size = topRight - bottomLeft;

                var copyObject = new GameObject("SpentApple" + (index + 1), typeof(RectTransform), typeof(Image));
                copyObject.transform.SetParent(root.transform, false);
                Image copy = copyObject.GetComponent<Image>();
                copy.sprite = source.sprite;
                copy.color = colors[index];
                copy.preserveAspect = source.preserveAspect;
                copy.raycastTarget = false;

                RectTransform rect = copy.rectTransform;
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = starts[n] = (bottomLeft + topRight) * 0.5f;

                rises[n] = size.y * riseHeight;
                copies[n] = copy;
            }

            var effect = root.AddComponent<StaminaSpendEffect>();
            effect.StartCoroutine(effect.Animate(root, copies, starts, rises, Mathf.Max(0.1f, duration)));

            return effect;
        }

        internal static void Sample(float time, out float lift, out float scale, out float alpha)
        {
            float t = Mathf.Clamp01(time);
            lift = 1f - Mathf.Pow(1f - t, 3f);

            float pop = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, t));
            scale = (1f + 0.12f * Mathf.Sin(t * Mathf.PI)) * (1f - pop);

            alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.75f, 1f, t));
        }

        private IEnumerator Animate(GameObject root, Image[] copies, Vector2[] starts, float[] rises, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                for (int i = 0; i < copies.Length; i++)
                {
                    if (!copies[i])
                    {
                        continue;
                    }

                    Sample(elapsed / duration, out float lift, out float scale, out float alpha);
                    copies[i].rectTransform.anchoredPosition = starts[i] + Vector2.up * (rises[i] * lift);
                    copies[i].rectTransform.localScale = Vector3.one * scale;

                    Color color = copies[i].color;
                    color.a = alpha;
                    copies[i].color = color;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            IsComplete = true;
            root.SetActive(false);
            Destroy(root);
        }
    }
}
