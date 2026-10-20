using _01.Scripts._04.UI.MainScene.Profile;
using _01.Scripts._12.Backend;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingUI : MonoBehaviour
    {
        [Header("Ranking UI")]
        [SerializeField] private GameObject rankingPanel;
        [SerializeField] private GameObject rankingUserPrefab;
        [SerializeField] private List<GameObject> topRankingUsers;
        [SerializeField] private RankingUserData rankerUserData;
        [SerializeField] private RankingUserData myRanking;

        [Header("Avatar")]
        [SerializeField] private AvatarDatabase avatarDatabase;

        private const int RankingCount = 100;

        private async void OnEnable()
        {
            try
            {
                await SupabaseManager.Instance.InitializationTask;
                await GetStageRanking();
                await GetUserStageRanking();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ranking Load Failed: {e}");
            }
        }

        private void OnDisable()
        {
            foreach (GameObject ranker in topRankingUsers)
            {
                ranker.SetActive(false);
            }
            
            foreach (Transform child in rankingPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private async Task GetStageRanking()
        {
            var client = SupabaseManager.Instance.Client;

            var response = await client.Rpc(
                "get_stage_ranking",
                new
                {
                    p_limit = RankingCount
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

                for (int i = 0; i < Math.Min(3, rankingList.Count); i++)
                {
                    RankingEntry entry = rankingList[i];

                    GameObject ranker = topRankingUsers[i];
                    Button button = ranker.GetComponent<Button>();
                    
                    Sprite avatar = avatarDatabase.GetAvatar(entry.AvatarId);
                    ranker.GetComponent<Image>().sprite = avatar;
                    ranker.GetComponentInChildren<TextMeshProUGUI>().text = entry.Nickname;

                    button.onClick.AddListener(() =>
                    {
                        rankerUserData.ProfileSetting((int)entry.Ranking, avatar, entry.Nickname, entry.MaxStage, entry.MinUsedTime);
                        rankerUserData.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        {
                            rankerUserData.gameObject.SetActive(false);
                        });
                        rankerUserData.gameObject.SetActive(true);
                    });
                    
                    if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                    {
                        StartCoroutine(LoadGoogleAvatar(entry.GoogleAvatarUrl,ranker.GetComponent<Image>()));
                        StartCoroutine(LoadGoogleAvatar(entry.GoogleAvatarUrl, rankerUserData));
                    }
                    
                    ranker.SetActive(true);
                }

                for (int i = 3; i < rankingList.Count; i++)
                {
                    RankingEntry entry = rankingList[i];
                    
                    GameObject obj = Instantiate(rankingUserPrefab, rankingPanel.transform);

                    RankingUserData userData = obj.GetComponent<RankingUserData>();
                    
                    Sprite avatar = avatarDatabase.GetAvatar(entry.AvatarId);

                    userData.ProfileSetting((int)entry.Ranking, avatar, entry.Nickname, entry.MaxStage, entry.MinUsedTime);
                    
                    if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                    {
                        StartCoroutine(LoadGoogleAvatar(entry.GoogleAvatarUrl, userData));
                    }
                }
            }
        }

        private async Task GetUserStageRanking()
        {
            var client = SupabaseManager.Instance.Client;

            var response = await client.Rpc(
                "get_user_stage_ranking",
                new
                {
                    p_user_id = client.Auth.CurrentUser.Id
                });

            if (response.Content != null)
            {
                List<RankingEntry> rankingEntry = JsonConvert.DeserializeObject<List<RankingEntry>>(response.Content);
                
                if (rankingEntry == null)
                {
                    Debug.LogError("Ranking JSON Parse Failed");
                    return;
                }

                RankingEntry entry = rankingEntry[0];
                
                Sprite avatar = avatarDatabase.GetAvatar(entry.AvatarId);

                myRanking.ProfileSetting((int)entry.Ranking, avatar, entry.Nickname, entry.MaxStage, entry.MinUsedTime);
                    
                if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                {
                    StartCoroutine(LoadGoogleAvatar(entry.GoogleAvatarUrl, myRanking));
                }
            }
        }

        private IEnumerator LoadGoogleAvatar(string imageUrl, Image image)
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
            
            image.sprite = sprite;
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