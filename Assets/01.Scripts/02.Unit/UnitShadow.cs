using UnityEngine;

/// <summary>
/// Unit의 발 밑에 들어가는 그림자 스크립트
/// </summary>
public class UnitShadow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private SpriteRenderer shadowRenderer;

    [Header("Shadow Size Settings")]
    [SerializeField] private float widthMultiplier = 0.75f;
    [SerializeField] private float heightMultiplier = 0.22f;
    [SerializeField] private float yOffset = -0.08f;
    
    [SerializeField] private bool lockWorldY = true;
    private float fixedWorldY;

    private void Start()
    {
        if (shadowRenderer != null)
        {
            fixedWorldY = shadowRenderer.transform.position.y;
        }
        
        ApplyShadowSize();
    }
    
    private void LateUpdate()
    {
        if (!lockWorldY || shadowRenderer == null)
            return;

        Transform shadow = shadowRenderer.transform;
        Vector3 position = shadow.position;
        position.y = fixedWorldY;
        shadow.position = position;
    }

    private void ApplyShadowSize()
    {
        if (!targetRenderer || !shadowRenderer || !shadowRenderer.sprite) return;

        Bounds unitBounds = targetRenderer.bounds;

        Transform shadow = shadowRenderer.transform;
        Transform parent = shadow.parent;

        Vector2 shadowSpriteSize = shadowRenderer.sprite.bounds.size;

        Vector3 parentScale = parent ? parent.lossyScale : Vector3.one;

        float desiredWorldWidth = unitBounds.size.x * widthMultiplier;
        float desiredWorldHeight = unitBounds.size.y * heightMultiplier;

        shadow.localScale = new Vector3(
            desiredWorldWidth / (shadowSpriteSize.x * parentScale.x),
            desiredWorldHeight / (shadowSpriteSize.y * parentScale.y),
            1f
        );

        /*shadow.position = new Vector3(
            unitBounds.center.x,
            unitBounds.min.y + yOffset,
            shadow.position.z
        );*/

        shadowRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
    }
}
