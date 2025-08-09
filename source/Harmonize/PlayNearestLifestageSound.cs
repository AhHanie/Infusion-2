using HarmonyLib;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(LifeStageUtility), "PlayNearestLifestageSound")]
    public static class PlayNearestLifestageSound
    {
        public static bool Prefix()
        {
            if (Locks.LifeStageUtiltiyAudioLock)
            {
                return false;
            }
            return true;
        }
    }
}


