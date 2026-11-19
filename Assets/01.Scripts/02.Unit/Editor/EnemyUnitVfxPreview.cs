using UnityEditor;

public sealed class EnemyUnitVfxPreview : UnitVfxPreview
{
    protected override bool IsEnemyPreview => true;

    [MenuItem("Tools/MoePang/Enemy Unit VFX Preview")]
    public static void OpenEnemyPreview()
    {
        GetWindow<EnemyUnitVfxPreview>("Enemy Unit VFX Preview").Show();
    }
}
