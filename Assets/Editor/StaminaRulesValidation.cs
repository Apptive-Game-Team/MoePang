#if UNITY_EDITOR
using System;
using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using UnityEditor;
using UnityEngine;

public static class StaminaRulesValidation
{
    [MenuItem("Tools/MoePang/Validate Stamina Rules")]
    public static void RunFromMenu() => Debug.Log(Run());

    public static string Run()
    {
        int checks = 0;
        Action<bool, string> check = (valid, message) =>
        {
            if (!valid)
            {
                throw new Exception("Stamina validation failed: " + message);
            }

            checks++;
        };

        DateTime now = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

        var data = new StaminaData { stamina = 4, additionalStamina = 999, lastSavedDateTime = now };
        check(StaminaRules.TryConsume(data, 1, now) && data.stamina == 3 && data.additionalStamina == 999, "natural first");

        data.stamina = 1;
        check(StaminaRules.TryConsume(data, 2, now) && data.stamina == 0 && data.additionalStamina == 998, "split habitat payment");

        data.additionalStamina = 1;
        check(!StaminaRules.TryConsume(data, 2, now) && data.additionalStamina == 1 && data.stamina == 0, "insufficient payment is atomic");

        check(!StaminaRules.TryConsume(data, 0, now) && !StaminaRules.TryConsume(data, -1, now), "invalid payment");

        check(!StaminaRules.Recover(data, now.AddSeconds(299)), "not before five minutes");

        check(StaminaRules.Recover(data, now.AddMinutes(5)) && data.stamina == 1 && data.additionalStamina == 1, "one per five minutes");

        check(StaminaRules.Recover(data, now.AddMinutes(17)) && data.stamina == 3, "offline multiple intervals");

        check(StaminaRules.Remaining(data, now.AddMinutes(17)) == TimeSpan.FromMinutes(3), "partial interval preserved");

        check(StaminaRules.Recover(data, now.AddDays(10000)) && data.stamina == 5, "long absence capped");

        check(!StaminaRules.Recover(data, now.AddDays(10001)) && data.stamina == 5, "full has no extra recovery");

        DateTime consumeTime = now.AddDays(10002);

        check(StaminaRules.TryConsume(data, 1, consumeTime) && data.lastSavedDateTime == consumeTime, "full starts fresh timer");

        check(StaminaRules.Remaining(data, consumeTime) == TimeSpan.FromMinutes(5), "no banked full time");

        StaminaRules.TryConsume(data, 1, consumeTime.AddMinutes(2));

        check(data.lastSavedDateTime == consumeTime, "partial use retains progress");

        var legacy = new StaminaData { stamina = 100, lastSavedDateTime = now };
        StaminaRules.Normalize(legacy, now);

        check(legacy.stamina == 5 && legacy.additionalStamina == 0 && legacy.Total == 5, "legacy natural balance capped without bonus transfer");

        StaminaRules.Normalize(legacy, now);

        check(legacy.stamina == 5 && legacy.additionalStamina == 0, "migration is idempotent");

        legacy.stamina = 100;
        legacy.additionalStamina = 7;
        StaminaRules.Normalize(legacy, now);

        check(legacy.stamina == 5 && legacy.additionalStamina == 7, "existing bonus balance is independent of natural cap");

        var bad = new StaminaData { stamina = -2, additionalStamina = -7, lastSavedDateTime = now.AddDays(1) };
        StaminaRules.Normalize(bad, now);

        check(bad.Total == 0 && bad.lastSavedDateTime == now, "invalid values corrected");
        data = new StaminaData { stamina = 2, additionalStamina = 123, lastSavedDateTime = now.AddSeconds(-121) };
        string json = JsonUtility.ToJson(data);

        var restored = JsonUtility.FromJson<StaminaData>(json);

        check(restored.stamina == 2 && restored.additionalStamina == 123 && restored.lastSavedDateTime == data.lastSavedDateTime, "local JSON round trip");

        check(StaminaRules.Remaining(restored, now) == TimeSpan.FromSeconds(179), "local timer retained");

        var settings = Supabase.Postgrest.Client.SerializerSettings(new Supabase.Postgrest.ClientOptions());
        string cloudJson = "{\"stamina\":4,\"additional_stamina\":7,\"last_saved_datetime\":\"2026-11-15T12:00:00+00:00\"}";
        var cloud = Newtonsoft.Json.JsonConvert.DeserializeObject<_01.Scripts._12.Backend.PlayerStaminaModel>(cloudJson, settings);

        var reconnected = new StaminaData { stamina = cloud.Stamina, additionalStamina = cloud.AdditionalStamina,
            lastSavedDateTime = cloud.LastSavedDateTime };
        check(StaminaRules.Remaining(reconnected, now.AddMinutes(2)) == TimeSpan.FromMinutes(3), "cloud reconnect retains partial progress before normalization");

        StaminaRules.Recover(reconnected, now.AddMinutes(2));

        check(reconnected.lastSavedDateTime == now && reconnected.lastSavedDateTime.Kind == DateTimeKind.Utc,
            "cloud local timestamp normalized without resetting anchor");

        check(reconnected.stamina == 4 && reconnected.additionalStamina == 7 &&
            StaminaRules.Remaining(reconnected, now.AddMinutes(2)) == TimeSpan.FromMinutes(3), "restart after two minutes retains three minutes");

        check(StaminaRules.Recover(reconnected, now.AddMinutes(6)) && reconnected.stamina == 5,
            "restart after six minutes restores missing apple");

        var offline = new StaminaData { stamina = 0, additionalStamina = 9, lastSavedDateTime = now.ToLocalTime() };
        StaminaRules.Recover(offline, now.AddMinutes(17));

        check(offline.stamina == 3 && offline.additionalStamina == 9 && offline.lastSavedDateTime == now.AddMinutes(15),
            "local timestamp offline recovery retains completed intervals");

        check(StaminaRules.Remaining(offline, now.AddMinutes(17)) == TimeSpan.FromMinutes(3), "offline two-minute remainder retained");

        var offsetFree = new StaminaData { stamina = 4, lastSavedDateTime = DateTime.SpecifyKind(now, DateTimeKind.Unspecified) };
        StaminaRules.Recover(offsetFree, now.AddMinutes(2));

        check(offsetFree.lastSavedDateTime == now && offsetFree.lastSavedDateTime.Kind == DateTimeKind.Utc &&
            StaminaRules.Remaining(offsetFree, now.AddMinutes(2)) == TimeSpan.FromMinutes(3), "legacy offset-free server timestamp treated as UTC");

        var localInput = new StaminaData { stamina = 4, lastSavedDateTime = now.ToLocalTime() };
        StaminaRules.Recover(localInput, now.AddMinutes(2).ToLocalTime());

        check(StaminaRules.Remaining(localInput, now.AddMinutes(2).ToLocalTime()) == TimeSpan.FromMinutes(3), "local current time converted consistently");

        var cloudLocalSave = JsonUtility.FromJson<StaminaData>(JsonUtility.ToJson(offline));

        check(cloudLocalSave.lastSavedDateTime == now.AddMinutes(15) &&
            StaminaRules.Remaining(cloudLocalSave, now.AddMinutes(17)) == TimeSpan.FromMinutes(3), "cloud to local save reload preserves remainder");

        return "Stamina: " + checks + " deterministic checks passed. No player data or database was modified.";
    }
}

#endif
