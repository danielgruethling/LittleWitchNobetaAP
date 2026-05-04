using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using Object = UnityEngine.Object;
using UnityEngine;
using Random = System.Random;

namespace LittleWitchNobetaAP.Patches;

public enum TrapType
{
    BonkTrap,
    ManaDrainTrap,
}

// An ActiveEvent represents a game event that should only occur
// when the player has control and isn't in a cutscene or UI menu
internal abstract class ActiveEvent
{
}

internal class TrapEvent : ActiveEvent
{
    public TrapType TrapType { get; }
    public TrapEvent(TrapType type) => TrapType = type;
}

internal class GiveItemEvent : ActiveEvent
{
    public ItemSystem.ItemType ItemType { get; }
    public GiveItemEvent(ItemSystem.ItemType itemType) => ItemType = itemType;
}

public static class FillerItemPatches
{
    private static readonly Random Random = new();
    private static readonly Queue<ActiveEvent> EventQueue = new();
    private static float _trapCooldown = 0;

    public static void QueueTrap(TrapType trapType)
    {
        EventQueue.Enqueue(new TrapEvent(trapType));
    }

    public static void QueueDropItem(ItemSystem.ItemType itemType)
    {
        EventQueue.Enqueue(new GiveItemEvent(itemType));
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
            // For some reason, "down" actually means up. A bonk trap could, with extreme luck, allow for
            // out-of-logic movement, so the up-bonk is made less powerful than other directions.
            var repulseStrength = (randomDirection == (int)AttackData.AttackDirection.Down)
                ? Random.Next(3, 7)
                : Random.Next(10, 20);
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

    private static void GiveItem(ItemSystem.ItemType itemType)
    {
        var wizardGirl = Singletons.WizardGirl;
        var items = wizardGirl?.g_PlayerItem;
        var itemSystem = Singletons.ItemSystem;
        if (wizardGirl == null || items == null || itemSystem == null) return;

        Melon<LwnApMod>.Logger.Msg($"Giving item {itemType}");

        MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
        {
            // Find first empty slot if there's any
            for (var i = 0; i < items.g_iItemSize; i++)
            {
                if (items.g_HoldItem[i] != ItemSystem.ItemType.Null) continue;
                items.g_HoldItem[i] = itemType;
                Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                return;
            }

            // Special handling for Trial key items
            if (itemType == ItemSystem.ItemType.SPMaxAdd)
            {
                Melon<LwnApMod>.Logger.Msg($"Attempting to add trial key to inventory");
                for (var i = 0; i < items.g_iItemSize; i++)
                {
                    if (items.g_HoldItem[i] == ItemSystem.ItemType.SPMaxAdd) continue;

                    Melon<LwnApMod>.Logger.Msg($"Adding trial key to item slot {i}");
                    items.g_HoldItem[i] = itemType;
                    Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                    Game.CreateSoul(SoulSystem.SoulType.Money, wizardGirl.transform.position, 400);

                    return;
                }
            }
            // If no empty slots, drop on floor where player is standing
            else
            {
                itemSystem.NewItem(itemType, wizardGirl.transform.position, Quaternion.identity);
            }
        }));
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    private static class OnWizardGirlUpdate
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void TrySendActiveTrap(WizardGirlManage __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (!EventQueue.Any()) return;
            if (Singletons.SceneManager.stageId <= 1) return;
            // This flag should be false during cutscenes or UI menus
            if (!__instance.PlayerController.CharacterControllable) return;
            if (_trapCooldown > 0)
            {
                _trapCooldown -= __instance.PlayerController.DeltaTime;
                return;
            }

            var first = EventQueue.Dequeue();
            Melon<LwnApMod>.Logger.Msg($"Executing active event: {first}");
            switch (first)
            {
                case TrapEvent trapEvent:
                    switch (trapEvent.TrapType)
                    {
                        case TrapType.BonkTrap:
                            GiveBonkTrap();
                            break;
                        case TrapType.ManaDrainTrap:
                            GiveManaDrainTrap();
                            break;
                        default:
                            Melon<LwnApMod>.Logger.Error($"Unknown trap event detected.");
                            break;
                    }

                    break;
                case GiveItemEvent itemEvent:
                    GiveItem(itemEvent.ItemType);
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