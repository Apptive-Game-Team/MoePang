using System;
using System.Globalization;
using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class ProfileCommentUI : MonoBehaviour, IPointerClickHandler
    {
        public const int MaxLength = 50;

        private TMP_Text label;
        private TMP_InputField input;
        private TMP_Text status;
        private bool saving;
        private bool editing;
        private bool suppressEndEdit;
        private string accountId;
        private string original;

        public static string Normalize(string value)
        {
            value = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
            var indexes = StringInfo.ParseCombiningCharacters(value);
            return indexes.Length > MaxLength ? value.Substring(0, indexes[MaxLength]) : value;
        }

        public static string Display(string value, string nickname)
        {
            return string.IsNullOrWhiteSpace(value) ? $"안녕하세요, {nickname}입니다." : value;
        }

        private string CurrentAccount => GameManager.Instance.isLoggedIn
            ? SupabaseLoginManager.Instance.profile?.Id : null;
        private string Stored => SupabaseLoginManager.Instance.profile?.Comment;
        private string Nickname => SupabaseLoginManager.Instance.profile?.Nickname;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            label.richText = false;
            label.raycastTarget = true;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20;
            label.fontSizeMax = label.fontSize;

            BuildInput();
        }

        private void OnEnable()
        {
            if (label != null)
            {
                Refresh();
            }
        }

        private void LateUpdate()
        {
            if (!editing && !saving)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            label.text = GameManager.Instance.isLoggedIn ? Display(Stored, Nickname) : string.Empty;
        }

        private void BuildInput()
        {
            var go = new GameObject("CommentInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            var rect = (RectTransform)go.transform;
            rect.SetParent(transform.parent, false);

            var source = (RectTransform)transform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition3D = source.anchoredPosition3D;
            rect.localRotation = source.localRotation;
            rect.localScale = source.localScale;

            var background = go.GetComponent<Image>();
            background.color = new Color(1, 1, 1, 0.12f);

            input = go.GetComponent<TMP_InputField>();

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            var vr = (RectTransform)viewport.transform;
            vr.SetParent(rect, false);
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = new Vector2(6, 6);
            vr.offsetMax = new Vector2(-6, -6);

            var text = CreateText(vr);
            var tr = (RectTransform)text.transform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            text.raycastTarget = false;

            input.textViewport = vr;
            input.textComponent = text;
            input.targetGraphic = background;
            input.richText = false;
            input.lineType = TMP_InputField.LineType.MultiLineSubmit;

            input.onValueChanged.AddListener(value =>
            {
                var indexes = StringInfo.ParseCombiningCharacters(value);

                if (indexes.Length > MaxLength)
                {
                    input.SetTextWithoutNotify(value.Substring(0, indexes[MaxLength]));
                }
            });

            input.onEndEdit.AddListener(Commit);

            status = CreateText(transform);
            var sr = (RectTransform)status.transform;
            sr.anchorMin = new Vector2(0, 0);
            sr.anchorMax = new Vector2(1, 0);
            sr.pivot = new Vector2(0.5f, 1);
            sr.sizeDelta = new Vector2(0, 32);
            sr.anchoredPosition = new Vector2(0, -4);
            status.fontSize = 18;
            status.enableAutoSizing = false;
            status.raycastTarget = false;
            status.text = "";

            go.SetActive(false);
        }

        private TMP_Text CreateText(Transform parent)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<TMP_Text>();
            text.font = label.font;
            text.fontSharedMaterial = label.fontSharedMaterial;
            text.fontSize = label.fontSize;
            text.color = label.color;
            text.alignment = label.alignment;
            text.enableAutoSizing = true;
            text.fontSizeMin = 16;
            text.fontSizeMax = label.fontSize;
            text.richText = false;

            return text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!GameManager.Instance.isLoggedIn || saving || editing)
            {
                return;
            }

            original = Stored ?? "";
            accountId = CurrentAccount;

            input.SetTextWithoutNotify(Display(original, Nickname));
            status.text = "최대 50자 · 입력 완료 시 저장";

            editing = true;
            label.enabled = false;

            input.gameObject.SetActive(true);
            input.Select();
            input.ActivateInputField();
        }

        private async void Commit(string value)
        {
            if (suppressEndEdit || !editing || saving)
            {
                return;
            }

            editing = false;

            if (input.wasCanceled)
            {
                Finish(null);
                return;
            }

            var normalized = Normalize(value);

            if (value == Display(original, Nickname))
            {
                normalized = original;
            }

            if (normalized == original)
            {
                Finish(null);
                return;
            }

            if (!GameManager.Instance.isLoggedIn || accountId != CurrentAccount)
            {
                Finish("계정이 변경되었습니다. 다시 입력해주세요.");
                return;
            }

            saving = true;
            input.interactable = false;
            status.text = "저장 중…";

            string error = null;

            try
            {
                var profile = SupabaseLoginManager.Instance.profile;

                normalized = Display(normalized, profile.Nickname);

                if (!await SupabaseLoginManager.Instance.ProfileRepository.UpdateComment(normalized))
                {
                    throw new InvalidOperationException("Comment save failed.");
                }

                profile.Comment = normalized;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("프로필 코멘트 저장 실패: " + ex.Message);
                error = "저장 실패 · 다시 클릭해서 입력해주세요.";
            }
            finally
            {
                saving = false;

                if (this != null)
                {
                    Finish(error);
                }
            }
        }

        private void Finish(string message)
        {
            suppressEndEdit = true;
            input.gameObject.SetActive(false);
            input.interactable = true;
            label.enabled = true;
            status.text = message ?? "";

            Refresh();
            suppressEndEdit = false;
        }

        private void OnDisable()
        {
            if (input == null)
            {
                return;
            }

            if (editing)
            {
                Commit(input.text);
            }

            if (input != null)
            {
                input.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (input != null)
            {
                Destroy(input.gameObject);
            }
        }
    }
}



