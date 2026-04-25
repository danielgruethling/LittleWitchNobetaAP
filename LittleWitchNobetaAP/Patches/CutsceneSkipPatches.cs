using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using UnityEngine;

namespace LittleWitchNobetaAP.Patches;

public static class CutsceneSkipPatches
{
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class OnSceneInitComplete
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void SkipCutscene(SceneManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
            {
                return;
            }

            var cutscenesToSkip = ArchipelagoData.CutscenesToSkip.ByStageId[Singletons.SceneManager.stageId];
            foreach (var cutscene in cutscenesToSkip)
            {
                if (!cutscene.ShouldSkip()) continue;
                var gameObj = UnityUtils.FindObjectByPath(cutscene.Trigger);
                if (gameObj is null) continue;
                gameObj.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
}