using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LittleWitchNobetaAP.Patches;

// Adds custom warp points to stages to fix softlocks or provide more interesting
// logic generation if enabled
public static class CustomWarpPatches
{
    private static void AddExtraAssetsToShrine()
    {
        if (Singletons.WizardGirl is null) return;
        if (!Singletons.WizardGirl.GameSave.flags.stage01Cleared) return;

        // Move collapsed wall to boss room area, allowing access to barrels at bottom of stairs in case
        // barrelsanity is enabled
        var wallCollapse = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room06/Other/L01CleraOpenEvent");
        if (wallCollapse is not null)
        {
            wallCollapse.transform.position = new Vector3(-84f, -41f, -235f);
            wallCollapse.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            wallCollapse.transform.localScale = new Vector3(1.4f, 2f, 1.4f);
        }

        // Create door for exit to underground start. Note that this is placed far in the hall because the exit point
        // has to be in a room area check trigger in order to render the room on load.
        var archDoorTemplate = UnityUtils.FindObjectByPath("/Scene/Room06_Save/StaticObject/Wall/Door_01_Setup");
        if (archDoorTemplate is not null)
        {
            var clonedDoor = Object.Instantiate(archDoorTemplate);
            if (clonedDoor is not null)
            {
                clonedDoor.name = "ToUndergroundRoom01Door";
                clonedDoor.transform.position = new Vector3(-80f, -16f, -270f);
                clonedDoor.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                clonedDoor.transform.localScale = new Vector3(0.85f, 1.11f, 1f);
                var leftDoor = clonedDoor.transform.Find("Door_L_01")?.GetComponent<MeshRenderer>();
                if (leftDoor is not null) leftDoor.enabled = true;
                var rightDoor = clonedDoor.transform.Find("Door_R_01")?.GetComponent<MeshRenderer>();
                if (rightDoor is not null) rightDoor.enabled = true;
            }
        }
    }

    private static void AddCustomWarpToShrine()
    {
        var saveSystem = UnityUtils.FindObjectByPath("/SaveSystem")?.GetComponent<SaveSystem>();
        if (saveSystem is null)
        {
            Melon<LwnApMod>.Logger.Msg("Unable to find SaveSystem component");
            return;
        }

        var template = UnityUtils.FindObjectByPath("/Scene/Room06_Save/Special/SavePointTransfer");
        var savePointObjClone = Object.Instantiate(template);
        if (savePointObjClone is null)
        {
            Melon<LwnApMod>.Logger.Msg("Failed to clone template object");
            return;
        }

        savePointObjClone.name = "ToUndergroundRoom01WarpPoint";
        var savePoint = savePointObjClone.GetComponentInChildren<SavePoint>(true);
        savePoint.name = "CustomWarpPoint";
        savePoint.TransferLevelNumber = 3;
        savePoint.TransferSavePointNumber = 7;

        // Place new warp point in hallway to boss room, inside room area check volume to ensure texture load
        // This is placed regardless of whether the boss has been defeated yet, in order to give flexibility
        // for having the first boss appearing as an item
        savePointObjClone.transform.position = new Vector3(-81.5f, -16f, -270f);
        savePointObjClone.transform.rotation = Quaternion.Euler(0f, 270f, 0f);

        var allSavePoints = saveSystem.AllSavePoint;
        if (allSavePoints is null)
        {
            Melon<LwnApMod>.Logger.Msg("AllSavePoints was null");
            return;
        }

        var newArray = new Il2CppReferenceArray<SavePoint?>(allSavePoints.Length + 1);
        for (var i = 0; i < allSavePoints.Length; i++)
        {
            newArray[i] = allSavePoints[i];
        }

        newArray[allSavePoints.Length] = savePoint;
        saveSystem.AllSavePoint = newArray;

        Melon<LwnApMod>.Logger.Msg("Injected custom save point into save system");
    }

