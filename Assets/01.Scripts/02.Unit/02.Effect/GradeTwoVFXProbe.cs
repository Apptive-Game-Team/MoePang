#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GradeTwoVFXProbe : Unit
{
    public int damageCalls;
    public float totalDamage;
    public void Initialize(UnitData source, TeamType side)
    {
        data = source; team = side; unitSize = attackRange = source.UnitSize;
        attackDamage = source.AttackDamage; maxHp = currentHp = 1000;
        direction = side == TeamType.Friendly ? 1 : -1;
        targetLayer = 0;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
    public void Attack() => MeleeAttack();
    public override void TakeDamage(float damage, Sprite hitSprite = null)
    { damageCalls++; totalDamage += damage; currentHp -= damage; }
}

public static class GradeTwoEffectValidation
{
    [MenuItem("Tools/MoePang/Validate Grade Two VFX")]
    public static void Run() => Debug.Log(Validate());
    public static string Validate()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run outside Play mode.");
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var singleton = typeof(UnitTransformQueue).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        object previous = singleton.GetValue(null);
        var scene = EditorSceneManager.NewPreviewScene();
        int cases = 0;
        try
        {
            foreach (var guid in AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Resources/ScriptableObjects/00.Unit/00.FriendlyUnit/2Grade" }))
            {
                var data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
                if (!GradeFiveEffectLibrary.Supports(data)) throw new Exception("Unsupported: " + data.name);
                foreach (int mode in new[] { 0, 1, 2, 3 })
                {
                    singleton.SetValue(null, null);
                    var queue = Create("Queue", scene).AddComponent<UnitTransformQueue>();
                    typeof(UnitTransformQueue).GetMethod("Awake", flags).Invoke(queue, null);
                    var attacker = Create("Attacker", scene).AddComponent<GradeTwoVFXProbe>();
                    attacker.Initialize(data, TeamType.Friendly);
                    var target = Create("Target", scene).AddComponent<GradeTwoVFXProbe>();
                    target.Initialize(data, TeamType.Enemy);
                    target.transform.position = Vector3.right * (mode == 2 ? 10 : mode == 3 ? -0.2f : 0.2f);
                    if (mode != 0) queue.Insert(target);
                    attacker.Attack();
                    int expected = mode == 1 ? 1 : 0;
                    if (target.damageCalls != expected || Mathf.Abs(target.totalDamage - expected * data.AttackDamage) > 0.001f)
                        throw new Exception("Melee damage changed: " + data.name);
                    if (attacker.GetComponentsInChildren<GradeFiveEffectPlayer>().Length != expected ||
                        target.GetComponentsInChildren<GradeFiveEffectPlayer>().Length != expected)
                        throw new Exception("Melee effect parenting/count: " + data.name);
                    foreach (var root in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);
                    cases++;
                }
            }
            if (cases != 20) throw new Exception("Expected 20 cases, got " + cases);
            return "PASS: 20 grade-two melee cases: 5 units x no target / in range / out of range / behind. Damage unchanged; exactly one child attack and one child hit VFX only for a valid target.";
        }
        finally { singleton.SetValue(null, previous); EditorSceneManager.ClosePreviewScene(scene); }
    }
    private static GameObject Create(string name, Scene scene)
    {
        var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(go, scene); return go;
    }
}
#endif
