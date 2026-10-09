using _01.Scripts._04.UI.MainScene.Profile;
using _01.Scripts._12.Backend;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingUI : MonoBehaviour
    {
        [Header("Ranking UI")]
        [SerializeField] private GameObject rankingPanel;
        [SerializeField] private GameObject rankingUserPrefab;

        [Header("Avatar")]
        [SerializeField] private AvatarDatabase avatarDatabase;

        private async void OnEnable()
        {
            try
            {
                await SupabaseManager.Instance.InitializationTask;
                await GetStageRanking();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ranking Load Failed: {e}");
            }
        }

        private async Task GetStageRanking()
        {
            var client = SupabaseManager.Instance.Client;

            var response = await client.Rpc(
                "get_stage_ranking",
                new
                {
                    p_limit = 100
                }
            );

            if (response.Content != null)
            {
                List<RankingEntry> rankingList = JsonConvert.DeserializeObject<List<RankingEntry>>(response.Content);

                if (rankingList == null)
                {
                    Debug.LogError("Ranking JSON Parse Failed");
                    return;
                }

                Debug.Log($"Ranking Loaded: {rankingList.Count}");

                foreach (Transform child in rankingPanel.transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (RankingEntry entry in rankingList)
                {
                    GameObject obj = Instantiate(rankingUserPrefab, rankingPanel.transform);

                    RankingUserData userData = obj.GetComponent<RankingUserData>();
                    
                    Sprite avatar = avatarDatabase.GetAvatar(entry.AvatarId);

                    userData.ProfileSetting((int)entry.Ranking, avatar, entry.Nickname, entry.MaxStage);
                    
                    if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                    {
                        StartCoroutine(LoadGoogleAvatar(entry.GoogleAvatarUrl, userData));
                    }
                }
            }
        }

        private IEnumerator LoadGoogleAvatar(string imageUrl, RankingUserData userData)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Google Avatar Load Failed: {request.error}");
                
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (!texture)
            {
                yield break;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            if (userData)
            {
                userData.SetProfileImage(sprite);
            }
            else
            {
                Destroy(sprite);
                Destroy(texture);
            }
        }
    }
}