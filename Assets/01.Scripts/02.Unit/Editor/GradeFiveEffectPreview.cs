using UnityEditor;
using UnityEngine;

public sealed class GradeFiveEffectPreview : EditorWindow
{
    private readonly string[] _names = { "tiger", "whale", "lion", "elephant", "polarbear" };
    private readonly string[] _labels = { "Meadow / Tiger", "Ocean / Whale", "Desert / Lion", "Forest / Elephant", "Polar / Polarbear" };
    private Texture2D[] _sheets;
    private bool _playing = true;
    private bool _flip;
    private float _speed = 1f;
    private double _start;
    private Vector2 _scroll;

    [MenuItem("Tools/MoePang/Grade Five VFX Preview")]
    public static void Open()
    {
        GetWindow<GradeFiveEffectPreview>("Grade Five VFX").Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(620, 480);
        _sheets = new Texture2D[_names.Length];
        
        for (int i = 0; i < _names.Length; i++)
        {
            _sheets[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/MoePang_Art/00.Unit/02.UnitEffect/" + _names[i] + "-attack-hit-sheet.png");
        }
        
        _start = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }

    private void OnDisable() { EditorApplication.update -= Tick; }
    private void Tick() { if (_playing) Repaint(); }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Top-row attack / bottom-row hit. Art preview only; combat damage is unchanged.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        _playing = GUILayout.Toggle(_playing, "Play", "Button", GUILayout.Width(70));
        _flip = GUILayout.Toggle(_flip, "Face left", "Button", GUILayout.Width(90));
        if (GUILayout.Button("Replay", GUILayout.Width(70))) _start = EditorApplication.timeSinceStartup;
        _speed = EditorGUILayout.Slider("Speed", _speed, 0.2f, 2f);
        EditorGUILayout.EndHorizontal();
        // Leave a small pause between bursts so the four-frame sequence is readable.
        float phase = (float)((EditorApplication.timeSinceStartup - _start) * _speed % 0.8);
        int frame = _playing ? Mathf.FloorToInt(phase / 0.06f) : 2;
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (int i = 0; i < _names.Length; i++)
        {
            EditorGUILayout.LabelField(_labels[i], EditorStyles.boldLabel);
            Rect row = GUILayoutUtility.GetRect(560, 160, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(row, new Color(0.23f, 0.28f, 0.33f));
            GUI.Label(new Rect(row.x + 8, row.y + 4, 100, 20), "Attack");
            GUI.Label(new Rect(row.center.x + 8, row.y + 4, 100, 20), "Hit");
            if (_sheets[i] == null) { GUI.Label(row, "Missing VFX sheet"); continue; }
            if (frame < 4)
            {
                DrawCell(_sheets[i], new Rect(row.center.x - 190, row.y + 16, 144, 144), frame, false);
                DrawCell(_sheets[i], new Rect(row.center.x + 85, row.y + 16, 144, 144), frame, true);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawCell(Texture2D sheet, Rect destination, int col, bool hit)
    {
        int left = col * sheet.width / 4;
        int right = (col + 1) * sheet.width / 4;
        int bottom = hit ? 0 : sheet.height / 2;
        int top = hit ? sheet.height / 2 : sheet.height;
        Rect uv = new Rect(left / (float)sheet.width, bottom / (float)sheet.height,
            (right - left) / (float)sheet.width, (top - bottom) / (float)sheet.height);
        if (_flip) { uv.x += uv.width; uv.width = -uv.width; }
        GUI.DrawTextureWithTexCoords(destination, sheet, uv, true);
    }
}
