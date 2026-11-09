using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _01.Scripts._08.Utility;

public static class UIButtonEffectBatch
{
    private static string[] Prefabs() => AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/02.Prefabs", "Assets/Resources" })
        .Select(AssetDatabase.GUIDToAssetPath)
        .OrderBy(p => AssetDatabase.GetDependencies(p, true).Length).ToArray();
    private static string[] Scenes() => AssetDatabase.FindAssets("t:Scene", new[] { "Assets/00.Scenes" })
        .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();

    private static Dictionary<Component, string> Snapshot(GameObject[] roots)
    {
        return roots.SelectMany(r => r.GetComponentsInChildren<Component>(true))
            .Where(c => c != null && !(c is UIButtonEffect))
            .ToDictionary(c => c, c => EditorJsonUtility.ToJson(c));
    }

    private static void VerifyUnchanged(Dictionary<Component, string> before)
    {
        foreach (var entry in before)
            if (entry.Key == null || EditorJsonUtility.ToJson(entry.Key) != entry.Value)
                throw new InvalidOperationException("Existing component changed: " + entry.Key);
    }

    private static int Apply(GameObject[] roots, bool write)
    {
        var buttons = roots.SelectMany(r => r.GetComponentsInChildren<Button>(true)).ToArray();
        var before = write ? Snapshot(roots) : null;
        var count = 0;
        foreach (var button in buttons)
        {
            var existing = button.GetComponents<UIButtonEffect>();
            if (existing.Length > 1) throw new InvalidOperationException("Duplicate effect: " + button.name);
            if (existing.Length != 0) continue;
            count++;
            if (write) button.gameObject.AddComponent<UIButtonEffect>();
        }
        if (write) VerifyUnchanged(before);
        return count;
    }

    [MenuItem("Tools/UI Effects/Apply To Project Buttons")]
    public static void ApplyToProject() => Debug.Log(Run(true));

    [MenuItem("Tools/UI Effects/Check Project Button Coverage")]
    public static void CheckProject() => Debug.Log(Run(false));

    public static string Run(bool write)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play mode first.");
        for (var i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save open scenes first; no changes have been made.");

        var setup = EditorSceneManager.GetSceneManagerSetup();
        var added = 0;
        var changed = new List<string>();
        var scanned = 0;
        try
        {
            foreach (var path in Prefabs())
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    scanned++;
                    var count = Apply(new[] { root }, write);
                    added += count;
                    if (count == 0) continue;
                    changed.Add(path + ": " + count);
                    if (write)
                    {
                        bool success;
                        PrefabUtility.SaveAsPrefabAsset(root, path, out success);
                        if (!success) throw new InvalidOperationException("Could not save " + path);
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var path in Scenes())
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                scanned++;
                var count = Apply(scene.GetRootGameObjects(), write);
                added += count;
                if (count == 0) continue;
                changed.Add(path + ": " + count);
                if (write && !EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save " + path);
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        return (write ? "Added: " : "Missing: ") + added + "; scanned assets: " + scanned +
            "; changed assets: " + changed.Count + "\n" + string.Join("\n", changed);
    }
}
