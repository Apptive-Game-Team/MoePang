using UnityEngine;

public sealed class GradeFiveEffectPlayer : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _frames;
    private float _elapsed;
    private float _duration;

    public static void Play(Sprite[] frames, Vector3 position, float direction, float width,
        float duration, SpriteRenderer sortingReference)
    {
        GameObject instance = new("GradeFiveVFX_" + frames[0].name)
        {
            transform = { position = position }
        };
        
        SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
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
        GradeFiveEffectPlayer player = instance.AddComponent<GradeFiveEffectPlayer>();
        player._spriteRenderer = renderer;
        player._frames = frames;
        player._duration = Mathf.Max(0.04f, duration);
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= _duration)
        {
            Destroy(gameObject);
            return;
        }
        _spriteRenderer.sprite = _frames[Mathf.Min(_frames.Length - 1,
            Mathf.FloorToInt(_elapsed / _duration * _frames.Length))];
        // Fade the final breakup frame instead of leaving a visible hard cut.
        _spriteRenderer.color = new Color(1f, 1f, 1f,
            1f - Mathf.InverseLerp(0.75f, 1f, _elapsed / _duration));
    }
}
