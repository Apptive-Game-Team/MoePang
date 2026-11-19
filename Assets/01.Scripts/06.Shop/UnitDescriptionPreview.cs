using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    /// <summary>Animator의 현재 스프라이트를 공통 UI의 카메라 텍스처와 분리해 표시합니다.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public sealed class UnitDescriptionPreview : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer source;
        [SerializeField] private Material captureMaterial;
        [SerializeField, Min(1)] private int textureWidth = 1024;
        [SerializeField, Min(1)] private int textureHeight = 768;
        [SerializeField, Min(1f)] private float pixelsPerUnit = 50f;
        private RawImage output;
        private RenderTexture texture;
        private Mesh mesh;
        private CommandBuffer commands;
        private MaterialPropertyBlock properties;
        private Sprite lastSprite;
        private Vector3[] vertices;
        private Color[] colors;
        private TutorialManager tutorial;
        private TutorialHighlightMask highlightMask;

        private void OnEnable()
        {
            output = GetComponent<RawImage>();
            if (captureMaterial == null) captureMaterial = output.material;
            if (source == null || captureMaterial == null) return;
            // 기존 SpriteRenderer의 정렬 순서를 Overlay UI의 형제 순서로 옮깁니다.
            Transform background = transform.parent != null ? transform.parent.Find("BackgroundImage") : null;
            if (background != null && transform.GetSiblingIndex() < background.GetSiblingIndex())
                transform.SetSiblingIndex(background.GetSiblingIndex());
            tutorial = FindFirstObjectByType<TutorialManager>();
            highlightMask = FindFirstObjectByType<TutorialHighlightMask>();
            texture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32)
            {
                name = "UnitDescription Preview",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.Create();
            output.texture = texture;
            output.raycastTarget = false;
            mesh = new Mesh { name = "UnitDescription Sprite", hideFlags = HideFlags.HideAndDontSave };
            commands = new CommandBuffer { name = "UnitDescription Preview" };
            properties = new MaterialPropertyBlock();
            RenderPreview();
        }

        private void LateUpdate()
        {
            RenderPreview();
            if (tutorial != null && tutorial.IsRunning) highlightMask?.RequestRefresh();
        }

        private void RenderPreview()
        {
            if (texture == null || mesh == null || source == null || source.sprite == null) return;
            Rect rect = output.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            Sprite sprite = source.sprite;
            Vector2[] points = sprite.vertices;
            if (lastSprite != sprite)
            {
                mesh.Clear();
                vertices = new Vector3[points.Length];
                colors = new Color[points.Length];
                mesh.vertices = vertices;
                mesh.uv = sprite.uv;
                ushort[] sourceTriangles = sprite.triangles;
                var triangles = new int[sourceTriangles.Length];
                for (int i = 0; i < triangles.Length; i++) triangles[i] = sourceTriangles[i];
                mesh.triangles = triangles;
                lastSprite = sprite;
            }
            for (int i = 0; i < points.Length; i++)
            {
                vertices[i] = new Vector3(source.flipX ? -points[i].x : points[i].x,
                    source.flipY ? -points[i].y : points[i].y, 0f);
                colors[i] = source.color;
            }
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.RecalculateBounds();
            properties.SetTexture("_MainTex", sprite.texture);
            commands.Clear();
            commands.SetRenderTarget(texture);
            commands.ClearRenderTarget(false, true, Color.clear);
            commands.SetViewport(new Rect(0, 0, texture.width, texture.height));
            Matrix4x4 projection = Matrix4x4.Ortho(rect.xMin / pixelsPerUnit, rect.xMax / pixelsPerUnit,
                rect.yMin / pixelsPerUnit, rect.yMax / pixelsPerUnit, -1f, 1f);
            commands.SetViewProjectionMatrices(Matrix4x4.identity, GL.GetGPUProjectionMatrix(projection, false));
            // 카메라와 CameraSortingLayerTexture를 건드리지 않고 현재 프레임만 그립니다.
            commands.DrawMesh(mesh, Matrix4x4.identity, captureMaterial, 0, 1, properties);
            RenderTexture previous = RenderTexture.active;
            try { Graphics.ExecuteCommandBuffer(commands); }
            finally { RenderTexture.active = previous; }
        }

        private void OnDisable()
        {
            if (output != null) output.texture = null;
            commands?.Release();
            commands = null;
            if (texture != null)
            {
                texture.Release();
                DestroyResource(texture);
                texture = null;
            }
            if (mesh != null) DestroyResource(mesh);
            mesh = null;
            lastSprite = null;
        }

        private static void DestroyResource(Object resource)
        {
            if (Application.isPlaying) Destroy(resource);
            else DestroyImmediate(resource);
        }
    }
}
