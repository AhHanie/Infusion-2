namespace Infusion
{
    public enum KeyedData
    {
        SMART_INFUSION_CHANCE,
        ENERGY_SHIELD_RECHARGE_RATE,
        ENERGY_SHIELD_MAX_ENERGY,
        BARRAGE_BURSTCOUNT_MULTIPLIER,
        MINING_LOOT_CHANCE,
        MINING_LOOT_RARE_CHANCE,
        MINING_LOOT_ROLLS
    }

    public class KeyedDataHelper
    {
        public static string ConvertToString(KeyedData key)
        {
            switch (key)
            {
                case KeyedData.SMART_INFUSION_CHANCE:
                    return "chance";
                case KeyedData.ENERGY_SHIELD_RECHARGE_RATE:
                    return "EnergyShieldRechargeRate";
                case KeyedData.ENERGY_SHIELD_MAX_ENERGY:
                    return "EnergyShieldEnergyMax";
                case KeyedData.BARRAGE_BURSTCOUNT_MULTIPLIER:
                    return "BurstCountMultiplier";
                case KeyedData.MINING_LOOT_CHANCE:
                    return "chanceOres";
                case KeyedData.MINING_LOOT_RARE_CHANCE:
                    return "chanceRocks";
                case KeyedData.MINING_LOOT_ROLLS:
                    return "lootRolls";
            }
            return null;
        }
    }
}
