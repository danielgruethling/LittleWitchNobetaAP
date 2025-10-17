using HarmonyLib;
using Il2Cpp;
using UnityEngine.UI;

namespace LittleWitchNobetaAP.Patches;

public static class StartPatches
{
    private const string PluginVersion = "0.2.1";
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
}