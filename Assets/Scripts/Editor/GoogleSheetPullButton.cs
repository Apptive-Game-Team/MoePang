using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;
using UnityEngine;

public static class GoogleSheetPullButton
{
    private const string CredentialsPath = "UserSettings/Localization/google-oauth.json";

    [MenuItem("Debug Tool/Pull Google Sheets Localization Data")]
    public static void PullFromGoogleSheets()
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection("LocalizationDataTable");
        var extension = collection?.Extensions.OfType<GoogleSheetsExtension>().FirstOrDefault();
        if (extension == null || extension.SheetsServiceProvider == null)
        {
            Debug.LogError("LocalizationDataTable의 Google Sheets Extension과 공용 Provider 연결을 확인해 주세요.");
            return;
        }

        string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), CredentialsPath);
        if (!File.Exists(path))
        {
            Debug.LogError($"Google OAuth JSON을 프로젝트의 {CredentialsPath}에 저장해 주세요. 이 파일은 Git에서 제외됩니다.");
            return;
        }

        // Keep credentials off the shared asset; use a transient copy for this pull.
        var provider = UnityEngine.Object.Instantiate(extension.SheetsServiceProvider);
        provider.name = extension.SheetsServiceProvider.name;
        provider.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            try
            {
                provider.SetOAuthCredentials(File.ReadAllText(path));
                if (string.IsNullOrWhiteSpace(provider.ClientId) || string.IsNullOrWhiteSpace(provider.ClientSecret))
                    throw new InvalidDataException();
            }
            catch (Exception)
            {
                // Parser exceptions may contain secrets. Never print their contents.
                Debug.LogError($"{CredentialsPath}를 읽을 수 없거나 유효한 OAuth 클라이언트 JSON이 아닙니다. client_id와 client_secret을 확인해 주세요.");
                return;
            }

            var sheets = new GoogleSheets(provider) { SpreadSheetId = extension.SpreadsheetId };
            sheets.PullIntoStringTableCollection(extension.SheetId, collection, extension.Columns,
                extension.RemoveMissingPulledKeys, new ProgressReporter(), true);
        }
        catch (Exception)
        {
            Debug.LogError("Google Sheets Pull에 실패했습니다. Google 로그인, 시트 접근 권한 및 네트워크를 확인해 주세요.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(provider);
        }
    }
}
