using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LittleWitchNobetaAP.Archipelago;
using System.Reflection;
using Archipelago.MultiClient.Net;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Action = Il2CppSystem.Action;
using Object = UnityEngine.Object;

namespace LittleWitchNobetaAP.Patches;

public class EndRequirementsPatches
{
    private static readonly string NonotaTriggerPath = "/SEM/AreaEvent/RoomBoss/Other/LoadScriptBoss02";
    private static readonly string TeleportEnablePath = "/SEM/AreaEvent/RoomCentral/Other/00_OpenTeleport04";
    private static GameObject? _nonotaTrigger;
    private static GameObject? _throne;
    private static TeleportEnable? _enableTeleport;
    private static GameObject? _teleporter;
    private static bool _enteredNonotaMsgField;
    private static bool _enteredAbyssTeleporterField;

    private static bool HasAbyssTrialRequirements()
    {
        if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return false;

        switch (ArchipelagoClient.ServerData.Settings?.AbyssTrialRequirement)
        {
            case ArchipelagoSettings.AbyssTrialRequirementType.MagicMaster:
            {
                return Singletons.GameSave?.stats is
                {
                    secretMagicLevel: >= 5,
                    iceMagicLevel: >= 5,
                    fireMagicLevel: >= 5,
                    thunderMagicLevel: >= 5,
                };
            }
            case ArchipelagoSettings.AbyssTrialRequirementType.BossHunt:
            {
                var bossTokenTargets = ArchipelagoData.Items
                    .Where(pair => pair.Value == "Boss Tokens")
                    .Select(pair => pair.Key)
                    .ToList();
                var hasAllTokens = bossTokenTargets.All(targetName =>
                    ArchipelagoClient.Session.Items.AllItemsReceived.Any(receivedItem =>
                        receivedItem.ItemName == targetName)
                );
                return hasAllTokens;
            }
            case ArchipelagoSettings.AbyssTrialRequirementType.LoreKeeper:
            {
                var numLoreItems = Singletons.GameSave?.props.GetPropCollectionAmount();
                var requiredAmount = ArchipelagoClient.ServerData.Settings.RandomizeLoreEnabled ==
                                     ArchipelagoSettings.RandomizeLore.Vanilla
                    ? 102
                    : 103;
                return numLoreItems >= requiredAmount;
            }
            case ArchipelagoSettings.AbyssTrialRequirementType.RandomizedItem:
            case ArchipelagoSettings.AbyssTrialRequirementType.Vanilla:
            {
                var targetItems = ArchipelagoData.Items
                    .Where(pair => pair.Value == "Abyss Trial Clear")
                    .Select(pair => pair.Key)
                    .ToList();
                var hasAllTrials = targetItems.All(targetName =>
                    ArchipelagoClient.Session.Items.AllItemsReceived.Any(receivedItem =>
                        receivedItem.ItemName == targetName)
                );
                return hasAllTrials;
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class SceneManagerOnSceneInitComplete
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void DisableNonotaTrigger(SceneManager __instance)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.stageId != (int)StageId.Abyss) return;
            var nonotaTrigger = UnityUtils.FindObjectByPath(NonotaTriggerPath);
            if (nonotaTrigger is null)
            {
                Melon<LwnApMod>.Logger.Error("Failed to get Nonota fight trigger while in Abyss.");
                return;
            }

            nonotaTrigger.SetActive(false);
            var throne = UnityUtils.FindObjectByPath("/Scene/RoomBoss/Special/act07_07_Throne");
            _throne = throne;
            _nonotaTrigger = nonotaTrigger;
            Melon<LwnApMod>.Logger.Msg("Disabled Nonota Trigger on stage load.");
        }

        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void SaveAbyssTeleporter(SceneManager __instance)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.stageId != (int)StageId.Abyss) return;
            var enableTeleport = UnityUtils.FindObjectByPath(TeleportEnablePath)?.GetComponent<TeleportEnable>();
            if (enableTeleport is null)
            {
                Melon<LwnApMod>.Logger.Error("Failed to get Abyss teleporter while in Abyss.");
                return;
            }

