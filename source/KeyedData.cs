namespace Infusion
{
    public enum KeyedData
    {
        SMART_INFUSION_CHANCE,
        ENERGY_SHIELD_RECHARGE_RATE,
        ENERGY_SHIELD_MAX_ENERGY,
        BARRAGE_BURSTCOUNT_MULTIPLIER
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
            }
            return null;
        }
    }
}