    private static void AddExtraAssetsToUnderground()
    {
        var archedDoorTemplate =
            UnityUtils.FindObjectByPath("/Scene/Room07_Save/StaticObject/Wall/Arch_Door_03_Preset");
        if (archedDoorTemplate is not null)
        {
            var clonedDoor = Object.Instantiate(archedDoorTemplate);
            if (clonedDoor is not null)
            {
                clonedDoor.transform.position = new Vector3(-8f, 0f, -16.9f);
                clonedDoor.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                clonedDoor.transform.rotation = Quaternion.Euler(0f, 180f, 0);
                clonedDoor.name = "ToShrineRoom06StaticDoor";
                var renderer = clonedDoor.GetComponent<MeshRenderer>();
                // Sort of arbitrary but use lightmap that looks okay enough
                renderer.lightmapIndex = 3;
                renderer.lightmapScaleOffset = new Vector4(0.0725f, 0.08f, 0.6f, 0.9f);
                renderer.enabled = true;
                var leftDoor = clonedDoor.transform.Find("Door_Left")?.GetComponent<MeshRenderer>();
                if (leftDoor is not null) leftDoor.enabled = true;
                var rightDoor = clonedDoor.transform.Find("Door_Right")?.GetComponent<MeshRenderer>();
                if (rightDoor is not null) rightDoor.enabled = true;
            }
        }

        // Disable mushroom asset around where the door is to make more room
        var mushroomInFrontOfDoor =
            UnityUtils.FindObjectByPath("/Scene/Room01/StaticObject/Scale_0.4/Obj03/AC_Fungus03");
        if (mushroomInFrontOfDoor is not null)
        {
            mushroomInFrontOfDoor.active = false;
        }
    }

    private static void AddCustomWarpToUnderground()
    {
        var saveSystem = UnityUtils.FindObjectByPath("/SaveSystem")?.GetComponent<SaveSystem>();
        if (saveSystem is null)
        {
            Melon<LwnApMod>.Logger.Msg("Unable to find SaveSystem component");
            return;
        }

        var template =
            UnityUtils.FindObjectByPath("/Scene/Room01/Special/SavePointTransfer");
        var savePointObjClone = Object.Instantiate(template);
        if (savePointObjClone is null)
        {
            Melon<LwnApMod>.Logger.Msg("Failed to clone template object");
            return;
        }

        savePointObjClone.name = "ToShrineRoom06WarpPoint";
        var savePoint = savePointObjClone.GetComponentInChildren<SavePoint>(true);
        savePoint.name = "CustomWarpPoint";
        savePoint.TransferLevelNumber = 2;
        savePoint.TransferSavePointNumber = 6;
        // Add box collider to clone to make the exit prompt work
        savePoint.gameObject.AddComponent<BoxCollider>();

        // Place new warp point near cloned gate asset
        savePointObjClone.transform.position = new Vector3(-8.4f, 0f, -15.7f);
        savePointObjClone.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        var allSavePoints = saveSystem.AllSavePoint;
        if (allSavePoints is null)
        {
            Melon<LwnApMod>.Logger.Msg("AllSavePoints was null");
            return;
        }

        var newArray = new Il2CppReferenceArray<SavePoint?>(allSavePoints.Length + 1);
        for (var i = 0; i < allSavePoints.Length; i++)
        {
            newArray[i] = allSavePoints[i];
        }

        newArray[allSavePoints.Length] = savePoint;
        saveSystem.AllSavePoint = newArray;

        Melon<LwnApMod>.Logger.Msg("Injected custom save point into save system");
    }

