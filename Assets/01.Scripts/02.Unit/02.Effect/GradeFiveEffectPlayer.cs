using UnityEngine;

public sealed class GradeFiveEffectPlayer : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _frames;
    private float _elapsed;
    private float _duration;

    public static void Play(Sprite[] frames, Vector3 position, float direction, float width,
        float duration, SpriteRenderer sortingReference, Transform parent, Vector3 localOffset)
    {
        GameObject instance = new("GradeFiveVFX_" + frames[0].name)
        {
            transform = { position = position }
        };
        
        SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
        ConfigureVisual(renderer, frames, position, direction, width, sortingReference, parent, localOffset);
        GradeFiveEffectPlayer player = instance.AddComponent<GradeFiveEffectPlayer>();
        player._spriteRenderer = renderer;
        player._frames = frames;
        player._duration = Mathf.Max(0.04f, duration);
    }

    // Shared by combat and the editor preview: world-size first, then local offset.
    public static void ConfigureVisual(SpriteRenderer renderer, Sprite[] frames, Vector3 position,
        float direction, float width, SpriteRenderer sortingReference, Transform parent, Vector3 localOffset)
    {
        var instance = renderer.gameObject;
        instance.transform.SetParent(null, true);
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        renderer.sprite = frames[0];
        renderer.flipX = direction < 0f;
        
        if (sortingReference)
        {
            // Match the unit's camera culling layer as well as its sprite sorting layer.
            instance.layer = sortingReference.gameObject.layer;
            renderer.sortingLayerID = sortingReference.sortingLayerID;
            renderer.sortingOrder = sortingReference.sortingOrder + 10;
        }
        
        instance.transform.localScale = Vector3.one * (width / frames[0].bounds.size.x);
        // Preserve the initial world position and size, then follow the owning unit.
        instance.transform.SetParent(parent, true);
        localOffset.x *= direction < 0f ? -1f : 1f;
        instance.transform.localPosition += localOffset;
    }

    public static void SampleVisual(SpriteRenderer renderer, Sprite[] frames, float elapsed, float duration)
    {
        float progress = elapsed / Mathf.Max(0.04f, duration);
        renderer.sprite = frames[Mathf.Clamp(Mathf.FloorToInt(progress * frames.Length), 0, frames.Length - 1)];
        renderer.color = new Color(1f, 1f, 1f, 1f - Mathf.InverseLerp(0.75f, 1f, progress));
    }

    private void OnDisable()
    {
        // Units are pooled: an interrupted effect must not survive their next spawn.
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= _duration)
        {
            Destroy(gameObject);
            return;
        }
        SampleVisual(_spriteRenderer, _frames, _elapsed, _duration);
    }
}
