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

    // Open trial on token drop
    [HarmonyPatch(typeof(PlayerItem), nameof(PlayerItem.DiscardItemSuccess))]
    private static class DiscardItem
    {
        [HarmonyPostfix]
        // ReSharper disable UnusedMember.Local
        private static void DiscardItemPostfix()
            // ReSharper restore UnusedMember.Local
        {
            // Only check in last stage
            if (Game.sceneManager.stageId != 7 || ArchipelagoClient.ServerData is not { } sessionData) return;

            // Skip if trial keys are not enabled
            if (!TrialKeysEnabled) return;

            var items = UnityUtils.FindComponentsByTypeForced<Item>();

            // Check if any token is in a trial open bound
            foreach (var item in items)
            {
                if (item.currentItemType != ItemSystem.ItemType.SPMaxAdd) continue;

                foreach (var eventOpen in Openers.Where(eventOpen =>
                             !eventOpen.g_AllOpen && eventOpen.g_BC.Contains(item.transform.position)))
                {
                    eventOpen.OpenEvent();
                    sessionData.OpenedTrials.Add(eventOpen.name);

                    Object.Destroy(item.gameObject);

                    return;
                }
            }
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

                Game.AppearEventPrompt("Drop a trial key to open the trial teleporter.");

                return;
            }
        }
    }

    // Disable usage of tokens (can only drop)
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnUseItemHotKeyDown))]
    private static class UseItem
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool UseItemPrefix(PlayerController __instance, int index)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            return CheckUseItem(__instance.g_Item.GetSelectItemType(index));
        }

        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool UseItemPrefix(PlayerController __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            return CheckUseItem(__instance.g_Item.GetSelectItemType(Game.GetItemSelectPos()));
        }
    }

    // Patch display of token item name and description
    [HarmonyPatch(typeof(ItemSystem), nameof(ItemSystem.GetItemHelp))]
    private static class GetItemHelp
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool GetItemHelpPrefix(ref string __result, ItemSystem.ItemType Type)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (Type != ItemSystem.ItemType.SPMaxAdd) return true;

            __result = "Cannot be used.\nDrop on a trial path to unlock it.";
            return false;
        }
    }

    [HarmonyPatch(typeof(ItemSystem), nameof(ItemSystem.GetItemName))]
    private static class GetItemName
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool GetItemNamePrefix(ref string __result, ItemSystem.ItemType Type)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (Type != ItemSystem.ItemType.SPMaxAdd) return true;

            __result = "Trial Key";
            return false;
        }
    }
}