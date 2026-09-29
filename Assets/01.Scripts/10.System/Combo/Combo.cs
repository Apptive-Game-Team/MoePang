using _01.Scripts._00.Manager;
using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _01.Scripts._10.System.Combo
{
    [Serializable]
    public class ComboInfo
    {
        public Habitat comboType;
        public Sprite comboImage;
        public int ComboMaxLevel { get; private set; } = 20;
        [TextArea(3,10)] public string comboDescription;
    }
    
    public abstract class Combo : ScriptableObject
    {
        public ComboInfo info;

        public void UpgradeCombo()
        {
            GameManager.Instance.comboData.ComboLevels[info.comboType]++;
        }

        protected string LocalizedDescription(params object[] args)
        {
            string descriptionFormat = LocalizationSettings.StringDatabase.GetLocalizedString(
                "LocalizationDataTable",
                info.comboDescription);

            return string.Format(descriptionFormat, args);
        }
        
        public abstract void TriggerComboEffect(ComboContext context);
        public abstract string DynamicDescription();
    }
}
