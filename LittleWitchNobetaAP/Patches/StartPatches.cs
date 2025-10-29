using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine.UI;

namespace LittleWitchNobetaAP.Patches;

public static class StartPatches
{
    private const string PluginVersion = "0.2.2";
    private static string? GameVersionText { get; set; }
    private static string? RandomizerVersionText { get; set; }

    [HarmonyPatch(typeof(UIOpeningMenu), nameof(UIOpeningMenu.Init))]
    private static class UIOpeningMenuInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming UnusedMember.Local __instance is needed for Harmony self reference
        private static void OpeningMenuInitPostfix(UIOpeningMenu __instance)
            // ReSharper restore InconsistentNaming UnusedMember.Local
        {
            // Add randomizer plugin version next to game version
            var versionGameObject = __instance.transform.Find("Foreground/Version").gameObject;
            versionGameObject.transform.Translate(0, 5, 0);

            var versionText = versionGameObject.GetComponent<Text>();

            GameVersionText = versionText.text;
            RandomizerVersionText = $"Ver {PluginVersion}";

            versionText.text = $"Game: {GameVersionText} Randomizer: {RandomizerVersionText}";
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Init))]
    private static class UIPauseMenuInit
    {
        [HarmonyPostfix]
        private static void PauseMenuInitPostfix(UIPauseMenu __instance)
        {
            for (int i = 0; i < __instance.transform.childCount; i++)
            {
                var child = __instance.transform.GetChild(i).gameObject;
                Melon<LwnApMod>.Logger.Msg($"PauseItem: {child.name}");
            }
            
            var handlers = __instance.transform.Find("Handlers");
            for (int i = 0; i < handlers.childCount; i++)
            {
                var child = handlers.GetChild(i).gameObject;
                Melon<LwnApMod>.Logger.Msg($"HandlerItem: {child.name}");
            }
            
            var reload = __instance.transform.Find("Handlers/Reload").gameObject;
            var gameStats = __instance.transform.Find("Handlers/GameStats").gameObject.GetComponent<UILabelHandler>();
            var quit = __instance.transform.Find("Handlers/Quit").gameObject.GetComponent<UILabelHandler>();
            UnityEngine.Object.Destroy(reload.gameObject);
            gameStats.selectDown = quit;
            quit.selectUp = gameStats;
        }
    }
}