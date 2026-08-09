using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using LittleWitchNobetaAP.Utils.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LittleWitchNobetaAP.Patches;

public static class TrialKeysPatches
{
    private static readonly List<MultipleEventOpen> Openers = new();

    private static string _lastOpenerEnteredName = "";

    public static bool TrialKeysEnabled { get; set; }

    public static List<string> CollectedTrialKeys { get; } = new();

    private static bool CheckUseItem(ItemSystem.ItemType itemType)
    {
        if (itemType != ItemSystem.ItemType.SPMaxAdd) return true;

        Game.AppearEventPrompt("Trial keys can only be dropped, not used.");
        return false;
    }

    // Disable auto-open of trials
    [HarmonyPatch(typeof(MultipleEventOpen), nameof(MultipleEventOpen.InitData))]
    private static class MultipleEventOpenInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void MultipleEventOpenInitPostfix(ref MultipleEventOpen __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            // Only check in last stage
            if (Game.sceneManager.stageId != 7 || ArchipelagoClient.ServerData is not { } sessionData) return;

            // Skip if trial keys are not enabled
            if (!TrialKeysEnabled) return;

            if (__instance.name is not ("OpenLightRoomStart01" or "OpenLightRoomStart02" or "OpenLightRoomStart03"))
                return;

            // Reopen it if it has already been opened
            if (sessionData.OpenedTrials.Contains(__instance.name))
            {
                __instance.OpenEvent();

                return;
            }
            
            //Check for trial key
            switch (__instance.name)
            {
                case "OpenLightRoomStart01":
                    if (CollectedTrialKeys.Contains("Lava Ruins Trial Key"))
                    {
                        __instance.OpenEvent();
                        sessionData.OpenedTrials.Add(__instance.name);
                        return;
                    }

                    break;
                case "OpenLightRoomStart02":
                    if (CollectedTrialKeys.Contains("Underground Trial Key"))
                    {
                        __instance.OpenEvent();
                        sessionData.OpenedTrials.Add(__instance.name);
                        return;
                    }

                    break;
                case "OpenLightRoomStart03":
                    if (CollectedTrialKeys.Contains("Dark Tunnel Trial Key"))
                    {
                        __instance.OpenEvent();
                        sessionData.OpenedTrials.Add(__instance.name);
                        return;
                    }

                    break;
            }

            __instance.CheckPlayerEnter = false;

            // Make collider smaller so they don't overlap
            var extents = __instance.g_BC.extents - new Vector3(3f, 0f, 3f);
            __instance.g_BC.extents = extents;

            Openers.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    private static class EnterLoaderScenePostfix
    {
        [HarmonyPrefix]
        // ReSharper disable UnusedMember.Local
        private static void EnterLoaderScenePrefix()
            // ReSharper restore UnusedMember.Local
        {
            Openers.Clear();
        }
    }

    // Display a help message when near a trial
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    private static class HelpMessageUpdate
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void HelpMessageUpdatePostfix(WizardGirlManage __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            // Only check in last stage
            if (Game.sceneManager.stageId != 7 || ArchipelagoClient.ServerData is not { } sessionData) return;

            // Skip if trial keys are not enabled
            if (!TrialKeysEnabled) return;

            foreach (var opener in Openers.Where(opener => opener.name != _lastOpenerEnteredName
                                                           && opener.g_BC.Contains(__instance.transform.position)
                                                           && !sessionData.OpenedTrials.Contains(opener.name)))
            {
                _lastOpenerEnteredName = opener.name;

                Game.AppearEventPrompt("A trial key is needed to open the trial teleporter.");

                return;
            }
        }
    }
}