    private static void AddCustomWarpToLavaRuins()
    {
        var saveSystem = UnityUtils.FindObjectByPath("/SaveSystem")?.GetComponent<SaveSystem>();
        if (saveSystem is null)
        {
            Melon<LwnApMod>.Logger.Msg("Unable to find SaveSystem component");
            return;
        }

        var template =
            UnityUtils.FindObjectByPath("/Scene/Room01ToLevel02/Special/SavePointTransfer");

        var beforeMonicaObjClone = Object.Instantiate(template);
        var afterMonicaObjClone = Object.Instantiate(template);
        if (beforeMonicaObjClone is null || afterMonicaObjClone is null)
        {
            Melon<LwnApMod>.Logger.Msg("Failed to clone template object");
            return;
        }

        beforeMonicaObjClone.name = "BeforeMonica_WarpPoint";
        beforeMonicaObjClone.transform.rotation = Quaternion.Euler(0, 180, 0);
        var beforeMonicaSavePoint = beforeMonicaObjClone.GetComponentInChildren<SavePoint>(true);
        beforeMonicaSavePoint.name = "BeforeMonica_CustomWarpPoint";
        beforeMonicaSavePoint.TransferLevelNumber = 4;
        beforeMonicaSavePoint.TransferSavePointNumber = 7;

        afterMonicaObjClone.name = "AfterMonica_WarpPoint";
        afterMonicaObjClone.transform.rotation = Quaternion.Euler(0, 0, 0);
        var afterMonicaSavePoint = afterMonicaObjClone.GetComponentInChildren<SavePoint>(true);
        afterMonicaSavePoint.name = "AfterMonica_CustomWarpPoint";
        afterMonicaSavePoint.TransferLevelNumber = 4;
        afterMonicaSavePoint.TransferSavePointNumber = 6;

        // Before monica point placed before door after arena entrance blocked
        beforeMonicaObjClone.transform.position = new Vector3(18.6f, -12f, 287.75f);
        // After Monica point placed on final arena near broken arena entrance
        afterMonicaObjClone.transform.position = new Vector3(-97f, -20f, 319.4f);

        // Add effect to make interactability more visible
        var effectTemplate =
            UnityUtils.FindObjectByPath("/Scene/Room01_Save/Special/PassiveEventPrompt_GameSave/Effect");
        if (effectTemplate is not null)
        {
            Object.Instantiate(effectTemplate, beforeMonicaObjClone.transform);
            Object.Instantiate(effectTemplate, afterMonicaObjClone.transform);
        }

        // Do not expose warp point until Monica has been defeated
        if (Singletons.GameSave is not null && !Singletons.GameSave.flags.stage03Clear)
        {
            beforeMonicaObjClone.active = false;
            afterMonicaObjClone.active = false;
        }

        var allSavePoints = saveSystem.AllSavePoint;
        if (allSavePoints == null)
        {
            Melon<LwnApMod>.Logger.Msg("AllSavePoints was null");
            return;
        }

        var newArray = new Il2CppReferenceArray<SavePoint?>(allSavePoints.Length + 2);
        for (var i = 0; i < allSavePoints.Length; i++)
        {
            newArray[i] = allSavePoints[i];
        }

        newArray[allSavePoints.Length] = beforeMonicaSavePoint;
        newArray[allSavePoints.Length + 1] = afterMonicaSavePoint;
        saveSystem.AllSavePoint = newArray;

        Melon<LwnApMod>.Logger.Msg("Injected custom save point into save system");
    }

