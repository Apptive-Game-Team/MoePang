using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Grade Five Effect Library")]
public sealed class GradeFiveEffectLibrary : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        public UnitName unitName;
        public Texture2D sheet;
        [Min(0.04f)] public float attackDuration = 0.24f;
        [Min(0.04f)] public float hitDuration = 0.24f;
        [Min(0.1f)] public float attackWidthMultiplier = 1.6f;
        [Min(0.1f)] public float hitWidthMultiplier = 0.9f;
        [NonSerialized] private Sprite[][] frames;

        public Sprite[] GetFrames(bool hit)
        {
            if (!sheet)
            {
                return null;
            }
            
            if (frames == null)
            {
                frames = new Sprite[2][];
                for (int row = 0; row < 2; row++)
                {
                    frames[row] = new Sprite[4];
                    // Odd-sized source sheets: integer boundaries avoid losing the last pixels.
                    int bottom = (1 - row) * sheet.height / 2;
                    int top = (2 - row) * sheet.height / 2;
                    for (int col = 0; col < 4; col++)
                    {
                        int left = col * sheet.width / 4;
                        int right = (col + 1) * sheet.width / 4;
                        frames[row][col] = Sprite.Create(sheet,
                            new Rect(left, bottom, right - left, top - bottom),
                            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                        frames[row][col].name = unitName + (row == 0 ? "_Attack_" : "_Hit_") + col;
                    }
                }
            }
            return frames[hit ? 1 : 0];
        }
    }

    [SerializeField] private List<Entry> entries = new();
    private static GradeFiveEffectLibrary _loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        _loaded = null;
    }

    public static bool Supports(UnitData data)
    {
        return data && data.Team == TeamType.Friendly && data.UnitGrade == 5
            && Find(data.UnitName) != null;
    }

    private static Entry Find(UnitName name)
    {
        if (!_loaded)
        {
            _loaded = Resources.Load<GradeFiveEffectLibrary>(
                "ScriptableObjects/00.Unit/02.UnitEffect/GradeFiveEffects");
        }
        
        if (!_loaded)
        {
            return null;
        }
        
        return _loaded.entries.Find(entry => entry != null && entry.unitName == name && entry.sheet != null);
    }

    public static bool Play(UnitName source, bool hit, Vector3 position, float direction,
        float referenceSize, SpriteRenderer sortingReference)
    {
        Entry entry = Find(source);
        Sprite[] frames = entry?.GetFrames(hit);
        
        if (frames == null || frames.Length == 0)
        {
            return false;
        }
        
        float width = Mathf.Max(0.1f, referenceSize) *
            (hit ? entry.hitWidthMultiplier : entry.attackWidthMultiplier);
        
        GradeFiveEffectPlayer.Play(frames, position, direction, width,
            hit ? entry.hitDuration : entry.attackDuration, sortingReference);
        
        return true;
    }
}
