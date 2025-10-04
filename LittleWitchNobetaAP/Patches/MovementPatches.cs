using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LittleWitchNobetaAP.Patches;

public static class MovementPatches
{
    public static bool BlockInput { get; set; }
    
    public static class InputDisablePatches
    {
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Move))]
        private static class InputMove
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputMovePrefix(PlayerInputController __instance, Vector2 movement)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Aim))]
        private static class InputAim
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputAimPrefix(PlayerInputController __instance, bool onHolding)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.AppearMagicMenu))]
        private static class InputAppearMagicMenu
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputAppearMagicMenuPrefix(PlayerInputController __instance, bool onHolding)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Attack))]
        private static class InputAttack
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputAttackPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Chant))]
        private static class InputChant
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputChantPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Dash))]
        private static class InputDash
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputDashPrefix(PlayerInputController __instance, bool onHolding)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Dodge))]
        private static class InputDodge
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputDodgePrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.DropItem))]
        private static class InputDropItem
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputDropItemPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Jump))]
        private static class InputJump
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputJumpPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.SelectItem))]
        private static class InputSelectItem
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputSelectItemPrefix(PlayerInputController __instance, int index)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.SelectItemLeftward))]
        private static class InputSelectItemLeftward
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputSelectItemLeftwardPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.SelectItemRightward))]
        private static class InputSelectItemRightward
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputSelectItemRightwardPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Shoot))]
        private static class InputShoot
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputShootPrefix(PlayerInputController __instance, bool onHolding)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.UseItem))]
        private static class InputUseItem
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputUseItemPrefix(PlayerInputController __instance)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
        
        [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Walk))]
        private static class InputWalk
        {
            [HarmonyPrefix]
            // ReSharper disable InconsistentNaming UnusedMember.Local
            private static bool InputWalkPrefix(PlayerInputController __instance, bool onHolding)
                // ReSharper restore InconsistentNaming UnusedMember.Local
            {
                return !BlockInput;
            }
        }
    }
}