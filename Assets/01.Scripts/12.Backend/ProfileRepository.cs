using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace _01.Scripts._12.Backend
{
    public class ProfileRepository
    {
        private Supabase.Client Client => SupabaseManager.Instance.Client;

        public async Task<bool> CreateProfile(string nickname)
        {
            try
            {
                await Client.Rpc("create_user_profile", new { p_nickname = nickname });
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
                    Debug.LogError("GetProfile Failed - User is not logged in.");

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

        public async Task<bool> UpdateNickname(string nickname)
        {
            try
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    Debug.LogError("UpdateNickname Failed - User is not logged in.");

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
    }
}