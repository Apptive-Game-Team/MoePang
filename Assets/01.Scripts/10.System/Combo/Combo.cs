using _01.Scripts._00.Manager;
using System;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [Serializable]
    public class ComboInfo
    {
        public Habitat comboType;
        public Sprite comboImage;
        public int ComboMaxLevel { get; private set; } = 5;
        [TextArea(3,10)] public string comboDescription;
    }
    
    public abstract class Combo : ScriptableObject
    {
        public ComboInfo info;

        public void UpgradeCombo()
        {
            GameManager.Instance.comboData.comboLevels[info.comboType]++;
        }
        
        public abstract void TriggerComboEffect(ComboContext context);
        public abstract string DynamicDescription();
    }
}
