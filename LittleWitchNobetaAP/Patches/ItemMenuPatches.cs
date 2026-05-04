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

public class ItemMenuPatches
{
    public static bool IsOnInjectedMenu = false;

    [HarmonyPatch(typeof(GameUIManager), nameof(GameUIManager.Init))]
    private static class GameUIManagerInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void UIManagerAddNewWindow(GameUIManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var root = __instance.transform;
            var canvas = root.Find("Canvas");
            var itemsTemplate = canvas.Find("UIValuablesGuide");
            var menuCopy = Object.Instantiate(itemsTemplate, canvas);
            menuCopy.name = "UIApItemGuide";
            var valuablesHandlerRoot = menuCopy.Find("ValuablesHandlersRoot");
            for (var i = valuablesHandlerRoot.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(valuablesHandlerRoot.GetChild(i).gameObject);
            }

            var copiedUIClass = menuCopy.GetComponent<UIValuablesGuide>();
            menuCopy.Find("Title").GetComponent<Text>().text = "Lore Received";
            menuCopy.Find("Title").GetComponent<Text>().SetLayoutDirty();
            copiedUIClass.Init();
        }

        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void ChangeOriginalItemMenuName(GameUIManager __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var root = __instance.transform;
            var canvas = root.Find("Canvas");
            var itemMenu = canvas.Find("UIValuablesGuide");
            itemMenu.Find("Title").GetComponent<Text>().text = "Checked Lore Locations";
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Submit))]
    private static class UIPauseMenuSubmit
    {
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool UIPauseMenuHandleSubmit(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var selectedItem = __instance.navigator.currentHandler?.name;
            if (selectedItem != "APItems") return true;
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null) return false;
            if (ArchipelagoClient.ServerData.Settings?.RandomizeLoreEnabled == ArchipelagoSettings.RandomizeLore.NoLore)
            {
                Game.AppearEventPrompt("Lore items have been disabled.");
                return false;
            }

            if (Singletons.GameSave?.props.GetPropCollectionAmount() <= 0)
            {
                Game.AppearEventPrompt("No lore items received yet.");
                return false;
            }

            Melon<LwnApMod>.Logger.Msg($"APItems Detected!");
            var apItemUI = __instance.transform.parent.Find("UIApItemGuide");

            __instance.HideRootCanvas();
            __instance.OnSubPauseMenuAppeared();
            var newCanvas = apItemUI.GetComponent<UIValuablesGuide>();
            IsOnInjectedMenu = true;
            newCanvas.Open(DelegateSupport.ConvertDelegate<Action>(() =>
            {
                Melon<LwnApMod>.Logger.Msg("AP Items Menu Closed");
                __instance.AppearRootCanvas(null);
                IsOnInjectedMenu = false;
            }));
            return false;
        }

        // Only allow clicks on AP Lore menu items when AP is connected.
        // This is because the items are loaded when initially clicked, and this can break which items are shown
        // if this is done before the AP session begins.
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static bool UIPauseMenuPreventAPMenuLoadWhenDisconnected(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            if (ArchipelagoClient.IsAuthenticated && ArchipelagoClient.Session is not null) return true;
            var selectedItem = __instance.navigator.currentHandler?.name;
            return selectedItem != "Valuables";
        }
    }
    
    // Disables IsOnInjectedMenu flag as the normal closed handler for the injected menu doesn't run
    // if user hits "escape" to exit menu.
    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Hide))]
    private static class UIPauseMenuHide
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void DisableFlag(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            IsOnInjectedMenu = false;
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Init))]
    private static class UIPauseMenuInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void UIPauseMenuAddAPItems(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var root = __instance.transform;
            var handlers = root.Find("Handlers");
            var template = handlers.Find("Valuables");
            var newChild = Object.Instantiate(template, handlers);
            var newChildUiHandler = newChild.GetComponent<UILabelHandler>();
            newChild.name = "APItems";
            newChild.Find("Label").GetComponent<Text>().text = "AP Lore Received";
            newChildUiHandler.index = 7;

            var internalArray = __instance.handlers;
            var newArray = new Il2CppReferenceArray<UILabelHandler>(internalArray.Length + 1);
            for (var i = 0; i < internalArray.Length; i++)
            {
                newArray[i] = internalArray[i];
            }

            newArray[internalArray.Length] = newChildUiHandler;
            __instance.handlers = newArray;

            var resume = handlers.Find("Resume");
            var magicStats = handlers.Find("MagicStats");
            var valuables = handlers.Find("Valuables");
            var settings = handlers.Find("Settings");
            var gameStats = handlers.Find("GameStats");
            var reload = handlers.Find("Reload");
            var quit = handlers.Find("Quit");

            // Move menu items to make room for new AP Items option
            resume.transform.position =
                new Vector3(gameStats.transform.position.x, 1370f, gameStats.transform.position.z);
            magicStats.transform.position =
                new Vector3(gameStats.transform.position.x, 1250f, gameStats.transform.position.z);
            valuables.transform.position =
                new Vector3(gameStats.transform.position.x, 1130f, gameStats.transform.position.z);
            newChild.transform.position =
                new Vector3(newChild.transform.position.x, 1010f, newChild.transform.position.z);
            settings.transform.position =
                new Vector3(gameStats.transform.position.x, 890f, gameStats.transform.position.z);
            gameStats.transform.position =
                new Vector3(gameStats.transform.position.x, 770f, gameStats.transform.position.z);
            reload.transform.position = new Vector3(reload.transform.position.x, 650f, reload.transform.position.z);

            // Make the option interact with mouse and keyboard/controller
            var valuablesUiHandler = valuables.GetComponent<UILabelHandler>();
            var settingsUiHandler = settings.GetComponent<UILabelHandler>();
            valuablesUiHandler.selectDown = newChildUiHandler;
            settingsUiHandler.selectUp = newChildUiHandler;
            newChildUiHandler.selectUp = valuablesUiHandler;
            newChildUiHandler.selectDown = settingsUiHandler;
            newChildUiHandler.onDeselectedHandler = valuablesUiHandler.onDeselectedHandler;
            newChildUiHandler.onSelectedHandler = valuablesUiHandler.onSelectedHandler;
            newChildUiHandler.pointerEnterHandler = valuablesUiHandler.pointerEnterHandler;
            newChildUiHandler.pointerExitHandler = valuablesUiHandler.pointerExitHandler;
            newChild.GetComponent<RectTransform>().sizeDelta = new Vector2(225f, 70f);

            Melon<LwnApMod>.Logger.Msg("Patched UI Pause menu");
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Appear))]
    private static class UIPauseMenuAppear
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void UIPauseMenuChangeItemsName(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var root = __instance.transform;
            var handlers = root.Find("Handlers");
            var template = handlers.Find("Valuables");
            template.GetComponent<UILabelHandler>().SetLabel("AP Lore Checks");
            template.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 70f);
            Melon<LwnApMod>.Logger.Msg("Changed item menu name");
        }
    }
}