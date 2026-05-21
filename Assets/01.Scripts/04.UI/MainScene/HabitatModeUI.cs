using System;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene
{
    public class HabitatModeUI : MonoBehaviour
    {
        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    } 
}
