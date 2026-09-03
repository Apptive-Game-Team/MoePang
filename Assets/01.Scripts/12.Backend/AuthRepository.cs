using System;
using System.Threading.Tasks;
using UnityEngine;

namespace _01.Scripts._12.Backend
{
    public class AuthRepository
    {
        private Supabase.Client Client => SupabaseManager.Instance.Client;

        public bool IsLoggedIn()
        {
            return Client.Auth.CurrentUser != null &&
                   Client.Auth.CurrentSession != null;
        }

        public async Task Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is empty.");
            }

            await Client.Auth.SignIn(email, password);

            Debug.Log(
                $"Login success. UserId: {Client.Auth.CurrentUser?.Id}"
            );
        }

        public async Task SignUp(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is empty.");
            }

            await Client.Auth.SignUp(email, password);

            Debug.Log(
                $"SignUp success. UserId: {Client.Auth.CurrentUser?.Id}"
            );
        }

        public string GetCurrentUserId()
        {
            Supabase.Gotrue.User user = Client.Auth.CurrentUser;

            if (user == null)
            {
                return null;
            }

            return user.Id;
        }

        public async Task Logout()
        {
            await Client.Auth.SignOut();
        }
    }
}