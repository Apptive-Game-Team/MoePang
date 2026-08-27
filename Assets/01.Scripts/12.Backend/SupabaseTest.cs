using System.Threading.Tasks;
using UnityEngine;

namespace _01.Scripts._12.Backend
{
    public class SupabaseTest : MonoBehaviour
    {
        private TestDataRepository _repository;

        private async void Start()
        {
            await WaitForSupabase();

            _repository = new TestDataRepository();

            await TestCrud();
        }

        private async Task WaitForSupabase()
        {
            while (SupabaseManager.Instance == null || SupabaseManager.Instance.Client == null)
            {
                await Task.Yield();
            }
        }

        private async Task TestCrud()
        {
            Debug.Log("========== SUPABASE CRUD TEST START ==========");

            // INSERT
            TestData insertedData = await _repository.Insert(
                "Hello Supabase",
                100
            );

            if (insertedData == null)
            {
                return;
            }

            Debug.Log(
                $"Inserted Data - Id: {insertedData.Id}, " +
                $"Message: {insertedData.Message}, " +
                $"Number: {insertedData.NumberValue}"
            );

            // SELECT
            TestData selectedData = await _repository.SelectById(
                insertedData.Id
            );

            if (selectedData == null)
            {
                return;
            }

            Debug.Log(
                $"Selected Data - Id: {selectedData.Id}, " +
                $"Message: {selectedData.Message}, " +
                $"Number: {selectedData.NumberValue}"
            );

            // UPDATE
            bool updateSuccess = await _repository.Update(
                insertedData.Id,
                "Hello Unity",
                999
            );

            if (!updateSuccess)
            {
                return;
            }

            // UPDATE 결과 SELECT
            selectedData = await _repository.SelectById(
                insertedData.Id
            );

            if (selectedData == null)
            {
                return;
            }

            Debug.Log(
                $"Updated Data - Id: {selectedData.Id}, " +
                $"Message: {selectedData.Message}, " +
                $"Number: {selectedData.NumberValue}"
            );

            // DELETE
            bool deleteSuccess = await _repository.Delete(
                insertedData.Id
            );

            if (!deleteSuccess)
            {
                return;
            }

            // DELETE 결과 확인
            selectedData = await _repository.SelectById(
                insertedData.Id
            );

            Debug.Log(
                $"After Delete - Data Exists: {selectedData != null}"
            );

            Debug.Log("========== SUPABASE CRUD TEST END ==========");
        }
    }
}