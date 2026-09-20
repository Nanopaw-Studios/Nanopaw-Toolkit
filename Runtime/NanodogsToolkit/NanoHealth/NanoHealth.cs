using UnityEngine;

namespace Nanodogs.API.NanoHealth
{
    /// <summary>
    /// NanoHealth is a class for managing health-related functionality.
    /// health systems, damage handling, and healing mechanics.
    /// </summary>

    public class NanoHealth
    {
        /// <summary>
        /// returns health percentage (0 to 1)
        /// </summary>
        /// <returns></returns>
        public static float GetHealthPercentage(NanoHealthData healthData)
        {
            if (healthData == null || healthData.maxHealth <= 0) return 0f;
            return (float)healthData.currentHealth / healthData.maxHealth;
        }

        /// <summary>
        /// heals the player by the specified amount
        /// </summary>
        /// <param name="healthData"></param>
        /// <param name="amount"></param>
        public static void Heal(NanoHealthData healthData, int amount)
        {
            if (healthData == null || !healthData.isAlive) return;
            healthData.currentHealth += amount;
            if (healthData.currentHealth > healthData.maxHealth)
            {
                healthData.currentHealth = healthData.maxHealth;
            }
        }

        /// <summary>
        /// damages the player by the specified amount
        /// </summary>
        /// <param name="healthData"></param>
        /// <param name="amount"></param>
        public static void TakeDamage(NanoHealthData healthData, int amount)
        {
            if (healthData == null || !healthData.isAlive) return;
            healthData.currentHealth -= amount;
            if (healthData.currentHealth <= 0)
            {
                healthData.currentHealth = 0;
                healthData.isAlive = false;
                healthData.state = NanoHealthData.NanoHealthState.Dead;
            }
            else if (healthData.currentHealth < healthData.maxHealth * 0.5f)
            {
                healthData.state = NanoHealthData.NanoHealthState.Critical;
            }
            else
            {
                healthData.state = NanoHealthData.NanoHealthState.Injured;
            }
        }

        /// <summary>
        /// regenerates health over time based on the healthRegenRate and deltaTime
        /// </summary>
        /// <param name="healthData"></param>
        /// <param name="deltaTime"></param>
        public static void RegenerateHealth(NanoHealthData healthData, float deltaTime)
        {
            if (healthData == null || !healthData.isAlive) return;
            if (healthData.healthRegenCooldown > 0)
            {
                healthData.healthRegenCooldown -= deltaTime;
                return;
            }
            if (healthData.currentHealth < healthData.maxHealth)
            {
                healthData.currentHealth += Mathf.RoundToInt(healthData.healthRegenRate * deltaTime);
                if (healthData.currentHealth > healthData.maxHealth)
                {
                    healthData.currentHealth = healthData.maxHealth;
                }
            }
        }
    }

    /// <summary>
    /// health data structure
    /// </summary>
    public class NanoHealthData
    {
        public int currentHealth;
        public int maxHealth;
        public bool isAlive;
        public float healthRegenRate;
        public float healthRegenCooldown;
        public NanoHealthState state;

        /// <summary>
        /// the state of how damaged the player is
        /// Healthy = 100 health
        /// Injured = 50-99 health
        /// Critical = 0-49 health
        /// Dead = 0 health
        /// </summary>
        public enum NanoHealthState
        {
            Healthy, // 100 health
            Injured, // 50-99 health
            Critical, // 0-49 health
            Dead // 0 health
        }
    }
}
