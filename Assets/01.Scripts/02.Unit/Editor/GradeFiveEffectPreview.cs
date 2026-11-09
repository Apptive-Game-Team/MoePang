using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Visual-only objects live in PreviewRenderUtility's scene. No Unit logic is run.
public sealed class GradeFiveEffectPreview : EditorWindow
{
    [SerializeField] private int _grade = 5;
    private string LibraryPath => "Assets/Resources/ScriptableObjects/00.Unit/02.UnitEffect/" +
        (_grade == 2 ? "GradeTwoEffects.asset" : _grade == 4 ? "GradeFourEffects.asset" : "GradeFiveEffects.asset");
    private const string PrefabPath = "Assets/02.Prefabs/02.Unit/FriendlyUnitPrefab.prefab";
    private const string BattleScenePath = "Assets/00.Scenes/01.MainScene/02.3match&Battle.unity";
    private struct ParentPose { public string name; public Vector3 scale; public Quaternion rotation; }
    private readonly List<ParentPose> _parentPoses = new List<ParentPose>();
    private Transform _previewPool;
    private GameObject _unitPrefab, _enemyPrefab;
    private string _scaleSource;
    private Matrix4x4 _contextMatrix;
    private int _contextScene;
    private double _nextContextCheck;
    private GradeFiveEffectLibrary _library;
    private SerializedObject _settings;
    private UnitData[] _units;
    private string[] _labels;
    private int _selected;
    private bool _hit, _flip, _playing = true, _guides = true;
    private float _time, _speed = 1f, _zoom = 1f;
    private double _lastTick;
    private Vector2 _scroll;
    private UnitData _target;
    private PreviewRenderUtility _preview;
    private Texture _lastPreviewTexture;
    private GameObject _attacker, _receiver, _effect;
    private SpriteRenderer _attackerSprite, _receiverSprite, _effectSprite;
    private Vector3 _unitScale = Vector3.one;
    private GradeFiveEffectLibrary.Entry _framesEntry;
    private Texture2D _cachedSheet;
    private AnimationClip _attack, _idle, _targetIdle, _targetDamage;
    private UnitData _cachedUnit, _cachedTarget;

    [MenuItem("Tools/MoePang/Grade Five VFX Preview")]
    [MenuItem("Tools/MoePang/Unit VFX Preview")]
    public static void Open() => GetWindow<GradeFiveEffectPreview>("Unit VFX Preview").Show();

    private void OnEnable()
    {
        minSize = new Vector2(640, 660);
        LoadAssets();
        _lastTick = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        Undo.undoRedoPerformed += Repaint;
        EditorApplication.playModeStateChanged += PlayModeChanged;
    }

    private void LoadAssets()
    {
        _library = AssetDatabase.LoadAssetAtPath<GradeFiveEffectLibrary>(LibraryPath);
        _settings = _library ? new SerializedObject(_library) : null;
        var units = new List<UnitData>();
        foreach (string guid in AssetDatabase.FindAssets("t:UnitData",
                     new[] { "Assets/Resources/ScriptableObjects/00.Unit/00.FriendlyUnit/" + _grade + "Grade" }))
        {
            var unit = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (unit && unit.Team == TeamType.Friendly && unit.UnitGrade == _grade) units.Add(unit);
        }
        units.Sort((a, b) => ((int)a.UnitName).CompareTo((int)b.UnitName));
        _units = units.ToArray();
        _labels = Array.ConvertAll(_units, unit => unit.UnitName.ToString());
        _selected = Mathf.Clamp(_selected, 0, Mathf.Max(0, _units.Length - 1));
        LoadBattleContext();
        _cachedUnit = _cachedTarget = null;
        ReleaseFrames();
    }

