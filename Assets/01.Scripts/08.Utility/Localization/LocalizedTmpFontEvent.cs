using System;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Base
{
#if UNITY_EDITOR
    public static class TMProLocalizeExtension
    {
        private const string StringTableName = "LocalizationDataTable";

        [MenuItem("CONTEXT/TextMeshProUGUI/Localize Extension")]
        private static void LocalizeTMProTextWithFontAssets(MenuCommand command)
        {
            var target = command.context as TextMeshProUGUI;
            if (target == null)
            {
                return;
            }

            SetupForLocalizeString(target);
            SetupForLocalizeTmpFont(target);
        }

        private static void SetupForLocalizeString(TextMeshProUGUI target)
        {
            var comp = Undo.AddComponent<LocalizeStringEvent>(target.gameObject);
            comp.SetTable(StringTableName);

            var setStringMethod = target.GetType().GetProperty(nameof(TextMeshProUGUI.text))?.GetSetMethod();
            if (setStringMethod == null)
            {
                return;
            }

            var methodDelegate =
                Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
            UnityEventTools.AddPersistentListener(comp.OnUpdateString, methodDelegate);
            comp.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
        }

        private static void SetupForLocalizeTmpFont(TextMeshProUGUI target)
        {
            var comp = Undo.AddComponent<LocalizedTmpFontEvent>(target.gameObject);

            var setFontMethod = target.GetType().GetProperty(nameof(TextMeshProUGUI.font))?.GetSetMethod();
            if (setFontMethod == null)
            {
                return;
            }

            var methodDelegate =
                Delegate.CreateDelegate(typeof(UnityAction<TMP_FontAsset>), target, setFontMethod) as
                    UnityAction<TMP_FontAsset>;
            UnityEventTools.AddPersistentListener(comp.OnUpdateAsset, methodDelegate);
            comp.OnUpdateAsset.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
        }
    }
#endif

    [Serializable]
    public class UnityEventTmpFont : UnityEvent<TMP_FontAsset>
    {
    }

    [AddComponentMenu("Localization/Asset/" + nameof(LocalizedTmpFontEvent))]
    public class LocalizedTmpFontEvent : LocalizedAssetEvent<TMP_FontAsset, LocalizedTmpFont, UnityEventTmpFont>
    {
    }
}
