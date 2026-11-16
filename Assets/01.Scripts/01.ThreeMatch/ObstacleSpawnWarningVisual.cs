using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public sealed class ObstacleSpawnWarningVisual : MonoBehaviour
    {
        private SpriteRenderer _boundary;
        private SpriteRenderer _lightBloom;
        private SpriteRenderer _beacon;
        private static readonly Texture2D[] Textures = new Texture2D[3];
        private static readonly Sprite[] Sprites = new Sprite[3];
        private static readonly Color Amber = new(1f, 0.66f, 0.12f);
        private static readonly Color Red = new(1f, 0.18f, 0.08f);
        private static bool _sharedResourcesReady;
        private float _size;
        private float _duration;
        private float _elapsed;
        private float _finishElapsed;
        private bool _finishing;
        private bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedResources()
        {
            ReleaseSharedResources();
            Application.quitting -= ReleaseSharedResources;
            Application.quitting += ReleaseSharedResources;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateSharedResourcesOnce()
        {
            if (_sharedResourcesReady)
            {
                return;
            }
            for (int kind = 0; kind < Sprites.Length; kind++)
            {
                Textures[kind] = MakeTexture(kind);
                Textures[kind].hideFlags = HideFlags.HideAndDontSave;
                Sprites[kind] = Sprite.Create(Textures[kind], new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 256);
                Sprites[kind].hideFlags = HideFlags.HideAndDontSave;
            }
            _sharedResourcesReady = true;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= ReleaseSharedResources;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseSharedResources;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                ReleaseSharedResources();
            }
        }
#endif

        private static void ReleaseSharedResources()
        {
            _sharedResourcesReady = false;
            for (int i = 0; i < Sprites.Length; i++)
            {
                if (Sprites[i])
                {
                    DestroyImmediate(Sprites[i]);
                }
                if (Textures[i])
                {
                    DestroyImmediate(Textures[i]);
                }
                Sprites[i] = null;
                Textures[i] = null;
            }
        }

        public void Initialize(float cellSize, float warningDuration, int sortingLayer, int order)
        {
            if (!_sharedResourcesReady)
            {
                throw new System.InvalidOperationException("Warning sprites must be built once at game startup.");
            }
            _size = cellSize;
            _duration = Mathf.Max(0.001f, warningDuration);
            transform.localScale = Vector3.one;
            _boundary = CreateRendererUsingSharedSprite(0, "Construction perimeter", sortingLayer, order);
            _lightBloom = CreateRendererUsingSharedSprite(1, "Amber light flare", sortingLayer, order + 1);
            _beacon = CreateRendererUsingSharedSprite(2, "Site warning beacon", sortingLayer, order + 2);
            _initialized = true;
            Render();
        }

        private SpriteRenderer CreateRendererUsingSharedSprite(int kind, string label, int layer, int order)
        {
            // No texture or sprite creation is allowed on the warning spawn path.
            GameObject child = new(label);
            child.transform.SetParent(transform, false);
            SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = Sprites[kind];
            sr.sortingLayerID = layer;
            sr.sortingOrder = order;
            return sr;
        }

        private static Texture2D MakeTexture(int kind)
        {
            const int resolution = 256;
            Texture2D texture = new(resolution, resolution, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color[] pixels = new Color[resolution * resolution];
            Color ink = new(0.20f, 0.20f, 0.20f);
            Color amber = Color.white;
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                float px = (x + 0.5f) / resolution - 0.5f;
                float py = (y + 0.5f) / resolution - 0.5f;
                Color c = Color.clear;
                switch (kind)
                {
                    case 0:
                        {
                            Vector2 q = new Vector2(Mathf.Abs(px), Mathf.Abs(py)) - new Vector2(0.365f, 0.365f);
                            float sdf = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - 0.067f;
                            float d = Mathf.Abs(sdf);
                            c = ink;
                            if (d < 0.023f)
                            {
                                bool stripe = Mathf.Abs(py) > 0.395f && Mathf.Abs(px) < 0.33f && Mathf.Repeat((px + py) * 9f, 1f) < 0.43f;
                                c = stripe ? ink : amber;
                            }
                            c.a = 1f - Mathf.SmoothStep(0.035f, 0.043f, d);
                            break;
                        }
                    case 1:
                        {
                            float r = new Vector2(px, py).magnitude;
                            float ring = Mathf.Exp(-Mathf.Pow((r - 0.30f) / 0.095f, 2f));
                            float core = Mathf.Exp(-r * r / 0.065f);
                            c = new Color(1f, 1f, 1f, (ring * 0.64f + core * 0.18f) * (1f - Mathf.SmoothStep(0.40f, 0.50f, r)));
                            break;
                        }
                    default:
                        {
                            float bx = Mathf.Abs(px);
                            float dome = new Vector2(px, py - 0.427f).magnitude;
                            if (bx < 0.081f && py is > 0.368f and < 0.401f)
                            {
                                c = ink;
                            }
                            if ((dome < 0.066f && py >= 0.411f) || (bx < 0.066f && py is >= 0.393f and < 0.427f))
                            {
                                c = ink;
                            }
                            if ((dome < 0.048f && py >= 0.415f) || (bx < 0.048f && py is >= 0.408f and < 0.427f))
                            {
                                c = amber;
                            }
                            if (new Vector2(px + 0.017f, py - 0.446f).magnitude < 0.014f)
                            {
                                c = Color.white;
                            }

                            break;
                        }
                }
                pixels[y * resolution + x] = c;
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public void Consume()
        {
            _finishing = true;
            _finishElapsed = 0f;
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }
            if (_finishing)
            {
                _finishElapsed += TutorialPuzzleClock.DeltaTime;
                if (_finishElapsed >= 0.18f)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                _elapsed += TutorialPuzzleClock.DeltaTime;
            }
            Render();
        }

        private void Render()
        {
            float charge = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_elapsed / _duration));
            float finish = _finishing ? Mathf.Clamp01(_finishElapsed / 0.18f) : 0f;
            float arrival = Mathf.Clamp01(_elapsed / 0.52f);
            float flash = 1f - Mathf.SmoothStep(0.10f, 0.52f, _elapsed);
            float pulse = 0.5f + 0.5f * Mathf.Cos(_elapsed * 5.8f);
            float fade = 1f - finish;
            float contraction = 1f - finish * finish * 0.70f;
            Color tint = Color.Lerp(Amber, Red, charge);
            _boundary.transform.localScale = Vector3.one * (_size * contraction);
            _boundary.color = new Color(tint.r, tint.g, tint.b, (0.90f + 0.10f * pulse) * fade);
            _beacon.transform.localScale = Vector3.one * (_size * contraction);
            _beacon.color = new Color(tint.r, tint.g, tint.b, fade);
            _lightBloom.transform.localScale = Vector3.one * (_size * Mathf.Lerp(0.70f, 1.26f, 1f - Mathf.Pow(1f - arrival, 3f)) * contraction);
            _lightBloom.color = new Color(tint.r, tint.g, tint.b, fade * (_finishing ? 0.55f * (1f - finish) : flash));
            if (!_finishing && _elapsed > 0.52f)
            {
                _lightBloom.transform.localPosition = new Vector3(0, _size * 0.43f, 0);
                _lightBloom.transform.localScale = Vector3.one * (_size * 0.33f);
                _lightBloom.color = new Color(tint.r, tint.g, tint.b, 0.35f + pulse * 0.40f);
            }
            else
            {
                _lightBloom.transform.localPosition = Vector3.zero;
            }
        }
    }
}
