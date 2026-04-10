using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;
using UnityEditor.Localization;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GoogleSheetPullButton : Editor
{
    static StringTableCollection googleSheetCollection;

    static GoogleSheetPullButton()
    {
        googleSheetCollection = LocalizationEditorSettings.GetStringTableCollection("LocalizationDataTable");
    }

    [MenuItem("Debug Tool/Pull Google Sheets Localization Data")]
    public static void PullFromGoogleSheets()
    {
        GoogleSheetsExtension extension = googleSheetCollection.Extensions[0] as GoogleSheetsExtension;
        SheetsServiceProvider sheetsServiceProvider = extension.SheetsServiceProvider;

        GoogleSheets googleSheets = new GoogleSheets(sheetsServiceProvider);
        googleSheets.SpreadSheetId = extension.SpreadsheetId;
        googleSheets.PullIntoStringTableCollection(extension.SheetId, googleSheetCollection, extension.Columns, extension.RemoveMissingPulledKeys, new ProgressReporter(), true);
        //Debug.Log("Pulled localization data from Google Sheets.");
    }
}
