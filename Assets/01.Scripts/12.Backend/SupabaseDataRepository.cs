using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using _01.Scripts._00.Manager;
using _01.Scripts._06.Shop;
using Supabase.Postgrest;
using ItemData = _01.Scripts._00.Manager.ItemData;
using UnitData = _01.Scripts._00.Manager.UnitData;

namespace _01.Scripts._12.Backend
{
    public class SupabaseDataRepository
    {
        private Supabase.Client Client => SupabaseManager.Instance.Client;

        private string UserId
        {
            get
            {
                Supabase.Gotrue.User user = Client.Auth.CurrentUser;

                if (user == null)
                {
                    throw new InvalidOperationException("Supabase user is not logged in.");
                }

                return user.Id;
            }
        }


        public bool IsLoggedIn()
        {
            return Client.Auth.CurrentUser != null &&
                   Client.Auth.CurrentSession != null;
        }


        public async Task CreateInitialGameData()
        {
            if (!IsLoggedIn())
            {
                throw new InvalidOperationException(
                    "Cannot create game data without a logged-in user.");
            }

            try
            {
                await Client.Rpc(
                    "create_user_game_data",
                    new Dictionary<string, object>()
                );

                Debug.Log("Create initial game data success.");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Create initial game data failed\n{e}"
                );

                throw;
            }
        }


