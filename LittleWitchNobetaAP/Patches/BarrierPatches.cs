using Archipelago.MultiClient.Net;
using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using MelonLoader;

namespace LittleWitchNobetaAP.Patches;

public static class BarrierPatches
{
    // Used to prevent sending checks for mappings when the trigger and action
    // have the same path when processing already received items
    private static bool _isExecutingBarrierActions;

    // Try to execute barrier actions associated with an item name
    public static void OpenBarrierByItemName(string itemName)
    {
        Melon<LwnApMod>.Logger.Msg($"Attempting to handle barrier with item name: {itemName}");
        var barrier = ArchipelagoData.Barriers.ByItemName[itemName].FirstOrDefault();
        if (barrier is null)
        {
            Melon<LwnApMod>.Logger.Error($"Did not find barrier: {itemName}");
        }
        else
        {
            if (!Singletons.SceneManager) return;
            if ((int)barrier.StageId != Singletons.SceneManager.stageId) return;
            MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
            {
                _isExecutingBarrierActions = true;
                foreach (var action in barrier.Actions
                             .Where(barrierData => (int)barrierData.StageId == Singletons.SceneManager.stageId))
                {
                    if (action.DoNotExecuteOnItem)
                    {
                        Melon<LwnApMod>.Logger.Msg($"Action {action.Path} was skipped.");
                        continue;
                    }

                    action.Execute();
                }

                _isExecutingBarrierActions = false;

                Melon<LwnApMod>.Logger.Msg($"Barrier was opened: {itemName}");
            }));
        }
    }

    // Execute all barrier actions that take place in the current stage for barrier
    // items owned by the player.
    public static void ExecuteAllStageBarrierActions()
    {
        if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
        {
            return;
        }

        if (!Singletons.SceneManager) return;
        
        Melon<LwnApMod>.Logger.Msg($"Running ExecuteAllStageBarrierActions.");

        foreach (var barrier in ArchipelagoData.Barriers.BarrierMappings)
        {
            var isAlwaysOpenGate = barrier.Type == BarrierType.MetalGate &&
                                   ArchipelagoClient.ServerData.Settings?.ShortcutGateBehaviour ==
                                   ArchipelagoSettings.ShortcutGateBehaviourType.AlwaysOpen;
            var isAlwaysOpenMagicPuzzle = barrier.Type == BarrierType.MagicPuzzle &&
                                          ArchipelagoClient.ServerData.Settings?.BarrierBehaviour ==
                                          ArchipelagoSettings.MagicPuzzleGateBehaviourType.AlwaysOpen;
            var hasItem = ArchipelagoClient.Session.Items.AllItemsReceived
                .Any(item => item.ItemName == barrier.ItemName);
            var isRandomizedGate = ArchipelagoClient.ServerData.Settings?.ShortcutGateBehaviour ==
                ArchipelagoSettings.ShortcutGateBehaviourType.Randomized && barrier.Type == BarrierType.MetalGate;
            var isRandomizedMagicPuzzle = ArchipelagoClient.ServerData.Settings?.BarrierBehaviour ==
                ArchipelagoSettings.MagicPuzzleGateBehaviourType.Randomized && barrier.Type == BarrierType.MagicPuzzle;
            
            if (isAlwaysOpenGate || isAlwaysOpenMagicPuzzle || (hasItem && isRandomizedGate) ||
                (hasItem && isRandomizedMagicPuzzle))
            {
                Melon<LwnApMod>.Logger.Msg($"Running barrier actions for barrier with item {barrier.ItemName}.");

                MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
                {
                    _isExecutingBarrierActions = true;
                    foreach (var action in barrier.Actions
                                 .Where(barrierData => (int)barrierData.StageId == Singletons.SceneManager.stageId))
                    {
                        if (action.DoNotExecuteOnItem) continue;
                        action.Execute();
                    }
                    _isExecutingBarrierActions = false;
                }));
            }
        }

        var onStageLoadActions =
            ArchipelagoData.Barriers.OnStageLoadActionsByStageId[Singletons.SceneManager.stageId];
        foreach (var barrier in onStageLoadActions)
        {
            if (barrier.ItemName is not null)
            {
                var hasItem = ArchipelagoClient.Session.Items.AllItemsReceived
                    .Any(item => item.ItemName == barrier.ItemName);
                if (hasItem) continue;
            }

            MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
            {
                _isExecutingBarrierActions = true;

                foreach (var action in barrier.Actions)
                {
                    action.Execute();
                    Melon<LwnApMod>.Logger.Msg(
                        barrier.ItemName is null
                            ? $"Barrier action {action.Path} executed on stage load."
                            : $"Barrier action {action.Path} executed as player lacks AP item {barrier.ItemName}.");
                }
                _isExecutingBarrierActions = false;
            }));
        }

    }

    private static void TryTriggerBarrierCheck(ArchipelagoSession session, string path)
    {
        var barrier = ArchipelagoData.Barriers.ByTriggerPath[path]
            .FirstOrDefault(barrier => (int)barrier.StageId == Singletons.SceneManager?.stageId);
        if (barrier is null) return;
        switch (barrier.Type)
        {
            case BarrierType.MetalGate when
                (ArchipelagoClient.ServerData.Settings?.ShortcutGateBehaviour !=
                 ArchipelagoSettings.ShortcutGateBehaviourType.Randomized):
            case BarrierType.MagicPuzzle when
                (ArchipelagoClient.ServerData.Settings?.BarrierBehaviour !=
                 ArchipelagoSettings.MagicPuzzleGateBehaviourType.Randomized):
                return;
        }

        var locationId = ArchipelagoData.GetLocationIdByName(barrier.LocationName);
        Melon<LwnApMod>.Logger.Msg(
            $"AP Location: {barrier.LocationName} ({locationId}) at trigger {path} checked.");
        session.Locations.CompleteLocationChecks(locationId);
    }

    private static bool ShouldAllowEvent(ArchipelagoSession session, string path)
    {
        var stageId = Singletons.SceneManager.stageId;
        if (ArchipelagoData.Barriers.ByActionPath.TryGetValue(stageId, out var stageLookup))
        {
            var barrier = stageLookup[path].FirstOrDefault();
            if (barrier is null)
            {
                Melon<LwnApMod>.Logger.Msg($"Failed to find any AP items for {path}.");
            }
            else
            {
                switch (barrier.Type)
                {
                    case BarrierType.MetalGate when
                        (ArchipelagoClient.ServerData.Settings?.ShortcutGateBehaviour !=
                         ArchipelagoSettings.ShortcutGateBehaviourType.Randomized):
                    case BarrierType.MagicPuzzle when
                        (ArchipelagoClient.ServerData.Settings?.BarrierBehaviour !=
                         ArchipelagoSettings.MagicPuzzleGateBehaviourType.Randomized):
                        return true;
                }

                var hasBarrierItem = session.Items.AllItemsReceived
                    .Any(item => item.ItemName == barrier.ItemName);
                Melon<LwnApMod>.Logger.Msg(hasBarrierItem
                    ? $"Event {path} allowed with AP item {barrier.ItemName}."
                    : $"Event {path} blocked due to lack of AP item {barrier.ItemName}.");
                return hasBarrierItem;
            }
        }
        else
        {
            Melon<LwnApMod>.Logger.Msg($"Failed to find stage id {stageId} when looking up {path}.");
        }

        return true;
    }

    // Handle LoadScript triggers, currently used for detecting barrier events.
    // As scripts may include other side effects, barrier open events should be blocked in its own patched call
    // if the barrier item is not unlocked.
    [HarmonyPatch(typeof(LoadScript), nameof(LoadScript.OpenEvent))]
    private static class LoadScriptOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void LoadScriptOpenEventPrefix(LoadScript __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            if (!Singletons.SceneManager) return;
            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Load Script detected: {path}.");
            TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(DoorSwitch), nameof(DoorSwitch.SwitchOn))]
    private static class DoorSwitchSwitchOnEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void DoorSwitchSwitchOnEventPrefix(DoorSwitch __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            if (!Singletons.SceneManager) return;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Door Switch detected: {path}.");
            TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(SwitchDevice), nameof(SwitchDevice.ReleaseDevice))]
    private static class SwitchDeviceReleaseDevice
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void SwitchDeviceReleaseDevicePrefix(SwitchDevice __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            if (!Singletons.SceneManager) return;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"SwitchDevice release event detected with path {path}.");

            TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(OpenScriptEvent), nameof(OpenScriptEvent.OpenEvent))]
    private static class OpenScriptEventOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void OpenScriptEventOpenEventPrefix(OpenScriptEvent __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            if (!Singletons.SceneManager) return;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
        }
    }
    
    [HarmonyPatch(typeof(EnemyEvent), nameof(EnemyEvent.OpenEvent))]
    private static class EnemyEventOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void EnemyEventOpenEventPrefix(OpenScriptEvent __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            if (!Singletons.SceneManager) return;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(MagicWall), nameof(MagicWall.ReleaseEvent))]
    private static class MagicWallReleaseEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool MagicWallReleaseEventPrefix(MagicWall __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            // Allows vanilla behavior is AP is disconnected
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            if (!Singletons.SceneManager) return true;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Magic Wall Release detected with path {path}.");
            // For some events the easiest trigger event is the wall release itself. However,
            // if the player has a barrier item but not the check, executing barrier actions
            // on scene load would unintentionally give player checks for free without this block
            if (!_isExecutingBarrierActions)
            {
                TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
            }

            return ShouldAllowEvent(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(OpenDoor), nameof(OpenDoor.OpenEvent))]
    private static class OpenDoorOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool OpenDoorOpenEventPrefix(OpenDoor __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            if (!Singletons.SceneManager) return true;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"OpenDoor event detected with path {path}.");

            if (!_isExecutingBarrierActions)
            {
                TryTriggerBarrierCheck(ArchipelagoClient.Session, path);
            }

            return ShouldAllowEvent(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(MoveFloor), nameof(MoveFloor.OpenEvent))]
    private static class MoveFloorOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool MoveFloorOpenEventPrefix(MoveFloor __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            if (!Singletons.SceneManager) return true;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"MoveFloor event detected with path {path}.");

            return ShouldAllowEvent(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(Elevator), nameof(Elevator.OpenEvent))]
    private static class ElevatorOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool ElevatorOpenEventPrefix(Elevator __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            if (!Singletons.SceneManager) return true;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Elevator event detected with path {path}.");

            return ShouldAllowEvent(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(MoveObject), nameof(MoveObject.OpenEvent))]
    private static class MoveObjectOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool MoveObjectOpenEventPrefix(MoveFloor __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            if (!Singletons.SceneManager) return true;

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"MoveObject event detected with path {path}.");

            return ShouldAllowEvent(ArchipelagoClient.Session, path);
        }
    }

    [HarmonyPatch(typeof(DamageObject), nameof(DamageObject.SetDamage))]
    private static class DamageObjectSetDamage
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool DamageObjectSetDamagePrefix(DamageObject __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return true;
            }

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Damage object {path}.");
            return true;
        }
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Init))]
    private static class OnSceneInit
    {
        [HarmonyPostfix]
        // ReSharper disable UnusedMember.Local
        private static void DisableDoorFlags(SceneManager __instance)
            // ReSharper restore UnusedMember.Local
        {
            if (Singletons.GameSave is null) return;
            // Door flags must be reset or else doors will be automatically open on
            // scene load without going through the OpenDoor.ReleaseEvent call.
            // TODO: Double check if additional flags need to be reset

            Singletons.GameSave.flags.stage01OpenDoor01 = false;
            Singletons.GameSave.flags.stage01OpenDoor02 = false;
            Singletons.GameSave.flags.stage01OpenDoor03 = false;
            Singletons.GameSave.flags.stage01Room08Door = false;
            Singletons.GameSave.flags.stage02OpenDoor = false;
            Singletons.GameSave.flags.stage02L03BackDoor = false;
            Singletons.GameSave.flags.stage03Room01DoorL = false;
            Singletons.GameSave.flags.stage03Room01DoorR = false;
            Singletons.GameSave.flags.stage03Room08ToBack = false;
            Singletons.GameSave.flags.stage03Stage04BackDoor = false;
            Singletons.GameSave.flags.stage04Room02DoorSwitch = false;
            Singletons.GameSave.flags.stage04Room04CrystalBall = false;

            Singletons.GameSave.flags.stage05Room02DoorSwitch = false;
            Singletons.GameSave.flags.stage05Room03To04DoorSwitch = false;
            Singletons.GameSave.flags.stage05Room04DoorSwitch = false;

            Singletons.GameSave.flags.stage06Act02Alarm = false;

            Melon<LwnApMod>.Logger.Msg($"Reset door related flags.");

            // Reset all barrier related flags. Flags cause barriers to release on stage load
            // without calling the typical method like ReleaseEvent, meaning logic could break
            // (e.g. player breaks switch and reloads stage, causing barrier to be released
            // even though they didn't get the corresponding barrier item)

            Singletons.GameSave.flags.stage01MeetCat = false;
            Singletons.GameSave.flags.stage01Room03 = false;
            Singletons.GameSave.flags.stage01Room04 = false;
            Singletons.GameSave.flags.stage01Room06To07 = false;
            Singletons.GameSave.flags.stage01Room07Barrier = false;
            Singletons.GameSave.flags.stage01Room08Door = false;
            Singletons.GameSave.flags.stage01Room09Barrier = false;

            Singletons.GameSave.flags.stage02Room06 = false;
            Singletons.GameSave.flags.stage02Room08 = false;
            Singletons.GameSave.flags.stage02Room09 = false;

            Singletons.GameSave.flags.stage03Room02 = false;
            Singletons.GameSave.flags.stage03Room04Event02 = false;
            Singletons.GameSave.flags.stage03Room06 = false;

            // Reset arcane barrier and platform shortcuts
            Singletons.GameSave.flags.stage05Room04_01 = false;
            Singletons.GameSave.flags.stage05Room04_02 = false;
            // Reset Seal fights as it releases the barriers
            Singletons.GameSave.flags.stage05Room05 = false;
            Singletons.GameSave.flags.stage05Room06 = false;
            // Reset lift, fire puzzle, and top barrier
            Singletons.GameSave.flags.stage05Room07_01 = false;
            Singletons.GameSave.flags.stage05Room07_02 = false;
            Singletons.GameSave.flags.stage05Room07_03 = false;

            // Prevent giant maid barrier from releasing
            Singletons.GameSave.flags.stage06Act02Clear = false;
            // Prevent underground trial barrier from releasing
            Singletons.GameSave.flags.stage06Act03Clear = false;
            // Prevent lava ruins lava floor from moving early
            Singletons.GameSave.flags.stage06Act04Siwtch = false;
            // Reset Abyss trial switches, otherwise teleporter will enable on init
            Singletons.GameSave.flags.stage06RoomCentralAct03 = false;
            Singletons.GameSave.flags.stage06RoomCentralAct04 = false;
            Singletons.GameSave.flags.stage06RoomCentralAct05 = false;

            Melon<LwnApMod>.Logger.Msg($"Reset magic puzzle gate flags.");
        }
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class OnSceneInitComplete
    {
        [HarmonyPostfix]
        // ReSharper disable UnusedMember.Local
        private static void OnSceneInitCompletePostfix()
            // ReSharper restore UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            ExecuteAllStageBarrierActions();
        }
    }

    [HarmonyPatch(typeof(AreaCheck), nameof(AreaCheck.OpenEvent))]
    private static class AreaCheckOpenEvent
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void AreaCheckOpenEventPostfix(AreaCheck __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            Melon<LwnApMod>.Logger.Msg($"Player has entered area {path}.");
        }
    }
}