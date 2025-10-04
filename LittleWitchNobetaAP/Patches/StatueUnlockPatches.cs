using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP.Patches;

public static class StatueUnlockPatches
{
    private const float UnlockDistance = 15f;
    private const string DataStorageKeyUnlockedStatues = "unlocked_statues";
    private const string StatueKeySeparator = ":::";

    private static SavePoint[]? _statues;

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class OnSceneInitComplete
    {
        [HarmonyPostfix]
        // ReSharper disable UnusedMember.Local
        private static void OnSceneInitCompletePostfix() 
        // ReSharper restore UnusedMember.Local
        {
            _statues = UnityUtils.FindComponentsByTypeForced<SavePoint>().Where(savePoint => savePoint.EventType == PassiveEvent.PassiveEventType.SavePoint).ToArray();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    private static class EnterLoaderScene
    {
        [HarmonyPostfix]
        // ReSharper disable UnusedMember.Local
        private static void EnterLoaderScenePostfix()
        // ReSharper restore UnusedMember.Local
        {
            _statues = null;
        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    private static class OnWizardGirlUpdate
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void OnWizardGirlUpdatePostfix(WizardGirlManage __instance)
        // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            switch (Singletons.SceneManager.stageId)
            {
                case > 1 when __instance.GetPlayerStatus() == NobetaState.SavePointUI:
                    // Check if new items received while offline
                    ArchipelagoClient.DequeueItems();
                    break;
                case > 1:
                    ArchipelagoClient.DeathLinkHandler?.KillPlayer();
                    break;
            }

            if (_statues is null)
            {
                return;
            }

            var gameSave = Game.GameSave.basic;
            var stageName = Game.sceneManager.stageName;
            var gameStage = gameSave.GetStage(stageName);

            foreach (var statue in _statues)
            {
                var savePointNumber = Game.sceneManager.GetSavePointNumber(statue);

                if (gameSave.HasSavePointUnlocked(gameStage, savePointNumber))
                {
                    continue;
                }

                var distance = Vector3.Distance(statue.transform.position, __instance.transform.position);

                if (!(distance < UnlockDistance)) continue;
            
                Melon<LwnApMod>.Logger.Msg($"Statue '{statue.name}#{statue.TransferLevelNumber}#{savePointNumber}' auto-unlocked");

                UnlockStatueInAp($"{stageName};{savePointNumber}");

                gameSave.AddNewSavePoint(stageName, savePointNumber);
                gameSave.stage = gameStage;
                gameSave.savePoint = savePointNumber;

                Game.AppearEventPrompt($"Nearby statue unlocked: {Game.GetLocationText(gameStage, savePointNumber)}");
            }
        }

        private static async void UnlockStatueInAp(string statueName)
        {
            try
            {
                var dataStorageContent = ArchipelagoClient.Session?.DataStorage[DataStorageKeyUnlockedStatues];
                if (ArchipelagoClient.Session is null || dataStorageContent is null) return;
                
                var unlockedStatues = (await dataStorageContent.GetAsync()).ToObject<string>();
                if (unlockedStatues is not null)
                {
                    if (!unlockedStatues.Contains(statueName))
                    {
                        unlockedStatues += $"{StatueKeySeparator}{statueName}";
                    }
                }
                else
                {
                    unlockedStatues = statueName;
                }
                    
                ArchipelagoClient.Session.DataStorage[DataStorageKeyUnlockedStatues] = unlockedStatues;
            }
            catch (Exception e)
            {
                Melon<LwnApMod>.Logger.Error(e);
            }
        }
    }
}