namespace Infusion
{
    public enum KeyedData
    {
        SMART_INFUSION_CHANCE = 1,
        ENERGY_SHIELD_RECHARGE_RATE,
        ENERGY_SHIELD_MAX_ENERGY
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
            }
            return null;
        }
    }
}
