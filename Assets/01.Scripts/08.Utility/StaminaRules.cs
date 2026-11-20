using _01.Scripts._00.Manager;
using System;

namespace _01.Scripts._08.Utility
{
    public static class StaminaRules
    {
        public const int MaxStamina = 5;
        public const int NormalEntryCost = 1;
        public const int HabitatEntryCost = 1;
        public static readonly TimeSpan RecoveryInterval = TimeSpan.FromMinutes(5);

        public static DateTime ToUtc(DateTime value) => value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        public static bool Normalize(StaminaData data, DateTime now)
        {
            int previous = data.stamina;
            int extra = data.additionalStamina;
            DateTime previousTime = data.lastSavedDateTime;

            now = ToUtc(now);
            data.lastSavedDateTime = ToUtc(data.lastSavedDateTime);
            data.additionalStamina = (int)Math.Min(int.MaxValue - MaxStamina, Math.Max(0L, data.additionalStamina));
            data.stamina = Math.Max(0, Math.Min(MaxStamina, data.stamina));

            if (data.lastSavedDateTime == DateTime.MinValue || data.lastSavedDateTime > now)
            {
                data.lastSavedDateTime = now;
            }

            return previous != data.stamina || extra != data.additionalStamina || previousTime != data.lastSavedDateTime;
        }

        public static bool Recover(StaminaData data, DateTime now)
        {
            now = ToUtc(now);
            bool changed = Normalize(data, now);

            if (data.stamina == MaxStamina)
            {
                return changed;
            }

            long count = (now - data.lastSavedDateTime).Ticks / RecoveryInterval.Ticks;

            if (count <= 0)
            {
                return changed;
            }

            int recovered = (int)Math.Min(MaxStamina - data.stamina, count);
            data.stamina += recovered;
            data.lastSavedDateTime = data.stamina == MaxStamina
                ? now : data.lastSavedDateTime.AddTicks(recovered * RecoveryInterval.Ticks);

            return true;
        }

        public static bool TryConsume(StaminaData data, int amount, DateTime now)
        {
            if (amount <= 0 || (long)data.stamina + data.additionalStamina < amount)
            {
                return false;
            }

            bool wasFull = data.stamina == MaxStamina;
            int natural = Math.Min(data.stamina, amount);
            data.stamina -= natural;
            data.additionalStamina -= amount - natural;

            if (wasFull && natural > 0)
            {
                data.lastSavedDateTime = ToUtc(now);
            }

            return true;
        }

        public static TimeSpan Remaining(StaminaData data, DateTime now)
        {
            if (data.stamina >= MaxStamina)
            {
                return TimeSpan.Zero;
            }

            TimeSpan elapsed = ToUtc(now) - ToUtc(data.lastSavedDateTime);

            if (elapsed < TimeSpan.Zero)
            {
                elapsed = TimeSpan.Zero;
            }

            TimeSpan remaining = RecoveryInterval - elapsed;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