        public async Task LoadAllData(
            PlayData playData,
            CastleData castleData,
            _00.Manager.UnitData unitData,
            ItemData itemData,
            ComboData comboData,
            FriendlyUnitList unitList)
        {
            if (!IsLoggedIn())
            {
                throw new InvalidOperationException(
                    "Cannot load Supabase data without a logged-in user.");
            }

            try
            {
                await LoadPlayerData(playData, castleData);
                await LoadProgressData(playData);
                await LoadStageData(playData);
                await LoadUnitData(unitData, unitList);
                await LoadItemData(itemData);
                await LoadComboData(comboData);

                Debug.Log("Load all Supabase data success.");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Load all Supabase data failed\n{e}"
                );

                throw;
            }
        }


        private async Task LoadPlayerData(PlayData playData, CastleData castleData)
        {
            Supabase.Postgrest.Responses.ModeledResponse<PlayerDataModel> response =
                await Client
                    .From<PlayerDataModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals,UserId)
                    .Get();

            if (response.Models.Count == 0)
            {
                Debug.LogWarning("PlayerData does not exist.");
                return;
            }

            PlayerDataModel data = response.Models[0];

            playData.goldAmount = data.Gold;
            playData.diaAmount = data.Dia;

            castleData.castleLevel = data.CastleLevel;
        }


        private async Task LoadProgressData(PlayData playData)
        {
            Supabase.Postgrest.Responses.ModeledResponse<PlayerProgressModel> response =
                await Client
                    .From<PlayerProgressModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals,UserId)
                    .Get();

            playData.MaxStages.Clear();
            playData.selectedStage.Clear();

            foreach (StageType stageType in Enum.GetValues(typeof(StageType)))
            {
                playData.MaxStages[stageType] = 0;
                playData.selectedStage.Add(0);
            }

            foreach (PlayerProgressModel data in response.Models)
            {
                StageType stageType = (StageType)data.StageType;

                playData.MaxStages[stageType] = data.MaxStage;
                playData.selectedStage[(int)stageType] = data.SelectedStage;
            }
        }


        private async Task LoadStageData(PlayData playData)
        {
            Supabase.Postgrest.Responses.ModeledResponse<PlayerStageModel> response =
                await Client
                    .From<PlayerStageModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals, UserId)
                    .Get();

            playData.StageData.Clear();

            foreach (StageType stageType in Enum.GetValues(typeof(StageType)))
            {
                playData.StageData[stageType] =
                    new Dictionary<int, StageData>();
            }

            foreach (PlayerStageModel data in response.Models)
            {
                StageType stageType = (StageType)data.StageType;

                StageData stageData = new()
                {
                    stageNum = data.StageNum,
                    minUsedTime = data.MinUsedTime,
                    minUsedTile = data.MinUsedTile,
                    maxUsedTile = data.MaxUsedTile
                };

                int dictionaryKey = data.StageNum - 1;

                playData.StageData[stageType][dictionaryKey] = stageData;
            }
        }


        private async Task LoadUnitData(_00.Manager.UnitData unitData, FriendlyUnitList unitList)
        {
            if (unitList == null)
            {
                Debug.LogError("FriendlyUnitList is null.");
                return;
            }

            Supabase.Postgrest.Responses.ModeledResponse<PlayerUnitModel> response =
                await Client
                    .From<PlayerUnitModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals, UserId)
                    .Get();

            unitData.UnlockedUnits ??= new Dictionary<FriendlyUnitData, bool>();

            unitData.UnitLevels ??= new Dictionary<FriendlyUnitData, int>();

            unitData.UnlockedUnits.Clear();
            unitData.UnitLevels.Clear();

            Dictionary<UnitName, FriendlyUnitData> unitMap = CreateUnitMap(unitList);

            foreach (PlayerUnitModel data in response.Models)
            {
                UnitName unitName = (UnitName)data.UnitId;

                if (!unitMap.TryGetValue(unitName, out FriendlyUnitData unit))
                {
                    Debug.LogWarning($"Cannot find FriendlyUnitData for UnitId: {data.UnitId}");

                    continue;
                }

                unitData.UnlockedUnits[unit] = data.Unlocked;
                unitData.UnitLevels[unit] = data.Level;
            }

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                List<FriendlyUnitData> units = unitList.GetUnits(habitat);

                if (units == null || units.Count == 0)
                {
                    continue;
                }

                foreach (FriendlyUnitData unit in units)
                {
                    unitData.UnlockedUnits.TryAdd(unit, false);

                    if (!unitData.UnitLevels.ContainsKey(unit))
                    {
                        unitData.UnitLevels[unit] =
                            Mathf.Max(
                                1,
                                Mathf.RoundToInt(unit.BaseUnitLevel)
                            );
                    }
                }
                
                unitData.UnlockedUnits[units[0]] = true;
            }
        }


        private Dictionary<UnitName, FriendlyUnitData> CreateUnitMap(FriendlyUnitList unitList)
        {
            Dictionary<UnitName, FriendlyUnitData> map = new();

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                List<FriendlyUnitData> units =
                    unitList.GetUnits(habitat);

                if (units == null)
                {
                    continue;
                }

                foreach (FriendlyUnitData unit in units)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    map[unit.UnitName] = unit;
                }
            }

            return map;
        }


        private async Task LoadItemData(ItemData itemData)
        {
            Supabase.Postgrest.Responses.ModeledResponse<PlayerItemModel> response =
                await Client
                    .From<PlayerItemModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals, UserId)
                    .Get();

            itemData.ItemAmounts.Clear();

            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                itemData.ItemAmounts[itemType] = 0;
            }

            foreach (PlayerItemModel data in response.Models)
            {
                ItemType itemType = (ItemType)data.ItemType;

                itemData.ItemAmounts[itemType] = data.Amount;
            }
        }


        private async Task LoadComboData(ComboData comboData)
        {
            Supabase.Postgrest.Responses.ModeledResponse<PlayerComboModel> response =
                await Client
                    .From<PlayerComboModel>()
                    .Select("*")
                    .Filter("user_id", Constants.Operator.Equals,UserId)
                    .Get();

            comboData.ComboLevels.Clear();

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                comboData.ComboLevels[habitat] = 1;
            }

            foreach (PlayerComboModel data in response.Models)
            {
                Habitat habitat = (Habitat)data.Habitat;

                comboData.ComboLevels[habitat] = data.Level;
            }
        }


        public async Task SavePlayData(PlayData playData)
        {
            await SavePlayerData(playData.goldAmount, playData.diaAmount, null);

            await SaveProgressData(playData);
            await SaveStageData(playData);
        }


        public async Task SaveCastleData(CastleData castleData, PlayData playData)
        {
            await SavePlayerData(playData.goldAmount, playData.diaAmount, castleData.castleLevel);
        }


        public async Task SaveUnitData(_00.Manager.UnitData unitData, PlayData playData)
        {
            await SavePlayerData(playData.goldAmount, playData.diaAmount, null);

            List<PlayerUnitModel> rows = new();

            foreach (KeyValuePair<FriendlyUnitData, bool> pair in unitData.UnlockedUnits)
            {
                if (pair.Key == null)
                {
                    continue;
                }

                rows.Add(
                    new PlayerUnitModel
                    {
                        UserId = UserId,
                        UnitId = (int)pair.Key.UnitName,
                        Unlocked = pair.Value,
                        Level = unitData.GetUnitLevel(pair.Key)
                    }
                );
            }

            if (rows.Count > 0)
            {
                await Client
                    .From<PlayerUnitModel>()
                    .Upsert(rows);
            }
        }


        public async Task SaveItemData(ItemData itemData, PlayData playData)
        {
            await SavePlayerData(playData.goldAmount, playData.diaAmount, null);

            List<PlayerItemModel> rows =
                new();

            foreach (KeyValuePair<ItemType, int> pair in itemData.ItemAmounts)
            {
                rows.Add(
                    new PlayerItemModel
                    {
                        UserId = UserId,
                        ItemType = (int)pair.Key,
                        Amount = pair.Value
                    }
                );
            }

            if (rows.Count > 0)
            {
                await Client
                    .From<PlayerItemModel>()
                    .Upsert(rows);
            }
        }


        public async Task SaveComboData(ComboData comboData)
        {
            List<PlayerComboModel> rows =
                new();

            foreach (KeyValuePair<Habitat, int> pair in comboData.ComboLevels)
            {
                rows.Add(
                    new PlayerComboModel
                    {
                        UserId = UserId,
                        Habitat = (int)pair.Key,
                        Level = pair.Value
                    }
                );
            }

            if (rows.Count > 0)
            {
                await Client
                    .From<PlayerComboModel>()
                    .Upsert(rows);
            }
        }


        private async Task SavePlayerData(int gold, int dia, int? castleLevel)
        {
            PlayerDataModel data =
                new()
                {
                    UserId = UserId,
                    Gold = gold,
                    Dia = dia,
                    CastleLevel = castleLevel ?? 1
                };

            if (castleLevel == null)
            {
                Supabase.Postgrest.Responses.ModeledResponse<PlayerDataModel> response = await Client
                        .From<PlayerDataModel>()
                        .Select("*")
                        .Filter("user_id", Constants.Operator.Equals,UserId)
                        .Get();

                if (response.Models.Count > 0)
                {
                    data.CastleLevel = response.Models[0].CastleLevel;
                }
            }

            await Client
                .From<PlayerDataModel>()
                .Upsert(data);
        }


        private async Task SaveProgressData(PlayData playData)
        {
            List<PlayerProgressModel> rows = new();

            foreach (StageType stageType in Enum.GetValues(typeof(StageType)))
            {
                int index = (int)stageType;

                int maxStage = playData.MaxStages.GetValueOrDefault(stageType, 0);

                int selectedStage = index < playData.selectedStage.Count
                        ? playData.selectedStage[index]
                        : 0;

                rows.Add(
                    new PlayerProgressModel
                    {
                        UserId = UserId,
                        StageType = index,
                        MaxStage = maxStage,
                        SelectedStage = selectedStage
                    }
                );
            }

            await Client
                .From<PlayerProgressModel>()
                .Upsert(rows);
        }


        private async Task SaveStageData(PlayData playData)
        {
            List<PlayerStageModel> rows = new();

            foreach (
                KeyValuePair<
                    StageType,
                    Dictionary<int, StageData>
                > stageTypePair in playData.StageData)
            {
                foreach (KeyValuePair<int, StageData> stagePair in stageTypePair.Value)
                {
                    StageData stage = stagePair.Value;

                    rows.Add(
                        new PlayerStageModel
                        {
                            UserId = UserId,
                            StageType = (int)stageTypePair.Key,
                            StageNum = stage.stageNum,
                            MinUsedTime = stage.minUsedTime,
                            MinUsedTile = stage.minUsedTile,
                            MaxUsedTile = stage.maxUsedTile
                        }
                    );
                }
            }

            if (rows.Count > 0)
            {
                await Client
                    .From<PlayerStageModel>()
                    .Upsert(rows);
            }
        }
    }
}