    private static void DisableDarkTunnelBridgeCutscenes()
    {
        var beforeBridgeCutscene = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08");
        if (beforeBridgeCutscene is not null)
        {
            beforeBridgeCutscene.GetComponent<BoxCollider>().enabled = false;
        }

        var postBridgeCutscene1 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_02");
        if (postBridgeCutscene1 is not null)
        {
            postBridgeCutscene1.GetComponent<BoxCollider>().enabled = false;
        }

        var postBridgeCutscene2 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_03");
        if (postBridgeCutscene2 is not null)
        {
            postBridgeCutscene2.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private static void AddExtraAssetsToDarkTunnel()
    {
        var magicWallParentTemplate = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room02/Other/MagicWall");
        var magicWallTemplate = magicWallParentTemplate?.GetComponent<MagicWall>().EffectMagicWall;
        if (magicWallTemplate is not null)
        {
            var clonedMagicWallRoom0802 = Object.Instantiate(magicWallTemplate);
            clonedMagicWallRoom0802.name = "Room08_02_Bridge_MagicWall";
            clonedMagicWallRoom0802.transform.position = new Vector3(36f, -16f, -191.75f);
            clonedMagicWallRoom0802.transform.rotation = Quaternion.Euler(0, 0, 0);

            var clonedMagicWallRoom08 = Object.Instantiate(magicWallTemplate);
            clonedMagicWallRoom08.name = "Room08_Bridge_MagicWall";
            clonedMagicWallRoom08.transform.position = new Vector3(219f, -16f, -167.6f);
            clonedMagicWallRoom08.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        var trapWallTemplate = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room01/Other/Tarp_Wall (1)");
        if (trapWallTemplate is not null)
        {
            var room08TrapWall = Object.Instantiate(trapWallTemplate);
            room08TrapWall.name = "Room08_TrapWall";
            room08TrapWall.transform.position = new Vector3(219f, -16f, -169.5f);
            room08TrapWall.transform.rotation = Quaternion.Euler(0, 0, 0);
            room08TrapWall.GetComponent<AttackData>().g_fRepulseMoveSpeed = 15;
            var room08TrapWallComponent = room08TrapWall.GetComponent<Trap_Wall_Level01>();
            room08TrapWallComponent.g_AD = room08TrapWall.GetComponent<AttackData>();
            room08TrapWallComponent.g_BC = room08TrapWall.GetComponent<BoxCollider>();
            room08TrapWallComponent.g_EffCollision = trapWallTemplate.GetComponent<Trap_Wall_Level01>().g_EffCollision;
            room08TrapWallComponent.g_SEM = trapWallTemplate.GetComponent<Trap_Wall_Level01>().g_SEM;
            var room0802TrapWall = Object.Instantiate(trapWallTemplate);
            room0802TrapWall.name = "Room08_02_TrapWall";
            room0802TrapWall.transform.position = new Vector3(36f, -16f, -189.5f);
            room0802TrapWall.transform.rotation = Quaternion.Euler(0, 180, 0);
            room0802TrapWall.GetComponent<AttackData>().g_fRepulseMoveSpeed = 15;
            var room0802TrapWallComponent = room0802TrapWall.GetComponent<Trap_Wall_Level01>();
            room0802TrapWallComponent.g_AD = room0802TrapWall.GetComponent<AttackData>();
            room0802TrapWallComponent.g_BC = room0802TrapWall.GetComponent<BoxCollider>();
            room0802TrapWallComponent.g_EffCollision =
                trapWallTemplate.GetComponent<Trap_Wall_Level01>().g_EffCollision;
            room0802TrapWallComponent.g_SEM = trapWallTemplate.GetComponent<Trap_Wall_Level01>().g_SEM;
        }

        var bridgeObjects = UnityUtils.FindObjectByPath("/Scene/Room08Script_02_Save/Damage");
        if (bridgeObjects is not null)
        {
            bridgeObjects.transform.Find("Original01")?.gameObject.SetActive(true);
            bridgeObjects.transform.Find("Original02")?.gameObject.SetActive(true);
            bridgeObjects.transform.Find("Original03")?.gameObject.SetActive(true);
            bridgeObjects.transform.Find("Original05")?.gameObject.SetActive(true);
            bridgeObjects.transform.Find("Original06")?.gameObject.SetActive(true);

            var original4 = bridgeObjects.transform.Find("Original04");
            original4?.gameObject.SetActive(true);
            original4?.Find("Base_Broken_02 (2)").gameObject.SetActive(false);
            original4?.Find("Bricks_03 (5)").gameObject.SetActive(false);
            original4?.Find("Railing_Broken_05 (4)").gameObject.SetActive(false);
            original4?.Find("Railing_Pillar_02 (3)").gameObject.SetActive(false);
            original4?.Find("Railing_Pillar_02 (5)").gameObject.SetActive(false);
            original4?.Find("Railing_Broken_05 (3)").gameObject.SetActive(false);
            original4?.Find("Base_Floor_Broken_06").gameObject.SetActive(false);
            original4?.Find("Arch_04 (18)").gameObject.SetActive(false);
            original4?.Find("Railing_01 (44)").gameObject.SetActive(false);
            original4?.Find("Base_Broken_Corner_01 (7)").gameObject.SetActive(false);
            original4?.Find("Arch_04 (16)").gameObject.SetActive(false);
            original4?.Find("Arch_04 (19)").gameObject.SetActive(false);
            original4?.Find("Ivy_01 (1)").gameObject.SetActive(false);
            original4?.Find("Column_B_04 (14)").gameObject.SetActive(false);
            original4?.Find("Torch_03 (24)").gameObject.SetActive(false);
        }
    }

    private static void AddCustomWarpToDarkTunnel()
    {
        var saveSystem = UnityUtils.FindObjectByPath("/SaveSystem")?.GetComponent<SaveSystem>();
        if (saveSystem == null)
        {
            Melon<LwnApMod>.Logger.Msg("Unable to find SaveSystem component");
            return;
        }

        var template =
            UnityUtils.FindObjectByPath(
                "/Scene/Room09ToBoss_Save/Special/SavePointTransfer_ToLevel01");
        var savePointObjCloneThroneSide = Object.Instantiate(template);
        var savePointObjCloneNotThroneSide = Object.Instantiate(template);
        if (savePointObjCloneThroneSide is null || savePointObjCloneNotThroneSide is null)
        {
            Melon<LwnApMod>.Logger.Msg("Failed to clone template object");
            return;
        }

        savePointObjCloneThroneSide.name = "Room08_02_WarpPoint";
        savePointObjCloneThroneSide.transform.rotation = Quaternion.Euler(0, 0, 0);
        var savePointThroneSide = savePointObjCloneThroneSide.GetComponentInChildren<SavePoint>(true);
        savePointThroneSide.name = "Room08_02_CustomWarpPoint";
        savePointThroneSide.TransferLevelNumber = 5;
        savePointThroneSide.TransferSavePointNumber = 7;

        savePointObjCloneNotThroneSide.name = "Room08_WarpPoint";
        savePointObjCloneNotThroneSide.transform.rotation = Quaternion.Euler(0, 0, 0);
        var savePointNotThroneSide = savePointObjCloneNotThroneSide.GetComponentInChildren<SavePoint>(true);
        savePointNotThroneSide.name = "Room08_CustomWarpPoint";
        savePointNotThroneSide.TransferLevelNumber = 5;
        savePointNotThroneSide.TransferSavePointNumber = 8;

        // Place warp point on bridge (that was reactivated even after breaking)
        savePointObjCloneThroneSide.transform.position = new Vector3(36f, -16f, -193.3f);
        savePointObjCloneNotThroneSide.transform.position = new Vector3(219f, -16f, -166.7f);

        // Add effect to make interactability more visible
        var effectTemplate =
            UnityUtils.FindObjectByPath("/Scene/Room09ToBoss_Save/Special/PassiveEventPrompt_CanNotBack/Effect");
        if (effectTemplate is not null)
        {
            Object.Instantiate(effectTemplate, savePointObjCloneThroneSide.transform);
            Object.Instantiate(effectTemplate, savePointObjCloneNotThroneSide.transform);
        }

        var allSavePoints = saveSystem.AllSavePoint;
        if (allSavePoints == null)
        {
            Melon<LwnApMod>.Logger.Msg("AllSavePoints was null");
            return;
        }

        var newArray = new Il2CppReferenceArray<SavePoint?>(allSavePoints.Length + 2);
        for (var i = 0; i < allSavePoints.Length; i++)
        {
            newArray[i] = allSavePoints[i];
        }

        newArray[allSavePoints.Length] = savePointNotThroneSide;
        newArray[allSavePoints.Length + 1] = savePointThroneSide;
        saveSystem.AllSavePoint = newArray;

        Melon<LwnApMod>.Logger.Msg("Injected custom save point into save system");
    }

    // Add SavePoint to SaveManager internal array on scene init. This can be finicky as we need to add
    // the save points before Nobeta spawns in (or else it will default to beginning of the stage), but
    // the GameObjects we clone might not be available, leading to a null exception.
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Init))]
    private static class SceneManagerInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void AddCustomSavePoints(SceneManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            Melon<LwnApMod>.Logger.Msg("Running CustomWarp init patches");
            if (Singletons.WizardGirl is null) return;
            switch (__instance.stageId)
            {
                case (int)StageId.Shrine:
                    AddCustomWarpToShrine();
                    break;
                case (int)StageId.Underground:
                    AddCustomWarpToUnderground();
                    break;
                case (int)StageId.LavaRuins:
                    AddCustomWarpToLavaRuins();
                    break;
                case (int)StageId.DarkTunnel:
                    AddCustomWarpToDarkTunnel();
                    break;
            }
        }
    }

    // Less time sensitive actions are put here, such as cloning static assets
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class SceneManagerStageInitComplete
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void AfterStageInitActions(SceneManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            Melon<LwnApMod>.Logger.Msg("Running CustomWarp OnSceneInitComplete patches");

            if (Singletons.WizardGirl is null) return;
            switch (__instance.stageId)
            {
                case (int)StageId.Shrine:
                    AddExtraAssetsToShrine();
                    break;
                case (int)StageId.Underground:
                    AddExtraAssetsToUnderground();
                    break;
                case (int)StageId.DarkTunnel:
                    DisableDarkTunnelBridgeCutscenes();
                    AddExtraAssetsToDarkTunnel();
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(StageUIManager), nameof(StageUIManager.GetExitLevelName))]
    public static class GetExitLevelNamePatch
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        public static void CustomPointNames(int transferLevelNum, int transferSavePointNum, ref string __result)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            __result = transferLevelNum switch
            {
                2 when transferSavePointNum == 6 => "Okun Shrine - Central Hall",
                3 when transferSavePointNum == 7 => "Underground - Entrance",
                4 when transferSavePointNum == 6 => "Lava Ruins - Before Monica Arena",
                4 when transferSavePointNum == 7 => "Lava Ruins - Monica Arena",
                5 when transferSavePointNum == 7 => "Dark Tunnel - Before Bridge",
                5 when transferSavePointNum == 8 => "Dark Tunnel - After Bridge",
                _ => __result
            };
        }
    }

    // Only allow the Monica arena warp point to appear after defeating Monica but before a stage reset.
    // After a stage reset, it will check the boss flag instead.
    [HarmonyPatch(typeof(LoadScript), nameof(LoadScript.OpenEvent))]
    public static class MonicaDefeatedCutsceneOpen
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        public static void AddWarpOnMonicaDefeat(LoadScript __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!Singletons.SceneManager) return;
            if (Singletons.SceneManager.stageId != (int)StageId.LavaRuins) return;
            var path = UnityUtils.GetObjectPath(__instance.gameObject);
            if (path != "/SEM/AreaEvent/RoomBoss02/Other/LoadScriptBoss03") return;
            var saveSystem = UnityUtils.FindObjectByPath("/SaveSystem")?.GetComponent<SaveSystem>();
            if (saveSystem == null)
            {
                Melon<LwnApMod>.Logger.Msg("Unable to find SaveSystem component");
                return;
            }

            saveSystem.AllSavePoint[6]?.transform.parent.gameObject.SetActive(true);
            saveSystem.AllSavePoint[7]?.transform.parent.gameObject.SetActive(true);
        }
    }
}