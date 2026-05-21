using System.Collections.Generic;

namespace _01.Scripts._08.Utility
{
    public enum SceneType
    {
        Title,
        Main,
        Shop,
        MatchAndBattle,
        HabitatBattle,
    }
    
    public static class SceneInfo
    {
        private static readonly Dictionary<SceneType, string> SceneNames = new()
        {
            { SceneType.Title , "00.TitleScene"},
            { SceneType.Main, "01.MainScene"},
            { SceneType.Shop, "02.ShopScene"},
            { SceneType.MatchAndBattle, "03.3match&Battle"},
            { SceneType.HabitatBattle, "04.HabitatBattle"},
        };

        public static string GetSceneName(SceneType type)
        {
            return SceneNames[type];
        }
    }
}
