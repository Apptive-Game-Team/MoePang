#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only regression probe: never runs combat managers or changes saved game data.
public sealed class GradeFourVFXProbe : Unit
{
    public int damageCalls;
    public float totalDamage;
    public void Initialize(UnitData source, TeamType side)
    {
        data = source;
        team = side;
        unitSize = attackRange = source.UnitSize;
        attackDamage = source.AttackDamage;
        maxHp = currentHp = 1000;
        direction = side == TeamType.Friendly ? 1 : -1;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
    public void Attack() => TripleAttack();
    public override void TakeDamage(float damage, Sprite hitSprite = null)
    {
        damageCalls++;
        totalDamage += damage;
        currentHp -= damage;
    }
}

public static class GradeFourEffectValidation
{
    [MenuItem("Tools/MoePang/Validate Grade Four VFX")]
    public static void Run() => Debug.Log(Validate());

    public static string Validate()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run validation outside Play mode.");
        var instance = typeof(UnitTransformQueue).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        object previous = instance.GetValue(null);
        var scene = EditorSceneManager.NewPreviewScene();
        int cases = 0;
        try
        {
            foreach (string guid in AssetDatabase.FindAssets("t:UnitData", new[] {
                         "Assets/Resources/ScriptableObjects/00.Unit/00.FriendlyUnit/4Grade" }))
            {
                var data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
                if (!GradeFiveEffectLibrary.Supports(data)) throw new Exception("Unsupported grade-four unit: " + data.name);
                foreach (int targetCount in new[] { 0, 1, 3, 4 })
                {
                    instance.SetValue(null, null);
                    var queueObject = Create("Queue", scene);
                    var queue = queueObject.AddComponent<UnitTransformQueue>();
                    typeof(UnitTransformQueue).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(queue, null);
                    var attacker = Create("Attacker", scene).AddComponent<GradeFourVFXProbe>();
                    attacker.Initialize(data, TeamType.Friendly);
                    attacker.transform.localScale = new Vector3(0.043277f, 0.045179f, 1);
                    var targets = new GradeFourVFXProbe[targetCount];
                    for (int i = 0; i < targetCount; i++)
                    {
                        targets[i] = Create("Target" + i, scene).AddComponent<GradeFourVFXProbe>();
                        targets[i].Initialize(data, TeamType.Enemy);
                        targets[i].transform.position = new Vector3(0.5f + i * 0.5f, 0, 0);
                        queue.Insert(targets[i]);
                    }
                    attacker.Attack();
                    int expected = Math.Min(3, targetCount);
                    if (attacker.GetComponentsInChildren<GradeFiveEffectPlayer>().Length != (expected > 0 ? 1 : 0))
                        throw new Exception(data.name + ": attack effect must play once, or zero times without a target.");
                    for (int i = 0; i < targetCount; i++)
                    {
                        int hits = i < expected ? 1 : 0;
                        if (targets[i].damageCalls != hits || Mathf.Abs(targets[i].totalDamage - hits * data.AttackDamage) > 0.001f)
                            throw new Exception(data.name + ": damage count/value changed.");
                        if (targets[i].GetComponentsInChildren<GradeFiveEffectPlayer>().Length != hits)
                            throw new Exception(data.name + ": hit effect count/parent mismatch.");
                    }
                    foreach (var root in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);
                    cases++;
                }
            }
            if (cases != 20) throw new Exception("Expected 5 units x 4 target counts, got " + cases);
            return "PASS: 20 grade-four TripleAttack cases (0/1/3/4 targets): unchanged nearest-three damage, one attacker VFX, one child hit VFX per struck unit, no VFX without targets.";
        }
        finally
        {
            instance.SetValue(null, previous);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static GameObject Create(string name, Scene scene)
    {
        var instance = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(instance, scene);
        return instance;
    }
}
#endif
