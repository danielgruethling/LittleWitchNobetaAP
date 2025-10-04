using HarmonyLib;
using Il2Cpp;

namespace LittleWitchNobetaAP.Patches;

public static class ArcaneDisabledPatches
{
    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Shoot))]
    private static class InputShoot
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool InputShootPrefix(PlayerInputController __instance, bool onHolding)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var wizardGirl = __instance.controller.wgm;

            if (wizardGirl.GetMagicType() != PlayerEffectPlay.Magic.Null ||
                wizardGirl.GameSave.stats.secretMagicLevel >= 1) return true;
            
            if (onHolding)
            {
                Game.AppearEventPrompt("You have yet to learn Arcane magic.");
            }
            
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Chant))]
    private static class InputChant
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool InputChantPrefix(PlayerInputController __instance)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var wizardGirl = __instance.controller.wgm;

            if (wizardGirl.GetMagicType() != PlayerEffectPlay.Magic.Null ||
                wizardGirl.GameSave.stats.secretMagicLevel >= 1) return true;
            
            Game.AppearEventPrompt("You have yet to learn Arcane magic.");
            return false;

        }
    }
}