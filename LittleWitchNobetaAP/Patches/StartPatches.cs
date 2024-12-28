using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Linq.Expressions.Interpreter;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWitchNobetaAP.Patches;

public static class StartPatches
{
    public const int GameSaveIndex = 9;

    public static Text? CopyrightText { get; private set; }
    private static string? GameVersionText { get; set; }
    private static string? RandomizerVersionText { get; set; }

    private const string PluginVersion = "v0.2.0";

    [HarmonyPatch(typeof(UIOpeningMenu), nameof(UIOpeningMenu.Init))]
    private static class UIOpeningMenuInit
    {
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming __instance is needed for Harmony self reference
        private static void OpeningMenuInitPostfix(UIOpeningMenu __instance)
            // ReSharper restore InconsistentNaming
        {
            var newGameObject = __instance.transform.Find("Foreground/NewGame").gameObject;

            if (newGameObject != null)
            {
                var newGameUIHandler = newGameObject.GetComponent<UILabelHandler>();
                var newGameLabel = newGameObject.GetComponentInChildren<Text>();

                var loadGameObject = __instance.transform.Find("Foreground/LoadGame").gameObject;
                var optionsUIHandler = __instance.transform.Find("Foreground/Option").gameObject
                    .GetComponent<UILabelHandler>();

                Melon<LwnApMod>.Logger.Msg("Found 'New Game' button");

                newGameLabel.text = "New Randomizer";

                loadGameObject.GetComponentInChildren<Text>().text = "Resume Run";
                // Remove Load button if no run is resume-able
                /*else
                {
                    Object.Destroy(loadGameObject);

                    // Reorder UI elements
                    newGameUIHandler.selectDown = optionsUIHandler;
                    optionsUIHandler.selectUp = newGameUIHandler;
                }*/

                // Replace copyright text to add seed hash
                var copyrightGameObject = __instance.transform.Find("Foreground/Copyright").gameObject;
                copyrightGameObject.transform.Translate(0, 5, 0);

                CopyrightText = copyrightGameObject.GetComponent<Text>();

                // Add randomizer plugin version next to game version
                var versionGameObject = __instance.transform.Find("Foreground/Version").gameObject;
                versionGameObject.transform.Translate(0, 5, 0);

                var versionText = versionGameObject.GetComponent<Text>();

                GameVersionText = versionText.text;
                RandomizerVersionText = $"Ver {PluginVersion}";

                versionText.text = $"Game {GameVersionText} Randomizer {RandomizerVersionText}";
            }
            else
            {
                Melon<LwnApMod>.Logger.Error(
                    "Couldn't find 'New Game' button, check that you are using the correct game and randomizer version!");
                Application.Quit();
            }
        }
    }
}