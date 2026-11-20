using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;
using UnityEngine;

public static class GoogleSheetPullButton
{
    private const string CredentialsPath = "UserSettings/Localization/google-oauth.json";
    private static bool isPulling;

    [MenuItem("Debug Tool/Pull Google Sheets Localization Data")]
    public static async void PullFromGoogleSheets()
    {
        if (isPulling)
        {
            Debug.LogWarning("Google Sheets 인증 또는 Pull이 이미 진행 중입니다.");
            return;
        }
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
        isPulling = true;
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

            // Localization 1.5.9 calls RunSynchronously on an async task in its
            // synchronous OAuth path. Populate the token cache asynchronously first.
            // Reflection avoids exposing the package's non-auto-referenced Google DLL
            // types to Assembly-CSharp-Editor.
            using (var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
            {
                var authorize = typeof(SheetsServiceProvider).GetMethod("AuthorizeOAuthAsync");
                await (Task)authorize.Invoke(provider, new object[] { timeout.Token });
            }

            var sheets = new GoogleSheets(provider) { SpreadSheetId = extension.SpreadsheetId };
            sheets.PullIntoStringTableCollection(extension.SheetId, collection, extension.Columns,
                extension.RemoveMissingPulledKeys, new ProgressReporter(), true);
            Debug.Log("Google Sheets Pull 완료: LocalizationDataTable을 갱신했습니다.");
        }
        catch (Exception exception)
        {
            if (exception is TargetInvocationException && exception.InnerException != null)
                exception = exception.InnerException;
            string message = exception.Message;
            foreach (string secret in new[] { provider.ClientId, provider.ClientSecret })
                if (!string.IsNullOrEmpty(secret)) message = message.Replace(secret, "[redacted]");
            Debug.LogError($"Google Sheets Pull 실패 ({exception.GetType().Name}): {message}");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(provider);
            isPulling = false;
        }
    }
}
