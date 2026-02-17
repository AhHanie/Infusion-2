using HarmonyLib;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class PawnKill_Necrosis
    {
        public class NecrosisPatchState
        {
            public bool shouldTrigger;
            public Apparel triggeringApparel;
        }

        public static void Prefix(Pawn __instance, ref NecrosisPatchState __state)
        {
            if (!__instance.RaceProps.Humanlike || HasGhoulHediff(__instance) || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            __state = new NecrosisPatchState();
            if (__instance.apparel.WornApparel == null || __instance.Dead)
            {
                return;
            }

            Apparel necrosisApparel = __instance.apparel.WornApparel.FirstOrDefault(HasNecrosisInfusion);
            if (necrosisApparel != null)
            {
                __state.shouldTrigger = true;
                __state.triggeringApparel = necrosisApparel;
            }
        }

        public static void Postfix(Pawn __instance, NecrosisPatchState __state)
        {
            if (!__instance.Dead || __state == null)
            {
                return;
            }

            var comp = Current.Game.GetComponent<Infusion.Comps.GameComponent_Infusion>();
            comp.QueueNecrosis(__instance.Corpse, __state.triggeringApparel, 600);
        }

        private static bool HasNecrosisInfusion(Apparel apparel)
        {
            CompInfusion comp = apparel.TryGetComp<CompInfusion>();
            if (comp == null)
            {
                return false;
            }

            return comp.ContainsTag(InfusionTags.NECROSIS);
        }

        public static bool TryResurrectPawn(Pawn pawn)
        {
            ResurrectionUtility.TryResurrect(pawn);
            return !pawn.Dead;
        }

        public static bool TryApplyGhoulMutation(Pawn pawn)
        {
            if (!pawn.health.hediffSet.HasHediff(ModHediffDefOf.Ghoul))
            {
                pawn.health.AddHediff(ModHediffDefOf.Ghoul);
            }

            return pawn.health.hediffSet.HasHediff(ModHediffDefOf.Ghoul);
        }

        public static bool HasGhoulHediff(Pawn pawn)
        {
            if (pawn.health.hediffSet == null)
            {
                return false;
            }

            return pawn.health.hediffSet.HasHediff(ModHediffDefOf.Ghoul);
        }

        public static void ResolveQueuedNecrosis(Pawn pawn, Apparel triggeringApparel)
        {
            Log.Message($"Necrosis resolved: {pawn?.Name} {triggeringApparel?.def}");
            if (!pawn.Dead || HasGhoulHediff(pawn))
            {
                return;
            }

            if (!TryResurrectPawn(pawn))
            {
                return;
            }

            if (!TryApplyGhoulMutation(pawn))
            {
                return;
            }

            Hediff rising = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Rising);
            if (rising != null)
            {
                pawn.health.RemoveHediff(rising);
            }

            PlayNecrosisFeedback(pawn);

            if (triggeringApparel != null && !triggeringApparel.Destroyed)
            {
                triggeringApparel.Destroy();
            }

            ModSoundDefOf.Pawn_Ghoul_Call.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld)));
        }

        public static void PlayNecrosisBuildup(Corpse corpse, ref Effecter riseEffecter)
        {
            Map map = corpse.MapHeld;
            IntVec3 pos = corpse.PositionHeld;

            TargetInfo target = new TargetInfo(pos, map);
            if (riseEffecter == null)
            {
                riseEffecter = EffecterDefOf.ShamblerRaise.Spawn();
            }

            riseEffecter?.Trigger(target, target);

            FleckMaker.ThrowShamblerParticles(corpse);
        }

        public static void CleanupNecrosisBuildup(ref Effecter riseEffecter)
        {
            riseEffecter?.Cleanup();
            riseEffecter = null;
        }

        public static void PlayNecrosisFeedback(Pawn pawn)
        {
            if (pawn.MapHeld == null)
            {
                return;
            }

            TargetInfo target = new TargetInfo(pawn.PositionHeld, pawn.MapHeld);

            Effecter releasing = ModEffecterDefOf.DeadlifeReleasing.Spawn();
            releasing.Trigger(target, target);
            releasing.Cleanup();

            Effecter raise = EffecterDefOf.ShamblerRaise.Spawn();
            raise.Trigger(target, target);
            raise.Cleanup();

            for (int i = 0; i < 8; i++)
            {
                FleckMaker.ThrowShamblerParticles(pawn);
            }

            MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "ShamblerRising".Translate(), 4f);
        }

    }
}
