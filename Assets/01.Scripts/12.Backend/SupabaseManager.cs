using System;
using System.Threading.Tasks;
using UnityEngine;

namespace _01.Scripts._12.Backend
{
    public class SupabaseManager : MonoBehaviour
    {
        public static SupabaseManager Instance { get; private set; }

        public Supabase.Client Client { get; private set; }

        public Task InitializationTask { get; private set; }

        [SerializeField] private string _supabaseUrl;
        [SerializeField] private string _supabasePublishableKey;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            InitializationTask = Initialize();

            await InitializationTask;
        }

        private async Task Initialize()
        {
            try
            {
                Debug.Log("Supabase Initialize Start");

                Client = new Supabase.Client(
                    _supabaseUrl,
                    _supabasePublishableKey
                );

                await Client.InitializeAsync();

                Debug.Log("Supabase Initialize Success");
            }
            catch (Exception e)
            {
                Debug.LogError($"Supabase Initialize Failed\n{e}");

                throw;
            }
        }
    }
}