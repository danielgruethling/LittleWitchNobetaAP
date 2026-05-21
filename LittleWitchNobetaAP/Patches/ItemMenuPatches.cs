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
            if (itemsTemplate is null) {
                Melon<LwnApMod>.Logger.Error("Failed to get UIValuablesGuide canvas to clone.");
                return;
            }
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
            if (ArchipelagoClient.ServerData.Settings?.RandomizeLoreEnabled ==
                ArchipelagoSettings.RandomizeLore.ChecksOnly
                || ArchipelagoClient.ServerData.Settings?.RandomizeLoreEnabled ==
                ArchipelagoSettings.RandomizeLore.NoLore)
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
            var selectedItem = __instance.navigator.currentHandler?.name;
            if (!ArchipelagoClient.IsAuthenticated || ArchipelagoClient.Session is null)
                return selectedItem != "Valuables";
            if (selectedItem == "Valuables"
                && (ArchipelagoClient.ServerData.Settings?.RandomizeLoreEnabled ==
                    ArchipelagoSettings.RandomizeLore.NoLore))
            {
                Game.AppearEventPrompt("Lore checks have been disabled.");
                return false;
            }
            return true;
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
            if (template is null) {
                Melon<LwnApMod>.Logger.Error("Failed to get Valuables UI handler to clone.");
                return;
            }
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
            resume.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 145f);
            magicStats.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 85f);
            valuables.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 25f);
            newChild.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -35f);
            settings.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -95f);
            gameStats.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -155f);
            reload.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -215f);

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
        }
        
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local
        private static void MarkItemMenusAsDirty(UIPauseMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            var canvasRoot = __instance.transform.parent;
            var originalMenu = canvasRoot.Find("UIValuablesGuide");
            var injectedMenu = canvasRoot.Find("UIApItemGuide");
            originalMenu.GetComponent<UIValuablesGuide>().isDirty = true;
            injectedMenu.GetComponent<UIValuablesGuide>().isDirty = true;
        }
    }
}