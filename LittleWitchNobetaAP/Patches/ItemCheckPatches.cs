using System.Collections.Immutable;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using MelonLoader;

namespace LittleWitchNobetaAP.Patches;

public static class ItemCheckPatches
{
    [HarmonyPatch(typeof(TreasureBox), nameof(TreasureBox.Init))]
    private static class TreasureBoxInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void TreasureBoxInitPostfix(ref TreasureBox __instance)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            // remove chest contents
            __instance.ItemType = ItemSystem.ItemType.Null;
        }
    }

    [HarmonyPatch(typeof(TreasureBox), nameof(TreasureBox.SetOpen))]
    private static class TreasureBoxSetOpen
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void TreasureBoxSetOpenPostfix(TreasureBox __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return;
            
            // Special exception because TreasureBox instance name TreasureBox_Room03 exists twice
            if (Game.sceneManager.stageId == 3 && __instance.name == "TreasureBox_Room03")
            {
                ArchipelagoClient.Session.Locations.CompleteLocationChecks(
                    ArchipelagoClient.Session.Locations.GetLocationIdFromName(
                        "Little Witch Nobeta", "Underground - Chest in alcove before falling rocks"));
            }
            else
            {
                ArchipelagoClient.Session.Locations.CompleteLocationChecks(
                    ArchipelagoClient.Session.Locations.GetLocationIdFromName("Little Witch Nobeta",
                        ArchipelagoData.GameLocationToDescriptiveLocation(__instance.name)));
            }
        }
    }

    [HarmonyPatch(typeof(CatEvent), nameof(CatEvent.OpenEvent))]
    private static class CatEventOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool CatEventOpenEventPrefix(CatEvent __instance)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (__instance.name is "04_CatAbsorbSkill" or "04_SkillBookAgain"
                && ArchipelagoClient.IsAuthenticated
                && ArchipelagoClient.Session is not null)
            {
                ArchipelagoClient.Session.Locations.CompleteLocationChecks(
                    ArchipelagoClient.Session.Locations.GetLocationIdFromName("Little Witch Nobeta",
                        ArchipelagoData.GameLocationToDescriptiveLocation(__instance.name)));
            }

            return false;
        }
    }

    // Disable thunder from Vanessa
    [HarmonyPatch(typeof(SceneEvent), nameof(SceneEvent.InitData))]
    private static class SceneEventInitData
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void LoadScriptInitPostfix(SceneEvent __instance, SceneEventManager SEM)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (__instance.name is not "LoadScriptRoomBossEnd"
                || !ArchipelagoClient.IsAuthenticated
                || ArchipelagoClient.Session is null) return;
            
            var loadScript = __instance.Cast<LoadScript>();
            var newEvents = loadScript.Event.SkipLast(1).ToArray();
            loadScript.Event = newEvents;
                
            ArchipelagoClient.Session.Locations.CompleteLocationChecks(
                ArchipelagoClient.Session.Locations.GetLocationIdFromName(
                    "Little Witch Nobeta", "Spirit Realm - Thunder spell from Vanessa V2"));
            ArchipelagoClient.Session.Locations.CompleteLocationChecks(
                ArchipelagoClient.Session.Locations.GetLocationIdFromName(
                    "Little Witch Nobeta", "Spirit Realm - 101. Proud King's Crafted Soul Shard from Vanessa V2"));
        }
    }
    
    // Item pickup
    [HarmonyPatch(typeof(PlayerItem), nameof(PlayerItem.PickUp))]
    private static class PlayerItemPickUp
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool ItemPickUpPrefix(PlayerItem __instance) 
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            Melon<LwnApMod>.Logger.Msg($"Picked up item {__instance.GetItemData().name}");
            if (!__instance.GetItemData().name.Contains("Item_Property")) return true;
            Melon<LwnApMod>.Logger.Msg($"Item property ID:  {__instance.GetItemData().GetPropertyID()}");
            
            if (ArchipelagoClient.IsAuthenticated && ArchipelagoClient.Session is not null)
            {
                var loreItems = 
                    from item in ArchipelagoData.Items
                    where item.Value == "Lore"
                    orderby int.Parse(new string(item.Key.TakeWhile(char.IsDigit).ToArray())) 
                    select item.Key;
                
                var loreItem = loreItems.ElementAt(__instance.GetItemData().GetPropertyID());

                if (loreItem is not null)
                {
                    var locationName = 
                        (from item in ArchipelagoData.Locations.Keys
                            where item.Contains(loreItem)
                            select item).FirstOrDefault();

                    if (locationName is not null)
                    {
                        var locationId = ArchipelagoData.GetLocationIdByName(locationName);
                        ArchipelagoClient.Session.Locations.CompleteLocationChecks(locationId);
                    }
                    else
                    {
                        Melon<LwnApMod>.Logger.Error($"Did not find a location for lore item: {loreItem}");
                    }
                }
                else
                {
                    Melon<LwnApMod>.Logger.Error($"Did not find a lore item with id: {__instance.GetItemData().GetPropertyID() + 1}");
                }
            }
            
            MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
            UnityEngine.Object.Destroy(__instance.GetItemData().gameObject)));

            return false;
        }
    }
    
    // Bosses
    [HarmonyPatch(typeof(NPCManage), nameof(NPCManage.Hit))]
    private static class NpcManageHit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void NpcManageHitPostfix(NPCManage __instance)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!__instance.GetIsDeath()) return;
            
            var descriptiveLocation = ArchipelagoData.GameLocationToDescriptiveLocation(__instance.name);
            Melon<LwnApMod>.Logger.Msg($"Killed enemy: {__instance.name}");
            if (descriptiveLocation != string.Empty)
            {
                Melon<LwnApMod>.Logger.Msg($"Sending enemy location: {descriptiveLocation}");
                ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                    ArchipelagoData.GetLocationIdByName(descriptiveLocation));
            }

            switch (__instance.name)
            {
                case "Boss_Act01_Plus":
                    ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                        ArchipelagoData.GetLocationIdByName("Secret Passage - Teleport from Enraged Armor"));
                    break;
                case "Boss_Level04":
                    ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                        ArchipelagoData.GetLocationIdByName("Dark Tunnel - 100. King's Final Honor from Vanessa"));
                    ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                        ArchipelagoData.GetLocationIdByName("Dark Tunnel - 78. Ancient Throne Rune from Vanessa"));
                    ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                        ArchipelagoData.GetLocationIdByName("Dark Tunnel - 77. The Throne from Vanessa"));
                    break;
                case "Boss_Level06":
                {
                    var statusUpdatePacket = new StatusUpdatePacket
                    {
                        Status = ArchipelagoClientState.ClientGoal
                    };
                
                    ArchipelagoClient.Session?.Locations.CompleteLocationChecks(
                        ArchipelagoData.GetLocationIdByName("Abyss - 102. Lost Maiden's Crafted Soul Shard from Nonota"));
                    ArchipelagoClient.Session?.Socket.SendPacket(statusUpdatePacket);
                    break;
                }
            }
        }
    }
}