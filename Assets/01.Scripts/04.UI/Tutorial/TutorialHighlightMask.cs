using System.Collections.Generic;
using _01.Scripts._01.ThreeMatch;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TutorialHighlightMask : MonoBehaviour
{
    [Header("DIM")]
    [SerializeField] private Image dimImage;
    [SerializeField] private Shader maskWriterShader;

    [Header("자식까지 함께 강조할 오브젝트")]
    [SerializeField] private List<GameObject> targets = new();

    [Header("유닛을 그리는 카메라")]
    [Tooltip("BattleCamera를 연결하세요. 비워두면 대상 레이어를 그리는 카메라를 찾습니다.")]
    [SerializeField] private Camera spriteCamera;

    // Preserve and migrate existing Inspector assignments.
    [SerializeField, HideInInspector] private List<Image> imageTargets = new();

    private readonly List<Mesh> meshes = new();
    private readonly List<Vector3> vertices = new();
    private readonly List<Graphic> graphics = new();
    private readonly List<SpriteRenderer> sprites = new();
    private readonly HashSet<Component> drawn = new();
    private MaterialPropertyBlock properties;

    private RenderTexture maskTexture;
    private Material dimMaterial;
    private Material writerMaterial;
    private Material originalDimMaterial;
    private CommandBuffer commands;
    private int meshIndex;
    private PuzzleGenerator puzzleGenerator;
    private readonly List<Vector2Int> puzzleCells = new();

    private static readonly int HighlightMaskId =
        Shader.PropertyToID("_HighlightMask");

    private static readonly int MainTextureId =
        Shader.PropertyToID("_MainTex");

    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
    private static readonly int UseSdfId = Shader.PropertyToID("_UseSDF");
    private static readonly int SdfThresholdId = Shader.PropertyToID("_SDFThreshold");

    private void OnValidate() => MigrateImageTargets();

    private void MigrateImageTargets()
    {
        foreach (Image image in imageTargets)
            if (image != null && !targets.Contains(image.gameObject))
                targets.Add(image.gameObject);
        imageTargets.Clear();
    }

    private void Awake()
    {
        MigrateImageTargets();
        if (dimImage == null || maskWriterShader == null ||
            dimImage.material == null)
        {
            Debug.LogError(
                "TutorialHighlightMask: DIM Image와 Mask Writer Shader를 연결하세요.",
                this
            );

            enabled = false;
            return;
        }

        // Unity 네이티브 객체는 MonoBehaviour 생성자가 아닌 Awake에서 생성합니다.
        properties = new MaterialPropertyBlock();
        originalDimMaterial = dimImage.material;

        // 원본 머티리얼 에셋 대신 실행용 복사본을 사용합니다.
        dimMaterial = new Material(originalDimMaterial);
        writerMaterial = new Material(maskWriterShader);

        dimImage.material = dimMaterial;

        commands = new CommandBuffer
        {
            name = "Tutorial Highlight Mask"
        };
    }

    private void OnEnable() => RequestRefresh();

    // 여러 대상 변경을 한 번의 UI 렌더 직전 갱신으로 모읍니다.
    // Time.timeScale이 0이어도 실행되며, 갱신 후에는 구독을 해제합니다.
    public void RequestRefresh()
    {
        if (!isActiveAndEnabled || dimMaterial == null)
            return;

        Canvas.willRenderCanvases -= RefreshBeforeRender;
        Canvas.willRenderCanvases += RefreshBeforeRender;
    }

    private void RefreshBeforeRender()
    {
        Canvas.willRenderCanvases -= RefreshBeforeRender;
        RefreshMask();
    }

    [ContextMenu("Refresh Highlight Mask")]
    public void RefreshMask()
    {
        if (!isActiveAndEnabled || dimMaterial == null)
            return;

        // 강제 UI 갱신 시 같은 콜백이 다시 실행되지 않도록 먼저 해제합니다.
        Canvas.willRenderCanvases -= RefreshBeforeRender;
        Canvas.ForceUpdateCanvases();

        EnsureMaskTexture();

        commands.Clear();
        commands.SetRenderTarget(maskTexture);
        commands.ClearRenderTarget(false, true, Color.clear);

        meshIndex = 0;
        drawn.Clear();

        foreach (GameObject root in targets)
        {
            if (root == null || !root.activeInHierarchy)
                continue;

            root.GetComponentsInChildren(false, graphics);
            foreach (Graphic target in graphics)
            {
                if (target == dimImage || !target.isActiveAndEnabled ||
                    target.canvasRenderer.cull || !drawn.Add(target))
                    continue;
                DrawGraphic(target);
            }

            root.GetComponentsInChildren(false, sprites);
            foreach (SpriteRenderer renderer in sprites)
            {
                if (!renderer.enabled || renderer.forceRenderingOff ||
                    renderer.sprite == null || !drawn.Add(renderer))
                    continue;
                DrawSprite(renderer);
            }
        }

        foreach (Vector2Int cell in puzzleCells)
            DrawPuzzleCell(cell);

        Graphics.ExecuteCommandBuffer(commands);
    }

    private void DrawPuzzleCell(Vector2Int cell)
    {
        if (puzzleGenerator == null ||
            !puzzleGenerator.TryGetTutorialCellRect(cell, out RectTransform frame, out Rect rect))
            return;

        Rect dimRect = dimImage.rectTransform.rect;
        if (dimRect.width <= 0f || dimRect.height <= 0f)
            return;

        var positions = new[]
        {
            new Vector3(rect.xMin, rect.yMin),
            new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMax, rect.yMax),
            new Vector3(rect.xMax, rect.yMin)
        };
        Camera sourceCamera = GetCanvasCamera(frame.GetComponentInParent<Canvas>());
        Camera dimCamera = GetCanvasCamera(dimImage.canvas);
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(
                sourceCamera, frame.TransformPoint(positions[i]));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dimImage.rectTransform, screen, dimCamera, out Vector2 local);
            positions[i] = new Vector3(
                (local.x - dimRect.xMin) / dimRect.width,
                (local.y - dimRect.yMin) / dimRect.height, 0f);
        }

        Mesh mesh = GetMesh(meshIndex++);
        mesh.Clear();
        mesh.vertices = positions;
        mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        mesh.colors32 = new Color32[] { Color.white, Color.white, Color.white, Color.white };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        QueueMesh(mesh, Texture2D.whiteTexture, 1f, false, 0.5f);
    }

    private void DrawGraphic(Graphic target)
    {
        Mesh sourceMesh = target.canvasRenderer.GetMesh();
        if (sourceMesh == null || sourceMesh.vertexCount == 0)
            return;

        // Copy the UI mesh before converting positions; never modify the original.
        Mesh mesh = GetMesh(meshIndex++);
        mesh.Clear();
        mesh.indexFormat = sourceMesh.indexFormat;
        mesh.vertices = sourceMesh.vertices;
        mesh.uv = sourceMesh.uv;
        mesh.colors32 = sourceMesh.colors32;
        mesh.triangles = sourceMesh.triangles;

        mesh.GetVertices(vertices);

        Camera targetCamera = GetCanvasCamera(target.canvas);
        Camera dimCamera = GetCanvasCamera(dimImage.canvas);
        Rect dimRect = dimImage.rectTransform.rect;

        if (dimRect.width <= 0f || dimRect.height <= 0f)
            return;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 worldPosition =
                target.rectTransform.TransformPoint(vertices[i]);

            Vector2 screenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    worldPosition
                );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dimImage.rectTransform,
                screenPosition,
                dimCamera,
                out Vector2 localPosition
            );

            vertices[i] = new Vector3(
                (localPosition.x - dimRect.xMin) / dimRect.width,
                (localPosition.y - dimRect.yMin) / dimRect.height,
                0f
            );
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();

        Material sourceMaterial = target.materialForRendering;
        bool sdf = (target is TMP_Text || target is TMP_SubMeshUI) &&
                   sourceMaterial != null && sourceMaterial.HasProperty("_GradientScale");
        float threshold = 0.5f;
        if (sdf)
        {
            if (sourceMaterial.HasProperty("_FaceDilate"))
                threshold -= sourceMaterial.GetFloat("_FaceDilate") * 0.5f;
            if (sourceMaterial.HasProperty("_OutlineWidth"))
                threshold -= sourceMaterial.GetFloat("_OutlineWidth") * 0.5f;
        }

        QueueMesh(mesh, target.mainTexture,
            target.canvasRenderer.GetAlpha() * target.canvasRenderer.GetInheritedAlpha(),
            sdf, threshold);
    }

    private void DrawSprite(SpriteRenderer renderer)
    {
        Camera camera = GetSpriteCamera(renderer);
        Rect dimRect = dimImage.rectTransform.rect;
        if (camera == null || dimRect.width <= 0f || dimRect.height <= 0f)
            return;

        Sprite sprite = renderer.sprite;
        Vector2[] sourceVertices = sprite.vertices;
        var positions = new Vector3[sourceVertices.Length];
        var colors = new Color32[sourceVertices.Length];
        Camera dimCamera = GetCanvasCamera(dimImage.canvas);
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            Vector2 point = sourceVertices[i];
            Vector3 local = new Vector3(renderer.flipX ? -point.x : point.x,
                renderer.flipY ? -point.y : point.y, 0f);
            Vector3 screen = camera.WorldToScreenPoint(renderer.transform.TransformPoint(local));
            // Units behind the camera must not cut holes in the DIM.
            if (screen.z <= 0f)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dimImage.rectTransform, screen, dimCamera, out Vector2 dimLocal);
            positions[i] = new Vector3(
                (dimLocal.x - dimRect.xMin) / dimRect.width,
                (dimLocal.y - dimRect.yMin) / dimRect.height, 0f);
            colors[i] = renderer.color;
        }

        ushort[] sourceTriangles = sprite.triangles;
        var triangles = new int[sourceTriangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = sourceTriangles[i];

        Mesh mesh = GetMesh(meshIndex++);
        mesh.Clear();
        mesh.vertices = positions;
        mesh.uv = sprite.uv;
        mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        QueueMesh(mesh, sprite.texture, 1f, false, 0.5f);
    }

    private void QueueMesh(Mesh mesh, Texture texture, float opacity, bool sdf, float threshold)
    {
        properties.Clear();
        properties.SetTexture(MainTextureId, texture != null ? texture : Texture2D.whiteTexture);
        properties.SetFloat(OpacityId, opacity);
        properties.SetFloat(UseSdfId, sdf ? 1f : 0f);
        properties.SetFloat(SdfThresholdId, threshold);
        commands.DrawMesh(mesh, Matrix4x4.identity, writerMaterial, 0, 0, properties);
    }

    private Camera GetSpriteCamera(SpriteRenderer renderer)
    {
        if (spriteCamera != null)
            return spriteCamera;

        Camera result = null;
        int layerMask = 1 << renderer.gameObject.layer;
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.targetTexture == null && (camera.cullingMask & layerMask) != 0 &&
                (result == null || camera.depth > result.depth))
                result = camera;
        }
        return result;
    }

    private void EnsureMaskTexture()
    {
        int width = Mathf.Max(1, Screen.width);
        int height = Mathf.Max(1, Screen.height);

        if (maskTexture != null &&
            maskTexture.width == width &&
            maskTexture.height == height)
        {
            return;
        }

        if (maskTexture != null)
        {
            maskTexture.Release();
            Destroy(maskTexture);
        }

        maskTexture = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        )
        {
            name = "Tutorial Highlight Mask",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = 1
        };

        maskTexture.Create();

        dimMaterial.SetTexture(HighlightMaskId, maskTexture);
    }

    private Mesh GetMesh(int index)
    {
        while (meshes.Count <= index)
        {
            var mesh = new Mesh
            {
                name = "Tutorial Highlight Image"
            };

            mesh.MarkDynamic();
            meshes.Add(mesh);
        }

        return meshes[index];
    }

    private static Camera GetCanvasCamera(Canvas canvas)
    {
        if (canvas == null)
            return null;

        Canvas root = canvas.rootCanvas;

        return root.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : root.worldCamera;
    }

    public void AddTarget(GameObject target)
    {
        if (target != null && !targets.Contains(target))
        {
            targets.Add(target);
            RequestRefresh();
        }
    }

    // UI 오브젝트와 임의 개수의 퍼즐 칸을 한 번에 지정하고 즉시 표시합니다.
    public void ShowHighlights(
        IEnumerable<GameObject> objects, PuzzleGenerator generator, IEnumerable<Vector2Int> cells)
    {
        if (!isActiveAndEnabled || dimMaterial == null)
            return;

        targets.Clear();
        if (objects != null)
            foreach (GameObject target in objects)
                if (target != null && !targets.Contains(target))
                    targets.Add(target);

        puzzleGenerator = generator;
        puzzleCells.Clear();
        if (cells != null)
            foreach (Vector2Int cell in cells)
                if (!puzzleCells.Contains(cell))
                    puzzleCells.Add(cell);

        // 같은 프레임에 마스크까지 완성하므로 구멍 없는 DIM이 먼저 보이지 않습니다.
        dimImage.gameObject.SetActive(true);
        RefreshMask();
    }

    public void HideHighlights()
    {
        Canvas.willRenderCanvases -= RefreshBeforeRender;
        targets.Clear();
        puzzleCells.Clear();
        puzzleGenerator = null;
        if (dimImage != null)
            dimImage.gameObject.SetActive(false);
    }

    public void RemoveTarget(GameObject target)
    {
        if (targets.Remove(target))
            RequestRefresh();
    }

    public void ClearTargets()
    {
        targets.Clear();
        puzzleCells.Clear();
        puzzleGenerator = null;
        RequestRefresh();
    }

    // Keep existing callers working with hierarchy targets.
    public void AddImage(Image target)
    {
        if (target != null)
            AddTarget(target.gameObject);
    }

    public void RemoveImage(Image target)
    {
        if (target != null)
            RemoveTarget(target.gameObject);
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= RefreshBeforeRender;

        if (maskTexture == null || commands == null)
            return;

        commands.Clear();
        commands.SetRenderTarget(maskTexture);
        commands.ClearRenderTarget(false, true, Color.clear);

        Graphics.ExecuteCommandBuffer(commands);
    }

    private void OnDestroy()
    {
        Canvas.willRenderCanvases -= RefreshBeforeRender;

        if (dimImage != null && dimImage.material == dimMaterial)
            dimImage.material = originalDimMaterial;

        commands?.Release();

        foreach (Mesh mesh in meshes)
            Destroy(mesh);

        if (maskTexture != null)
        {
            maskTexture.Release();
            Destroy(maskTexture);
        }

        if (dimMaterial != null)
            Destroy(dimMaterial);

        if (writerMaterial != null)
            Destroy(writerMaterial);
    }
}
