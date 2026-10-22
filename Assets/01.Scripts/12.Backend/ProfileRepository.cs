using Supabase.Postgrest;
using Supabase.Postgrest.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Client = Supabase.Client;

namespace _01.Scripts._12.Backend
{
    public class ProfileRepository
    {
        private Supabase.Client Client => SupabaseManager.Instance.Client;

        public async Task<bool> CreateProfile(string nickname)
        {
            try
            {
                await Client.Rpc(
                    "create_user_profile",
                    new { p_nickname = nickname }
                );

                Debug.Log("Profile Create Success via RPC!");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Profile Create Failed\n{e}");

                return false;
            }
        }
        
        public async Task<bool> CreateProfile(string nickname, string googleAvatarUrl)
        {
            try
            {
                await Client.Rpc(
                    "create_user_profile_mobile",
                    new
                    {
                        p_nickname = nickname,
                        p_google_avatar_url = googleAvatarUrl
                    }
                );

                Debug.Log("Profile Create Success via RPC!");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Profile Create Failed\n{e}");
                
                return false;
            }
        }

        public async Task<Profile> GetProfile()
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    Debug.LogError(
                        "GetProfile Failed - User is not logged in."
                    );

                    return null;
                }

                Profile profile = await Client
                    .From<Profile>()
                    .Where(x => x.Id == user.Id)
                    .Single();

                if (profile == null)
                {
                    Debug.Log($"Profile Not Found - User ID: {user.Id}");

                    return null;
                }

                Debug.Log(
                    $"Profile Get Success - Nickname: {profile.Nickname}"
                );

                return profile;
            }
            catch (Exception e)
            {
                Debug.LogError($"Profile Get Failed\n{e}");

                return null;
            }
        }

        public async Task<bool> Exists()
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    return false;
                }

                Profile profile = await Client
                    .From<Profile>()
                    .Where(x => x.Id == user.Id)
                    .Single();

                return profile != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAvatarMode(bool flag)
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    Debug.LogError(
                        "AvatarMode Failed - User is not logged in."
                    );

                    return false;
                }

                await Client
                    .From<Profile>()
                    .Set(x => x.IsGoogleAvatar, flag)
                    .Where(x => x.Id == user.Id)
                    .Update();

                Debug.Log($"AvatarMode Update Success - {flag}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AvatarMode Update Failed\n{e}");

                return false;
            }
        }
        
        public async Task<bool> UpdateAvatar(int avatarId)
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    Debug.LogError(
                        "AvatarId Failed - User is not logged in."
                    );

                    return false;
                }

                await Client
                    .From<Profile>()
                    .Set(x => x.AvatarId, avatarId)
                    .Where(x => x.Id == user.Id)
                    .Update();

                Debug.Log($"AvatarId Update Success - {avatarId}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AvatarId Update Failed\n{e}");

                return false;
            }
        }
        

        public async Task<bool> UpdateNickname(string nickname)
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    Debug.LogError(
                        "UpdateNickname Failed - User is not logged in."
                    );

                    return false;
                }

                await Client
                    .From<Profile>()
                    .Set(x => x.Nickname, nickname)
                    .Where(x => x.Id == user.Id)
                    .Update();

                Debug.Log($"Nickname Update Success - {nickname}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Nickname Update Failed\n{e}");

                return false;
            }
        }
        
        public async Task<int> GetMyMaxStage()
        {
            Client client = SupabaseManager.Instance.Client;
            string userId = client.Auth.CurrentUser.Id;

            ModeledResponse<PlayerProgressModel> result = await client
                .From<PlayerProgressModel>()
                .Filter("user_id", Constants.Operator.Equals, userId)
                .Get();

            if (result.Models.Count == 0)
            {
                Debug.LogWarning("PlayerProgress 데이터가 없습니다.");
                return 0;
            }

            int maxStage = result.Models.Max(x => x.MaxStage);

            return maxStage;
        }
    }
}