            _enableTeleport = enableTeleport;
            _teleporter = enableTeleport.T.First().gameObject;
            Melon<LwnApMod>.Logger.Msg("Saved Abyss teleporter reference.");
        }
    }
    
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnDisable))]
    private static class SceneManagerOnDisable
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void DisableReferences(SceneManager __instance)
            // ReSharper restore InconsistentNaming
        {
            _enableTeleport = null;
            _teleporter = null;
            _nonotaTrigger = null;
            _throne = null;
        }
    }

    // Allow teleporter open event if requirements are met
    // This allows teleporter to open on cutscene if all three switches destroyed
    [HarmonyPatch(typeof(TeleportEnable), nameof(TeleportEnable.OpenEvent))]
    private static class TeleportEnableOpenEvent
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming
        private static bool HandleOpen(TeleportEnable __instance)
            // ReSharper restore InconsistentNaming
        {
            return HasAbyssTrialRequirements();
        }
    }
    
    // Special handler for Abyss Trial Complete checks
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

            var locationName = path switch
            {
                "/Scene/RoomCentral/Special/SwitchDevice_Strengthen (1)/AttackabclObject02_Act03" =>
                    "Abyss - Underground Trial Complete",
                "/Scene/RoomCentral/Special/SwitchDevice_Strengthen/AttackabclObject01_Act04" =>
                    "Abyss - Lava Ruins Trial Complete",
                "/Scene/RoomCentral/Special/SwitchDevice_Strengthen (2)/AttackabclObject03_Act05" =>
                    "Abyss - Dark Tunnel Trial Complete",
                _ => null
            };

            if (locationName is null) return;
            Melon<LwnApMod>.Logger.Msg($"Abyss trial switch release event detected with path {path}.");
            var locationId = ArchipelagoData.GetLocationIdByName(locationName);
            ArchipelagoClient.Session.Locations.CompleteLocationChecks(locationId);

        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    private static class WizardGirlManageUpdate
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void ShowPromptNearNonota(WizardGirlManage __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (Singletons.SceneManager.stageId != (int)StageId.Abyss) return;
            if (_throne is null || _nonotaTrigger is null) return;
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return;
            var distance = Vector3.Distance(_throne.transform.position, __instance.transform.position);

            // Prevent prompt from showing again if user still hasn't left the area
            if (distance > 16f)
            {
                _enteredNonotaMsgField = false;
                return;
            }

            if (_enteredNonotaMsgField) return;
            _enteredNonotaMsgField = true;

            switch (ArchipelagoClient.ServerData.Settings?.Goal)
            {
                case ArchipelagoSettings.GoalType.MagicMaster:
                {
                    if (Singletons.GameSave?.stats is not
                        {
                            secretMagicLevel: >= 5,
                            iceMagicLevel: >= 5,
                            fireMagicLevel: >= 5,
                            thunderMagicLevel: >= 5,
                        })
                    {
                        Game.AppearEventPrompt("Only a Magic Master may approach the throne.");
                    }
                    else
                    {
                        _nonotaTrigger.SetActive(true);
                    }

                    break;
                }
                case ArchipelagoSettings.GoalType.BossHunt:
                {
                    var bossTokenTargets = ArchipelagoData.Items
                        .Where(pair => pair.Value == "Boss Tokens")
                        .Select(pair => pair.Key)
                        .ToList();
                    var hasAllTokens = bossTokenTargets.All(targetName =>
                        ArchipelagoClient.Session.Items.AllItemsReceived.Any(receivedItem =>
                            receivedItem.ItemName == targetName)
                    );
                    if (!hasAllTokens)
                    {
                        Game.AppearEventPrompt("Only an accomplished Boss Hunter may approach the throne.");
                    }
                    else
                    {
                        _nonotaTrigger.SetActive(true);
                    }

                    break;
                }
                case ArchipelagoSettings.GoalType.LoreKeeper:
                {
                    var numLoreItems = Singletons.GameSave?.props.GetPropCollectionAmount();
                    var requiredAmount = ArchipelagoClient.ServerData.Settings.RandomizeLoreEnabled ==
                                         ArchipelagoSettings.RandomizeLore.Vanilla
                        ? 102
                        : 103;
                    if (numLoreItems < requiredAmount)
                    {
                        Game.AppearEventPrompt("Only a true Lore Keeper may approach the throne.");
                    }
                    else
                    {
                        _nonotaTrigger.SetActive(true);
                    }

                    break;
                }
                case ArchipelagoSettings.GoalType.Vanilla:
                {
                    _nonotaTrigger.SetActive(true);
                    break;
                }
            }

            if (_nonotaTrigger.active)
            {
                Melon<LwnApMod>.Logger.Msg("Nonota trigger enabled as goal condition met.");
            }
        }

        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void ShowPromptNearTeleporter(WizardGirlManage __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (Singletons.SceneManager.stageId != (int)StageId.Abyss) return;
            if (_teleporter is null) return;
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return;

            var distance = Vector3.Distance(_teleporter.transform.position, __instance.transform.position);

            // Prevent prompt from showing again if user still hasn't left the area
            if (distance > 6f)
            {
                _enteredAbyssTeleporterField = false;
                return;
            }

            if (_enteredAbyssTeleporterField) return;
            _enteredAbyssTeleporterField = true;

            var hasRequirements = HasAbyssTrialRequirements();

            if (!hasRequirements)
            {
                switch (ArchipelagoClient.ServerData.Settings?.AbyssTrialRequirement)
                {
                    case ArchipelagoSettings.AbyssTrialRequirementType.MagicMaster:
                        Game.AppearEventPrompt("Only a Magic Master may open the path.");
                        break;
                    case ArchipelagoSettings.AbyssTrialRequirementType.BossHunt:
                        Game.AppearEventPrompt("Only an accomplished Boss Hunter may open the path.");
                        break;
                    case ArchipelagoSettings.AbyssTrialRequirementType.LoreKeeper:
                        Game.AppearEventPrompt("Only a true Lore Keeper may open the path.");
                        break;
                    case ArchipelagoSettings.AbyssTrialRequirementType.RandomizedItem:
                        Game.AppearEventPrompt("Only a holder of the three trial items may open the path.");
                        break;
                    case  ArchipelagoSettings.AbyssTrialRequirementType.Vanilla:
                        Game.AppearEventPrompt("Complete the three trials to open the path.");
                        break;
                }
            }
            else
            {
                _enableTeleport?.OpenEvent();
                Melon<LwnApMod>.Logger.Msg("Abyss Trial Teleporter enabled as player has requirements.");
            }
        }
    }
}