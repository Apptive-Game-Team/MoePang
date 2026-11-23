using UnityEngine;

public sealed class UnitEffectPlayer : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _frames;
    private float _elapsed;
    private float _duration;
    private bool _keepWorldPose;
    private Vector3 _worldOffset;
    private Vector3 _worldScale;

    public static void Play(Sprite[] frames, Vector3 position, float direction, float width, float duration, SpriteRenderer sortingReference, Transform parent, Vector3 localOffset, bool keepWorldPose = false)
    {
        GameObject instance = new("UnitVFX_" + frames[0].name)
        {
            transform =
            {
                position = position
            }
        };
        SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
        ConfigureVisual(renderer, frames, position, direction, width, sortingReference, parent, localOffset);
        UnitEffectPlayer player = instance.AddComponent<UnitEffectPlayer>();
        player._spriteRenderer = renderer;
        player._frames = frames;
        player._duration = Mathf.Max(0.04f, duration);
        player._keepWorldPose = keepWorldPose;
        if (keepWorldPose && parent)
        {
            player._worldOffset = position - parent.position;
            player._worldScale = Vector3.one * (width / frames[0].bounds.size.x);
        }
    }

    public static void ConfigureVisual(SpriteRenderer renderer, Sprite[] frames, Vector3 position, float direction, float width, SpriteRenderer sortingReference, Transform parent, Vector3 localOffset)
    {
        var instance = renderer.gameObject;
        instance.transform.SetParent(null, true);
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        renderer.sprite = frames[0];
        renderer.flipX = direction < 0f;
        if (sortingReference)
        {
            instance.layer = sortingReference.gameObject.layer;
            renderer.sortingLayerID = sortingReference.sortingLayerID;
            renderer.sortingOrder = sortingReference.sortingOrder + 10;
        }

        instance.transform.localScale = Vector3.one * (width / frames[0].bounds.size.x);
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
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (!_keepWorldPose || !transform.parent)
        {
            return;
        }

        Vector3 parentScale = transform.parent.lossyScale;
        if (Mathf.Approximately(parentScale.x, 0f) || Mathf.Approximately(parentScale.y, 0f) || Mathf.Approximately(parentScale.z, 0f))
        {
            return;
        }

        transform.SetPositionAndRotation(transform.parent.position + _worldOffset, Quaternion.identity);
        transform.localScale = new Vector3(_worldScale.x / parentScale.x, _worldScale.y / parentScale.y, _worldScale.z / parentScale.z);
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
