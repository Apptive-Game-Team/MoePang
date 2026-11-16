using _01.Scripts._00.Manager;
using _01.Scripts._04.UI.MainScene.Profile;
using _01.Scripts._12.Backend;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private RankerProfileUI rankerUI;
        [SerializeField] private RankingUserData myRanking;

        [Header("Avatar")]
        [SerializeField] private AvatarDatabase avatarDatabase;

        private Sprite _googleAvatarSprite;
        private _12.Backend.Profile _profile; 
        private const int RankingCount = 1000;

        private void Awake()
        {
            _profile = SupabaseLoginManager.Instance.profile;
            _googleAvatarSprite = GameManager.Instance.googleAvatarSprite ?? avatarDatabase.GetAvatar(_profile.AvatarId);
        }

        private async void OnEnable()
        {
            try
            {
                await SupabaseManager.Instance.InitializationTask;
                await GetUserStageRanking();
                await GetStageRanking();
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
                
                List<Task> avatarTasks = new();

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
                        rankerUI.InitialSetting(entry, avatar);
                        rankerUI.gameObject.SetActive(true);
                    });
                    
                    if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                    {
                        avatarTasks.Add(ApplyGoogleAvatarAsync(entry.GoogleAvatarUrl, sprite =>
                        {
                            avatar = sprite;
                            ranker.GetComponent<Image>().sprite = avatar;
                            
                            button.onClick.RemoveAllListeners();
                            button.onClick.AddListener(() =>
                            {
                                rankerUI.InitialSetting(entry, avatar);
                                rankerUI.gameObject.SetActive(true);
                            });
                        } ));
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
                        avatarTasks.Add(ApplyGoogleAvatarAsync(entry.GoogleAvatarUrl, userData.SetProfileImage));
                    }
                }
                
                await Task.WhenAll(avatarTasks);
                
                for (int i = 0; i < Math.Min(3, rankingList.Count); i++)
                {
                    topRankingUsers[i].SetActive(true);
                }
                
                foreach (Transform child in rankingPanel.transform)
                {
                    child.gameObject.SetActive(true);
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
                
                if (rankingEntry == null || rankingEntry.Count == 0)
                {
                    Debug.LogError("No Ranking Data");
                    return;
                }

                RankingEntry entry = rankingEntry[0];
                
                Sprite avatar = avatarDatabase.GetAvatar(entry.AvatarId);

                myRanking.ProfileSetting((int)entry.Ranking, avatar, entry.Nickname, entry.MaxStage, entry.MinUsedTime);
                    
                if (entry.IsGoogleAvatar && !string.IsNullOrEmpty(entry.GoogleAvatarUrl))
                {
                    myRanking.SetProfileImage(_googleAvatarSprite);
                }
                
                myRanking.gameObject.SetActive(true);
            }
        }
        
        private async Task ApplyGoogleAvatarAsync(string imageUrl, Action<Sprite> applySprite)
        {
            try
            {
                Sprite sprite = await LoadGoogleAvatarAsync(imageUrl);

                if (sprite != null)
                {
                    applySprite?.Invoke(sprite);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Google Avatar Load Failed: {e.Message}");
            }
        }

        private async Task<Sprite> LoadGoogleAvatarAsync(string imageUrl)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            request.timeout = 10;

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Google Avatar Load Failed: {request.error}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (!texture)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}