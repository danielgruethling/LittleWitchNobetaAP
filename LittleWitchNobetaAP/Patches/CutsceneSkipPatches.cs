using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using UnityEngine;
using MelonLoader;

namespace LittleWitchNobetaAP.Patches;

public static class CutsceneSkipPatches
{
    public static void DisableCutscenes()
    {
        var cutscenesToSkip = ArchipelagoData.CutscenesToSkip.ByStageId[Singletons.SceneManager.stageId];
        foreach (var cutscene in cutscenesToSkip)
        {
            if (!cutscene.ShouldSkip()) continue;
            var gameObj = UnityUtils.FindObjectByPath(cutscene.Trigger);
            if (gameObj is null) continue;
            gameObj.GetComponent<BoxCollider>().enabled = false;
        }
    }
    
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class OnSceneInitComplete
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void SkipCutscene(SceneManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            DisableCutscenes();
        }
    }

    [HarmonyPatch(typeof(LoadScript), nameof(LoadScript.OpenEvent))]
    private static class LoadScriptOpenEvent
    {
        // If Dark Tunnel Bridge Collapse is not disabled, touching the cutscene trigger from the castle side will
        // softlock the game. However, if you enter the cutscene from the vanilla side and the 02 and 03 cutscenes
        // are disabled, it will also softlock the game. Here we specifically enable the 02 and 03 cutscenes once the
        // vanilla trigger is hit.
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void HandleBridgeCollapseCutscene(LoadScript __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return;
            if (Singletons.SceneManager.stageId != (int)StageId.DarkTunnel) return;
            if (__instance.name != "LoadScriptRoom08") return;
            if (ArchipelagoClient.ServerData.Settings?.DisableDarkTunnelBridgeCollapse ?? true) return;
            var nextCutscene = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_02");
            var nextCutscene2 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_03");
            if (nextCutscene is null || nextCutscene2 is null) return;
            Melon<LwnApMod>.Logger.Msg($"Bridge collapse cutscene detected, enabling followup cutscenes.");
            nextCutscene.GetComponent<BoxCollider>().enabled = true;
            nextCutscene2.GetComponent<BoxCollider>().enabled = true;
        }
    }
}