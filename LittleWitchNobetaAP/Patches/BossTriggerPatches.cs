using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LittleWitchNobetaAP.Archipelago;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Action = Il2CppSystem.Action;
using Object = UnityEngine.Object;

namespace LittleWitchNobetaAP.Patches;

// Handles logic around boss triggers when souls or skippable bosses are enabled.
// The trigger for Nonota is handled by EndRequirementPatches!
public class BossTriggerPatches
{
    public static void enableBossTriggerOnItem(string itemName)
    {
        if (!ArchipelagoData.BossTriggers.BySoulName.TryGetValue(itemName, out var bossTrigger)) return;
        if (Singletons.SceneManager.stageId != (int)bossTrigger.StageId) return;
        
        var trigger = UnityUtils.FindObjectByPath(bossTrigger.Trigger);
        if (trigger is null)
        {
            Melon<LwnApMod>.Logger.Warning($"Failed to get trigger for {itemName}");
            return;
        }
        trigger.GetComponent<BoxCollider>().enabled = true;
        var maybeBossTriggerEffect = trigger.transform.Find("BossTriggerEffect");
        maybeBossTriggerEffect?.gameObject.SetActive(true);
    }
    
    private static void SetColliderWorldPosition(BoxCollider collider, Vector3 worldPos)
    {
        var localPos = collider.transform.InverseTransformPoint(worldPos);
        collider.center = localPos;
    }
    
    private static void HandleEnragedArmorTrigger()
    {
        if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
        {
            return;
        }

        var hasSoul =
            ArchipelagoClient.Session.Items.AllItemsReceived.Any(item => item.ItemName == "Enraged Armor Soul");
        var triggerPath = ArchipelagoData.BossTriggers.BySoulName["Enraged Armor Soul"].Trigger;
        var trigger = UnityUtils.FindObjectByPath(triggerPath);
        if (trigger is null)
        {
            Melon<LwnApMod>.Logger.Warning("Failed to find trigger for Enraged Armor");
            return;
        }
        var collider = trigger.GetComponent<BoxCollider>();

        GameObject? effect = null;
        if (ArchipelagoClient.ServerData.Settings?.SkippableBossesEnabled ?? false)
        {
            SetColliderWorldPosition(collider, new Vector3(-88f, -30f, -304f));
            collider.size = new Vector3(1f, 1f, 1f);
            var effectTemplate =
                UnityUtils.FindObjectByPath(
                    "/Scene/Room01/Special/PassiveEventPrompt_GameSave/Effect/Particle System (3)");
            if (effectTemplate is not null)
            {
                effect = Object.Instantiate(effectTemplate, trigger.transform);
                effect.name = "BossTriggerEffect";
                effect.transform.position = new Vector3(-88f, -29.95f, -304f);
            }
        }
        
        if ((ArchipelagoClient.ServerData.Settings?.RandomizeBossSoulsEnabled ?? false) && !hasSoul)
        {
            collider.enabled = false;
            effect?.SetActive(false);
        }
    }
    
    private static void HandleTaniaTrigger()
    {
        if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
        {
            return;
        }

        var hasSoul =
            ArchipelagoClient.Session.Items.AllItemsReceived.Any(item => item.ItemName == "Tania Soul");
        var triggerPath = ArchipelagoData.BossTriggers.BySoulName["Tania Soul"].Trigger;
        var trigger = UnityUtils.FindObjectByPath(triggerPath);
        if (trigger is null)
        {
            Melon<LwnApMod>.Logger.Warning("Failed to find trigger for Tania");
            return;
        }
        var collider = trigger.GetComponent<BoxCollider>();

        GameObject? effect = null;
        if (ArchipelagoClient.ServerData.Settings?.SkippableBossesEnabled ?? false)
        {
            SetColliderWorldPosition(collider, new Vector3(175f, 4.68f, -13.8f));
            collider.size = new Vector3(1f, 1f, 1f);
            var effectTemplate =
                UnityUtils.FindObjectByPath(
                    "/Scene/Room01/Special/PassiveEventPrompt_SkyJump/Effect/Particle System (3)");
            if (effectTemplate is not null)
            {
                effect = Object.Instantiate(effectTemplate, trigger.transform);
                effect.name = "BossTriggerEffect";
                effect.transform.position = new Vector3(175f, 4.68f, -13.8f);
            }
        }
        
        if ((ArchipelagoClient.ServerData.Settings?.RandomizeBossSoulsEnabled ?? false) && !hasSoul)
        {
            collider.enabled = false;
            effect?.SetActive(false);
        }
    }

    private static void HandleBossTrigger(string soulName)
    {
        if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
        {
            return;
        }

        var hasSoul =
            ArchipelagoClient.Session.Items.AllItemsReceived.Any(item => item.ItemName == soulName);
        if (!ArchipelagoData.BossTriggers.BySoulName.TryGetValue(soulName, out var bossTrigger)) return;
        var triggerPath = bossTrigger.Trigger;
        var trigger = UnityUtils.FindObjectByPath(triggerPath);
        if (trigger is null)
        {
            Melon<LwnApMod>.Logger.Warning($"Failed to find trigger for {soulName}");
            return;
        }
        var collider = trigger.GetComponent<BoxCollider>();
        if ((ArchipelagoClient.ServerData.Settings?.RandomizeBossSoulsEnabled ?? false) && !hasSoul)
        {
            collider.enabled = false;
        }
    }

    public static void HandleBossTriggers()
    {
        if (Singletons.WizardGirl is null) return;
        Melon<LwnApMod>.Logger.Msg("Running boss trigger init patches");
        switch (Singletons.SceneManager.stageId)
        {
            case (int)StageId.Shrine:
                HandleBossTrigger("Specter Armor Soul");
                HandleEnragedArmorTrigger();
                break;
            case (int)StageId.Underground:
                HandleTaniaTrigger();
                break;
            case (int)StageId.LavaRuins:
                HandleBossTrigger("Monica Soul");
                break;
            case (int)StageId.DarkTunnel:
                HandleBossTrigger("Vanessa Soul");
                break;
            case (int)StageId.SpiritRealm:
                HandleBossTrigger("Vanessa V2 Soul");
                break;
        }
    } 
    
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    private static class SceneManagerStageInitComplete
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void AfterStageInitActions(SceneManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            HandleBossTriggers();
        }
    }
    
    // Disable the effect after touching the boss trigger if the effect exists
    [HarmonyPatch(typeof(LoadScript), nameof(LoadScript.OpenEvent))]
    private static class LoadScriptOpenEvent
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void DisableEffect(LoadScript __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var triggerPath = UnityUtils.GetObjectPath(__instance.gameObject);
            var bossTrigger = ArchipelagoData.BossTriggers.Triggers.FirstOrDefault(bt =>
                Singletons.SceneManager.stageId == (int)bt.StageId && bt.Trigger == triggerPath);
            if (bossTrigger is null) return;
            var maybeEffect = __instance.transform.Find("BossTriggerEffect");
            maybeEffect?.gameObject.SetActive(false);
        }
    }
    
}