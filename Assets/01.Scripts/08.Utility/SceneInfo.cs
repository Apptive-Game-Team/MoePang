using System.Collections.Generic;

namespace _01.Scripts._08.Utility
{
    public enum SceneType
    {
        Title,
        Main,
        MatchAndBattle,
        UnitInfo,
        UnitDescription,
        Combo,
        HabitatModeSelect,
        Shop,
    }
    
    public static class SceneInfo
    {
        private static readonly Dictionary<SceneType, string> SceneNames = new()
        {
            { SceneType.Title , "00.TitleScene"},
            { SceneType.Main, "01.MainScene"},
            { SceneType.MatchAndBattle, "02.3match&Battle"},
            { SceneType.UnitInfo, "03.UnitInfoScene"},
            { SceneType.UnitDescription, "04.UnitDescription"},
            { SceneType.Combo, "05.ComboScene"},
            { SceneType.HabitatModeSelect, "06.HabitatModeSelect"},
            { SceneType.Shop, "07.ShopSceneR"},
        };

        public static string GetSceneName(SceneType type)
        {
            return SceneNames[type];
        }
    }
}
