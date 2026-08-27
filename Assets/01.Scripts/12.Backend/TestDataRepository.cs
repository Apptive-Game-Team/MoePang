using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Postgrest;
using UnityEngine;

namespace _01.Scripts._12.Backend
{
    public class TestDataRepository
    {
        private Supabase.Client Client => SupabaseManager.Instance.Client;

        public async Task<TestData> Insert(string message, int numberValue)
        {
            try
            {
                TestData data = new TestData
                {
                    Message = message,
                    NumberValue = numberValue
                };

                var response = await Client
                    .From<TestData>()
                    .Insert(data);

                Debug.Log($"INSERT Success - Id: {response.Model.Id}");

                return response.Model;
            }
            catch (Exception e)
            {
                Debug.LogError($"INSERT Failed\n{e}");

                return null;
            }
        }

        public async Task<List<TestData>> SelectAll()
        {
            try
            {
                var response = await Client
                    .From<TestData>()
                    .Get();

                Debug.Log($"SELECT Success - Count: {response.Models.Count}");

                return response.Models;
            }
            catch (Exception e)
            {
                Debug.LogError($"SELECT Failed\n{e}");

                return null;
            }
        }

        public async Task<TestData> SelectById(long id)
        {
            try
            {
                TestData response = await Client
                    .From<TestData>()
                    .Where(x => x.Id == id)
                    .Single();

                if (response != null)
                {
                    Debug.Log($"SELECT Success - Id: {response.Id}");
                }
                
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"SELECT BY ID Failed\n{e}");

                return null;
            }
        }

        public async Task<bool> Update(long id, string message, int numberValue)
        {
            try
            {
                await Client
                    .From<TestData>()
                    .Set(x => x.Message, message)
                    .Set(x => x.NumberValue, numberValue)
                    .Where(x => x.Id == id)
                    .Update();

                Debug.Log($"UPDATE Success - Id: {id}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"UPDATE Failed\n{e}");

                return false;
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await Client
                    .From<TestData>()
                    .Where(x => x.Id == id)
                    .Delete();

                Debug.Log($"DELETE Success - Id: {id}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"DELETE Failed\n{e}");

                return false;
            }
        }
    }
}