using HarmonyLib;
using Infusion.Comps;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
    public static class TradeDeal_Allure
    {
        public static void Postfix(bool __result, ref bool actuallyTraded)
        {
            if (!__result || !actuallyTraded)
            {
                return;
            }
            GameComponent_Infusion comp = Current.Game.GetComponent<GameComponent_Infusion>();
            AllureHelper.TryTriggerAfterTrade(TradeSession.playerNegotiator, TradeSession.trader);
        }
    }
}
