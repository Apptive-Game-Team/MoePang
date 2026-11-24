using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/Unit Effect Library")]
public sealed class UnitEffectLibrary : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        public UnitName unitName;
        public Texture2D sheet;
        [Min(0.04f)]
        public float attackDuration = 0.24f;
        [Min(0.04f)]
        public float hitDuration = 0.24f;
        [Min(0.1f)]
        public float attackWidthMultiplier = 1.6f;
        [Min(0.1f)]
        public float hitWidthMultiplier = 0.9f;
        [Tooltip("Local offset from the existing attack position. Positive X is forward; Y is up.")]
        public Vector3 attackPositionOffset = Vector3.zero;
        [Tooltip("Local offset on the struck unit, using this attacker's hit effect. Positive X follows the attack direction; Y is up.")]
        public Vector3 hitPositionOffset = Vector3.zero;
        [NonSerialized]
        private Sprite[][] frames;
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
                    int bottom = (1 - row) * sheet.height / 2;
                    int top = (2 - row) * sheet.height / 2;
                    for (int col = 0; col < 4; col++)
                    {
                        int left = col * sheet.width / 4;
                        int right = (col + 1) * sheet.width / 4;
                        frames[row][col] = Sprite.Create(sheet, new Rect(left, bottom, right - left, top - bottom), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                        frames[row][col].name = unitName + (row == 0 ? "_Attack_" : "_Hit_") + col;
                    }
                }
            }

            return frames[hit ? 1 : 0];
        }
    }

    [SerializeField]
    private List<Entry> entries = new();
    private static UnitEffectLibrary _loaded;
    private static UnitEffectLibrary _loadedGradeFour;
    private static UnitEffectLibrary _loadedGradeTwo;
    private static UnitEffectLibrary _loadedEnemy;
    public const string EnemyResourcePath = "ScriptableObjects/00.Unit/02.UnitEffect/EnemyUnitEffects";
    public const string EnemyHitReferencePath = "ScriptableObjects/00.Unit/00.FriendlyUnit/1Grade/00.Scorpion";
    private static UnitData _enemyHitReference;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        _loaded = null;
        _loadedGradeFour = null;
        _loadedGradeTwo = null;
        _loadedEnemy = null;
        _enemyHitReference = null;
    }

    public static bool Supports(UnitData data)
    {
        if (!data)
        {
            return false;
        }

        if (data.Team == TeamType.Enemy)
        {
            return FindEnemy(data.UnitName) != null;
        }

        return data.Team == TeamType.Friendly && (data.UnitGrade == 2 || data.UnitGrade == 4 || data.UnitGrade == 5) && Find(data.UnitName) != null;
    }

    private static Entry Find(UnitName name)
    {
        if (!_loaded)
        {
            _loaded = Resources.Load<UnitEffectLibrary>("ScriptableObjects/00.Unit/02.UnitEffect/GradeFiveEffects");
        }

        Entry entry = _loaded ? _loaded.entries.Find(item => item != null && item.unitName == name && item.sheet) : null;
        if (entry != null)
        {
            return entry;
        }

        if (!_loadedGradeFour)
        {
            _loadedGradeFour = Resources.Load<UnitEffectLibrary>("ScriptableObjects/00.Unit/02.UnitEffect/GradeFourEffects");
        }

        entry = _loadedGradeFour ? _loadedGradeFour.entries.Find(item => item != null && item.unitName == name && item.sheet) : null;
        if (entry != null)
        {
            return entry;
        }

        if (!_loadedGradeTwo)
        {
            _loadedGradeTwo = Resources.Load<UnitEffectLibrary>("ScriptableObjects/00.Unit/02.UnitEffect/GradeTwoEffects");
        }

        return _loadedGradeTwo ? _loadedGradeTwo.entries.Find(item => item != null && item.unitName == name && item.sheet) : null;
    }

    private static Entry FindEnemy(UnitName name)
    {
        if (!_loadedEnemy)
        {
            _loadedEnemy = Resources.Load<UnitEffectLibrary>(EnemyResourcePath);
        }

        return _loadedEnemy ? _loadedEnemy.entries.Find(item => item != null && item.unitName == name && item.sheet) : null;
    }

    public static float GetHitWidth(UnitData source, float multiplier)
    {
        float referenceSize = source.UnitSize;
        if (source.Team == TeamType.Enemy)
        {
            if (!_enemyHitReference)
            {
                _enemyHitReference = Resources.Load<UnitData>(EnemyHitReferencePath);
            }

            if (_enemyHitReference)
            {
                referenceSize = _enemyHitReference.UnitSize;
            }
        }

        return Mathf.Max(0.1f, referenceSize) * multiplier;
    }

    public static Vector3 GetHitPosition(Transform receiver, Vector3 restScale, float receiverSize, float direction, Vector3 offset)
    {
        offset.x *= direction < 0f ? -1f : 1f;
        Vector3 parentOffset = receiver.localRotation * Vector3.Scale(restScale, offset);
        Vector3 worldOffset = receiver.parent ? receiver.parent.TransformVector(parentOffset) : parentOffset;
        return receiver.position + Vector3.up * (receiverSize * 0.45f) + worldOffset;
    }

    public static bool PlayHit(UnitData source, float direction, float receiverSize, SpriteRenderer sortingReference, Transform receiver, Vector3 restScale)
    {
        Entry entry = Find(source.UnitName) ?? FindEnemy(source.UnitName);
        Sprite[] frames = entry?.GetFrames(true);
        if (frames == null || frames.Length == 0)
        {
            return false;
        }

        Vector3 position = GetHitPosition(receiver, restScale, receiverSize, direction, entry.hitPositionOffset);
        float width = GetHitWidth(source, entry.hitWidthMultiplier);
        UnitEffectPlayer.Play(frames, position, direction, width, entry.hitDuration, sortingReference, receiver, Vector3.zero, true);
        return true;
    }

    public static void PlayAttack(UnitName source, Vector3 position, float direction, float referenceSize, SpriteRenderer sortingReference, Transform parent)
    {
        Entry entry = Find(source) ?? FindEnemy(source);
        Sprite[] frames = entry?.GetFrames(false);
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        float width = Mathf.Max(0.1f, referenceSize) * entry.attackWidthMultiplier;
        UnitEffectPlayer.Play(frames, position, direction, width, entry.attackDuration, sortingReference, parent, entry.attackPositionOffset);
    }
}