    private void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
        {
            LoadBattleContext();
            Repaint();
        }
    }

    private void LoadBattleContext()
    {
        _parentPoses.Clear();
        _unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        _enemyPrefab = null;
        UnitPool pool = null;
        foreach (var candidate in Resources.FindObjectsOfTypeAll<UnitPool>())
            if (candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded
                && !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(candidate.gameObject.scene))
            { pool = candidate; break; }
        UnityEngine.SceneManagement.Scene temporary = default;
        try
        {
            if (!pool)
            {
                temporary = UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene(BattleScenePath);
                foreach (var root in temporary.GetRootGameObjects())
                {
                    pool = root.GetComponentInChildren<UnitPool>(true);
                    if (pool) break;
                }
            }
            if (!pool) throw new InvalidOperationException("Battle scene has no UnitPool.");
            _contextMatrix = pool.transform.localToWorldMatrix;
            _contextMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
            _contextScene = temporary.IsValid() ? 0 : pool.gameObject.scene.handle;
            _scaleSource = (temporary.IsValid() ? "전투 씬 에셋" : "열린 전투 씬") + " / " + pool.transform.parent.name;
            var chain = new List<Transform>();
            for (var parent = pool.transform; parent; parent = parent.parent) chain.Add(parent);
            chain.Reverse();
            foreach (var parent in chain) _parentPoses.Add(new ParentPose
                { name = parent.name, scale = parent.localScale, rotation = parent.localRotation });
            foreach (var root in pool.gameObject.scene.GetRootGameObjects())
            {
                var spawner = root.GetComponentInChildren<_01.Scripts._02.Unit.UnitSpawner>(true);
                if (!spawner) continue;
                var serialized = new SerializedObject(spawner);
                var friendly = serialized.FindProperty("friendlyPrefab").objectReferenceValue as Unit;
                var enemy = serialized.FindProperty("enemyPrefab").objectReferenceValue as Unit;
                if (friendly) _unitPrefab = friendly.gameObject;
                if (enemy) _enemyPrefab = enemy.gameObject;
                break;
            }
            if (_unitPrefab) _unitScale = _unitPrefab.transform.localScale;
        }
        catch (Exception exception)
        {
            _scaleSource = "전투 계층을 읽지 못했습니다: " + exception.Message;
            Debug.LogException(exception);
        }
        finally
        {
            if (temporary.IsValid()) UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(temporary);
        }
        _preview?.Cleanup();
        _preview = null;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Tick;
        Undo.undoRedoPerformed -= Repaint;
        EditorApplication.playModeStateChanged -= PlayModeChanged;
        _preview?.Cleanup();
        _preview = null;
        ReleaseFrames();
    }

    private void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now >= _nextContextCheck)
        {
            _nextContextCheck = now + 0.5;
            foreach (var pool in Resources.FindObjectsOfTypeAll<UnitPool>())
            {
                if (!pool.gameObject.scene.IsValid() || !pool.gameObject.scene.isLoaded
                    || UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(pool.gameObject.scene)) continue;
                var matrix = pool.transform.localToWorldMatrix;
                matrix.SetColumn(3, new Vector4(0, 0, 0, 1));
                if (matrix != _contextMatrix || pool.gameObject.scene.handle != _contextScene)
                { LoadBattleContext(); Repaint(); }
                break;
            }
        }
        if (_playing && _units != null && _units.Length > 0)
        {
            _time = (_time + (float)(now - _lastTick) * _speed) % CycleLength;
            Repaint();
        }
        _lastTick = now;
    }

    private float AttackSpeed => Mathf.Max(0.01f, _units[_selected].AttackSpeed);
    private float TriggerTime => 1f / AttackSpeed / 3f;
    private float CycleLength => Mathf.Max(1f / AttackSpeed + 0.2f,
        TriggerTime + (_hit ? ReadFloat("hitDuration", 0.24f) : ReadFloat("attackDuration", 0.24f)) + 0.2f);

    private SerializedProperty CurrentEntry()
    {
        if (_settings == null || _units == null || _units.Length == 0) return null;
        _settings.UpdateIfRequiredOrScript();
        var entries = _settings.FindProperty("entries");
        for (int i = 0; i < entries.arraySize; i++)
        {
            var entry = entries.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("unitName").intValue == (int)_units[_selected].UnitName) return entry;
        }
        return null;
    }

    private float ReadFloat(string name, float fallback) => CurrentEntry()?.FindPropertyRelative(name).floatValue ?? fallback;

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        int gradeIndex = EditorGUILayout.Popup("등급 / Grade", _grade == 2 ? 0 : _grade == 4 ? 1 : 2,
            new[] { "2등급", "4등급", "5등급" });
        if (EditorGUI.EndChangeCheck())
        {
            _grade = gradeIndex == 0 ? 2 : gradeIndex == 1 ? 4 : 5;
            _selected = 0;
            _time = 0;
            LoadAssets();
        }
        if (!_library || _units == null || _units.Length == 0)
        {
            EditorGUILayout.HelpBox(_grade + "등급 이펙트 설정 또는 UnitData가 없습니다.", MessageType.Warning);
            if (GUILayout.Button("Reload assets")) LoadAssets();
            return;
        }
        EditorGUILayout.HelpBox("플레이 없이 실제 공격 클립과 이펙트 위치를 확인합니다. 아래 설정은 " + _library.name + "에 직접 반영됩니다 (Undo 가능).", MessageType.Info);
        if (_grade == 4)
            EditorGUILayout.LabelField("4등급: 공격 이펙트 1회 + 선택된 최대 3개 유닛에 피격 이펙트", EditorStyles.miniLabel);
        if (_grade == 2)
            EditorGUILayout.LabelField("2등급: 사거리 내 단일 근접 대상. 3등급은 기존 투사체 표현을 유지합니다.", EditorStyles.miniLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("계층 기준: " + _scaleSource, EditorStyles.miniLabel);
            if (GUILayout.Button("전투 계층 다시 읽기", GUILayout.Width(140))) LoadBattleContext();
        }
        EditorGUI.BeginChangeCheck();
        _selected = EditorGUILayout.Popup("Unit / 이펙트 종류", _selected, _labels);
        _hit = GUILayout.Toolbar(_hit ? 1 : 0, new[] { "공격 + Attack VFX", "공격 + 대상 Hit VFX" }) == 1;
        if (EditorGUI.EndChangeCheck()) { _time = 0f; _cachedUnit = null; }
        if (_hit)
        {
            _target = (UnitData)EditorGUILayout.ObjectField("피격 대상 (비우면 같은 유닛)", _target, typeof(UnitData), false);
            EditorGUILayout.LabelField("Hit 설정은 공격자의 항목, 위치 기준은 피격 대상입니다.", EditorStyles.miniLabel);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            _playing = GUILayout.Toggle(_playing, "Play", "Button", GUILayout.Width(65));
            if (GUILayout.Button("Replay", GUILayout.Width(65))) _time = 0;
            if (GUILayout.Button("이펙트 정점", GUILayout.Width(100)))
            {
                _playing = false;
                _time = TriggerTime + ReadFloat(_hit ? "hitDuration" : "attackDuration", 0.24f) * 0.55f;
            }
            _flip = GUILayout.Toggle(_flip, "Face left", "Button", GUILayout.Width(85));
            _guides = GUILayout.Toggle(_guides, "Guides", "Button", GUILayout.Width(65));
        }
        EditorGUI.BeginChangeCheck();
        _time = EditorGUILayout.Slider("Time (s)", _time, 0, CycleLength);
        if (EditorGUI.EndChangeCheck()) _playing = false;
        using (new EditorGUILayout.HorizontalScope())
        {
            _speed = EditorGUILayout.Slider("Speed", _speed, 0.1f, 2f);
            _zoom = EditorGUILayout.Slider("Zoom", _zoom, 0.3f, 3f);
        }
        Rect view = GUILayoutUtility.GetRect(100, Mathf.Max(220, position.height * 0.4f), GUILayout.ExpandWidth(true));
        if (Event.current.type == EventType.Repaint) DrawPreview(view);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        var entry = CurrentEntry();
        if (entry != null)
        {
            EditorGUILayout.LabelField(_library.name + " / " + _labels[_selected], EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Offset: 기존 위치에 더하는 로컬 좌표. +X = 공격 방향, +Y = 위", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            foreach (string field in new[] { "attackPositionOffset", "hitPositionOffset", "attackDuration", "hitDuration", "attackWidthMultiplier", "hitWidthMultiplier" })
                EditorGUILayout.PropertyField(entry.FindPropertyRelative(field));
            if (EditorGUI.EndChangeCheck()) { _settings.ApplyModifiedProperties(); Repaint(); }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("설정 에셋 선택")) { Selection.activeObject = _library; EditorGUIUtility.PingObject(_library); }
                if (GUILayout.Button("설정 저장")) AssetDatabase.SaveAssetIfDirty(_library);
            }
        }
        else EditorGUILayout.HelpBox("This unit has no entry in " + _library.name + ".", MessageType.Warning);
        EditorGUILayout.EndScrollView();
    }

    private void EnsurePreview()
    {
        if (_preview != null) return;
        _preview = new PreviewRenderUtility();
        _preview.camera.orthographic = true;
        _preview.camera.nearClipPlane = 0.01f;
        _preview.camera.farClipPlane = 100f;
        _preview.camera.clearFlags = CameraClearFlags.SolidColor;
        _preview.camera.backgroundColor = new Color(0.16f, 0.20f, 0.25f);
        _attacker = CreateVisual("Attacker", out _attackerSprite);
        _receiver = CreateVisual("Receiver", out _receiverSprite);
        _effect = CreateVisual("Effect", out _effectSprite);
        _effectSprite.sortingOrder = 10;
        _previewPool = null;
        foreach (var pose in _parentPoses)
        {
            var go = new GameObject("Preview_" + pose.name) { hideFlags = HideFlags.HideAndDontSave };
            _preview.AddSingleGO(go);
            go.transform.SetParent(_previewPool, false);
            go.transform.localScale = pose.scale;
            go.transform.localRotation = pose.rotation;
            _previewPool = go.transform;
        }
        _attacker.transform.SetParent(_previewPool, false);
        _receiver.transform.SetParent(_previewPool, false);
        ApplyPrefabVisual(_unitPrefab, _attackerSprite);
    }

    private static void ApplyPrefabVisual(GameObject prefab, SpriteRenderer renderer)
    {
        var source = prefab ? prefab.GetComponent<SpriteRenderer>() : null;
        if (!source) return;
        renderer.sharedMaterials = source.sharedMaterials;
        renderer.color = source.color;
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder;
    }

    private GameObject CreateVisual(string name, out SpriteRenderer renderer)
    {
        var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        renderer = go.AddComponent<SpriteRenderer>();
        if (name != "Effect")
        {
            // Non-legacy clips need an Animator to bind their sprite curves.
            var animator = go.AddComponent<Animator>();
            animator.fireEvents = false;
        }
        // Match the runtime VFX's unlit default SpriteRenderer material.
        _preview.AddSingleGO(go);
        return go;
    }

    private static AnimationClip FindClip(UnitData unit, string name)
    {
        if (!unit || !unit.AnimatorOverride) return null;
        var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        unit.AnimatorOverride.GetOverrides(clips);
        foreach (var pair in clips)
            if (pair.Key.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) return pair.Value ? pair.Value : pair.Key;
        return null;
    }

    private void DrawPreview(Rect rect)
    {
        EnsurePreview();
        var unit = _units[_selected];
        var target = _target ? _target : unit;
        if (_cachedUnit != unit)
        {
            _attack = FindClip(unit, "Attack");
            _idle = FindClip(unit, "Idle");
            _cachedUnit = unit;
        }
        if (_cachedTarget != target)
        {
            _targetIdle = FindClip(target, "Idle");
            _targetDamage = FindClip(target, "Damage");
            _cachedTarget = target;
        }
        float direction = _flip ? -1f : 1f;
        // Same base position and reference sizes as Unit.RangeAttack/TakeGradeFiveAreaDamage.
        _attacker.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _attacker.transform.localScale = _unitScale;
        var clip = _time < 2f * TriggerTime ? _attack : _idle;
        if (clip) clip.SampleAnimation(_attacker, Mathf.Min(_time * AttackSpeed, Mathf.Max(0, clip.length - 0.0001f)));
        _attackerSprite.flipX = _flip;
        _receiver.SetActive(_hit);
        _receiver.transform.position = Vector3.right * direction * unit.UnitSize;
        var targetPrefab = target.Team == TeamType.Enemy && _enemyPrefab ? _enemyPrefab : _unitPrefab;
        _receiver.transform.localScale = targetPrefab ? targetPrefab.transform.localScale : _unitScale;
        ApplyPrefabVisual(targetPrefab, _receiverSprite);
        var targetClip = _time >= TriggerTime && _targetDamage ? _targetDamage : _targetIdle;
        if (targetClip) targetClip.SampleAnimation(_receiver, Mathf.Repeat(Mathf.Max(0, _time - TriggerTime), Mathf.Max(0.01f, targetClip.length)));
        _receiverSprite.flipX = !_flip;

        var entry = CurrentEntry();
        var sheet = entry?.FindPropertyRelative("sheet").objectReferenceValue as Texture2D;
        if (_cachedSheet != sheet || _framesEntry == null)
        {
            ReleaseFrames();
            _cachedSheet = sheet;
            _framesEntry = new GradeFiveEffectLibrary.Entry { sheet = sheet, unitName = unit.UnitName };
        }
        var frames = _framesEntry.GetFrames(_hit);
        float duration = Mathf.Max(0.04f, ReadFloat(_hit ? "hitDuration" : "attackDuration", 0.24f));
        float effectTime = _time - TriggerTime;
        bool visible = frames != null && effectTime >= 0 && effectTime < duration;
        _effect.SetActive(visible);
        Vector3 origin = _hit ? _receiver.transform.position : Vector3.zero;
        Vector3 spawn = origin + new Vector3(_hit ? 0 : direction * unit.UnitSize * 0.5f,
            (_hit ? target.UnitSize : unit.UnitSize) * 0.45f, 0);
        var parent = _hit ? _receiver.transform : _attacker.transform;
        var offset = entry?.FindPropertyRelative(_hit ? "hitPositionOffset" : "attackPositionOffset").vector3Value ?? Vector3.zero;
        if (frames != null)
        {
            float width = Mathf.Max(0.1f, _hit ? target.UnitSize : unit.UnitSize) * ReadFloat(_hit ? "hitWidthMultiplier" : "attackWidthMultiplier", 1f);
            GradeFiveEffectPlayer.ConfigureVisual(_effectSprite, frames, spawn, direction, width,
                _hit ? _receiverSprite : _attackerSprite, parent, offset);
            GradeFiveEffectPlayer.SampleVisual(_effectSprite, frames, effectTime, duration);
        }
        _preview.camera.transform.SetPositionAndRotation(new Vector3(direction * unit.UnitSize * 0.4f, unit.UnitSize * 0.4f, -20), Quaternion.identity);
        _preview.camera.orthographicSize = Mathf.Max(_grade == 2 ? 0.22f : 0.6f, unit.UnitSize * 0.7f) / _zoom;
        _preview.BeginPreview(rect, GUIStyle.none);
        _preview.Render(true);
        _lastPreviewTexture = _preview.EndPreview();
        GUI.DrawTexture(rect, _lastPreviewTexture, ScaleMode.StretchToFill, false);
        if (_guides)
        {
            DrawMarker(rect, Vector3.zero, Color.cyan, "Unit origin");
            DrawMarker(rect, spawn, Color.yellow, "Base spawn");
            DrawMarker(rect, _effect.transform.position, Color.green, "VFX + offset");
        }
        GUI.Label(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, 24),
            "Attack trigger: " + TriggerTime.ToString("0.000") + "s  |  " + (visible ? "VFX playing" : "VFX off"), EditorStyles.whiteMiniLabel);
        GUI.Label(new Rect(rect.x + 8, rect.y + 24, rect.width - 16, 20),
            "Unit world scale: " + _attacker.transform.lossyScale.ToString("F4"), EditorStyles.whiteMiniLabel);
        if (!_attack) GUI.Label(rect, "Attack animation missing", EditorStyles.whiteLabel);
    }

    private void DrawMarker(Rect rect, Vector3 position, Color color, string label)
    {
        Vector3 point = _preview.camera.WorldToViewportPoint(position);
        Vector2 gui = new Vector2(rect.x + point.x * rect.width, rect.yMax - point.y * rect.height);
        if (!rect.Contains(gui)) return;
        EditorGUI.DrawRect(new Rect(gui.x - 5, gui.y, 11, 1), color);
        EditorGUI.DrawRect(new Rect(gui.x, gui.y - 5, 1, 11), color);
        GUI.Label(new Rect(gui.x + 7, gui.y, 110, 20), label, EditorStyles.whiteMiniLabel);
    }

    private void ReleaseFrames()
    {
        if (_framesEntry?.sheet)
            foreach (bool hit in new[] { false, true })
                foreach (Sprite sprite in _framesEntry.GetFrames(hit)) if (sprite) DestroyImmediate(sprite);
        _framesEntry = null;
        _cachedSheet = null;
    }
}
