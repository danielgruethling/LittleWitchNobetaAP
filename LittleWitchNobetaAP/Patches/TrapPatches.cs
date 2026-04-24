using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using Object = UnityEngine.Object;
using UnityEngine;
using Random = System.Random;

namespace LittleWitchNobetaAP.Patches;

// An ActiveEvent represents a game event that should only occur
// when the player has control and isn't in a cutscene or UI menu
internal enum ActiveEvent
{
    BonkTrap,
    ManaDrainTrap,
}

public static class TrapPatches
{
    private static readonly Random Random = new();
    private static Queue<ActiveEvent> _eventQueue = new();
    private static float _trapCooldown = 0;
    
    public static void QueueBonkTrap()
    {
        _eventQueue.Enqueue(ActiveEvent.BonkTrap);
    }
    
    // While Mana Drain works in cutscenes, it will also regen in cutscenes, meaning
    // player might not see effect in a long cutscene unless queued as an active trap
    public static void QueueManaDrainTrap()
    {
        _eventQueue.Enqueue(ActiveEvent.ManaDrainTrap);
    }

    private static void GiveBonkTrap()
    {
        if (Singletons.WizardGirl is null) return;
        Melon<LwnApMod>.Logger.Error("Executing Bonk Trap.");
        var trapWallDonor = Object.FindObjectOfType<Trap_Wall_Level01>(true);
        if (trapWallDonor == null)
        {
            Melon<LwnApMod>.Logger.Error("Could not find an TrapWall component in the current scene!");
            return;
        }
        Melon<LwnApMod>.Logger.Msg($"Using {UnityUtils.GetObjectPath(trapWallDonor.gameObject)} as bonk trap donor.");
        var attack = trapWallDonor.GetComponent<AttackData>();
        var originalDirection = attack.GetAttackDirection();
        var originalAttackElement = attack.GetAttackElement();
        var originalAttackType = attack.GetAttackType();
        var originalHitDirection = attack.GetHitDirection();
        var originalRepulseSpeed = attack.g_fRepulseMoveSpeed;
        
        MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
        {
            var randomDirection = Random.Next(0, 5);
            var repulseStrength = (randomDirection == (int)AttackData.AttackDirection.Down) ? 3 : Random.Next(5, 16);
            Melon<LwnApMod>.Logger.Msg($"Bonking in direction: {(AttackData.AttackDirection)randomDirection}.");
            Melon<LwnApMod>.Logger.Msg($"Bonking with strength: {repulseStrength}.");
            attack.SetAttackDirection((AttackData.AttackDirection)randomDirection);
            attack.SetAttackElement(PlayerEffectPlay.Magic.Null);
            attack.SetAttackType(AttackData.AttackType.Fly);
            attack.SetHitDirection(new Vector3(Random.Next(-50, 50), 0, Random.Next(-50, 50)));
            attack.g_fRepulseMoveSpeed = repulseStrength;
            Singletons.WizardGirl.Hit(attack, true);
            attack.SetAttackDirection(originalDirection);
            attack.SetAttackElement(originalAttackElement);
            attack.SetAttackType(originalAttackType);
            attack.SetHitDirection(originalHitDirection);
            attack.g_fRepulseMoveSpeed = originalRepulseSpeed;
        }));
    }
    
    private static void GiveManaDrainTrap()
    {
        if (Singletons.WizardGirl is null) return;
        Singletons.WizardGirl.BaseData.SetMP(0);
    }
    
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    private static class OnWizardGirlUpdate
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void TrySendActiveTrap(WizardGirlManage __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!_eventQueue.Any()) return;
            if (Singletons.SceneManager.stageId <= 1) return;
            // This flag should be false during cutscenes or UI menus
            if (!__instance.PlayerController.CharacterControllable) return;
            if (_trapCooldown > 0)
            {
                _trapCooldown -= __instance.PlayerController.DeltaTime;
                return;
            }
            

            var first = _eventQueue.Dequeue();
            Melon<LwnApMod>.Logger.Msg($"Executing active event: {first}");
            switch (first)
            {
                case ActiveEvent.BonkTrap:
                    GiveBonkTrap();
                    break;
                case ActiveEvent.ManaDrainTrap:
                    GiveManaDrainTrap();
                    break;
                default:
                    Melon<LwnApMod>.Logger.Error($"Unknown active event detected.");
                    break;
            }

            // Set a cooldown for 0.5 seconds between active traps
            _trapCooldown = 0.5f;
        }
    }
}