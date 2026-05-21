using UnityEngine;

namespace _01.Scripts._11.HabitatMode
{
    public class HabitatModeManager : SingletonObject<HabitatModeManager>
    {
        [SerializeField] private HabitatMode habitatMode = HabitatMode.MeadowMode;

        public HabitatMode HabitatMode
        {
            get => habitatMode;
            set => habitatMode = value;
        }
    }
}

