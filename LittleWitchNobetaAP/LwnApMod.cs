using System.Collections;
using LittleWitchNobetaAP.Archipelago;
using LittleWitchNobetaAP.Patches;
using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP;

public class LwnApMod : MelonMod
{
    private const string ModDisplayInfo = $"{MyPluginInfo.PluginName} v{MyPluginInfo.PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    private static bool _showApConnectionGUI;

    private static float _guiScale = 1f;
    private static int _stringCursorPosition;
    private static bool _showPort;
    private static bool _showPassword;
    private static string _stringToEdit = "";

    private static readonly Dictionary<string, bool> EditingFlags = new()
    {
        { "Player", false },
        { "Hostname", false },
        { "Port", false },
        { "Password", false }
    };

    public static bool ShowApConnectionUI
    {
        get => _showApConnectionGUI;
        set
        {
            _showApConnectionGUI = value;
            if (_showApConnectionGUI)
                MelonEvents.OnGUI.Subscribe(DrawApMenu, 100); // The higher the value, the lower the priority.
            else
                MelonEvents.OnGUI.Unsubscribe(DrawApMenu);
        }
    }

    public static ArchipelagoClient? ArchipelagoClient { get; private set; }

    public override void OnInitializeMelon()
    {
        // Plugin startup logic
        ArchipelagoClient = new ArchipelagoClient();

        ArchipelagoConsole.OnInitialize();
        ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");

        MelonEvents.OnUpdate.Subscribe(Update);
    }

    public override void OnApplicationQuit()
    {
        var archipelagoSlotInfoCategory = MelonPreferences.GetCategory("LwnApSaveFileCategory");
        archipelagoSlotInfoCategory.Entries.Clear();
        archipelagoSlotInfoCategory.ResetFilePath();
    }

    public static IEnumerator RunOnMainThread(Action action)
    {
        yield return null; // Wait 1 frame to ensure we're on the main thread
        action.Invoke();
    }

    private void Update()
    {
        var submitKeyPressed = false;

        //handle text input
        if (Input.anyKeyDown
            && !Input.GetKeyDown(KeyCode.Return)
            && !Input.GetKeyDown(KeyCode.Escape)
            && !Input.GetKeyDown(KeyCode.Tab)
            && !Input.GetKeyDown(KeyCode.Backspace)
            && !Input.GetKeyDown(KeyCode.Delete)
            && !Input.GetKeyDown(KeyCode.LeftArrow)
            && !Input.GetKeyDown(KeyCode.RightArrow)
            && Input.inputString != ""
           )
        {
            //validation for any fields that require it
            var inputValid = !(EditingFlags["Port"] && !int.TryParse(Input.inputString, out _));

            if (inputValid)
            {
                _stringToEdit = _stringToEdit.Insert(_stringCursorPosition, Input.inputString);
                _stringCursorPosition++;
            }
        }

        //handle backspacing
        if (Input.GetKeyDown(KeyCode.Backspace))
            if (_stringToEdit.Length > 0 && _stringCursorPosition > 0)
            {
                _stringToEdit = _stringToEdit.Remove(_stringCursorPosition - 1, 1);
                _stringCursorPosition--;
            }

        //handle delete
        if (Input.GetKeyDown(KeyCode.Delete))
            if (_stringToEdit.Length > 0 && _stringCursorPosition < _stringToEdit.Length)
                _stringToEdit = _stringToEdit.Remove(_stringCursorPosition, 1);

        //handle cursor navigation
        if (Input.GetKeyDown(KeyCode.LeftArrow) && _stringCursorPosition > 0) _stringCursorPosition--;
        if (Input.GetKeyDown(KeyCode.RightArrow) && _stringCursorPosition < _stringToEdit.Length)
            _stringCursorPosition++;

        //update the relevant connection setting field
        Dictionary<string, bool> originalEditingFlags = new(EditingFlags);
        foreach (var editingFlag in originalEditingFlags.Where(editingFlag => editingFlag.Value))
        {
            SetConnectionSetting(editingFlag.Key, _stringToEdit);
            if (submitKeyPressed) FinishEditingTextField(editingFlag.Key);
        }
    }

    private static void DrawApMenu()
    {
        ArchipelagoConsole.OnGUI();

        if (ArchipelagoClient.IsAuthenticated) return;
        _guiScale = Screen.width <= 1280 && Screen.height <= 800 ? 0.75f : 1f;

        var apWindowRect = new Rect(20f, Screen.height * 0.12f, 430f * _guiScale, 540f * _guiScale);
        GUI.Window(101, apWindowRect, new Action<int>(ArchipelagoConfigEditorWindow), "Archipelago Connection");
    }

    private static void ArchipelagoConfigEditorWindow(int windowID)
    {
        GUI.skin.label.fontSize = (int)(20f * _guiScale);
        GUI.skin.button.fontSize = (int)(17f * _guiScale);

        // show the Archipelago Version and whether we're connected or not
        var statusMessage =
            ArchipelagoClient.IsAuthenticated ? " Status: Connected" : " Status: Disconnected";
        GUI.Label(new Rect(10f * _guiScale, 20f * _guiScale, 400f * _guiScale, 30f * _guiScale),
            APDisplayInfo + statusMessage);

        //Player name
        GUI.Label(new Rect(10f * _guiScale, 60f * _guiScale, 300f * _guiScale, 30f * _guiScale),
            $"Player: {TextWithCursor(GetConnectionSetting("Player"), EditingFlags["Player"], true)}");

        var editPlayer = GUI.Button(new Rect(10f * _guiScale, 100f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            EditingFlags["Player"] ? "Save" : "Edit");
        if (editPlayer) HandleEditButton("Player");

        var pastePlayer = GUI.Button(new Rect(100f * _guiScale, 100f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Paste");
        if (pastePlayer) HandlePasteButton("Player");

        var clearPlayer = GUI.Button(new Rect(190f * _guiScale, 100f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Clear");
        if (clearPlayer) HandleClearButton("Player");

        //Hostname
        GUI.Label(new Rect(10f * _guiScale, 160f * _guiScale, 300f * _guiScale, 30f * _guiScale),
            $"Host: {TextWithCursor(GetConnectionSetting("Hostname"), EditingFlags["Hostname"], true)}");

        var setLocalhost = GUI.Toggle(new Rect(160f * _guiScale, 200f * _guiScale, 90f * _guiScale, 30f * _guiScale),
            ArchipelagoClient.ServerData.Hostname == "localhost", "localhost");
        if (setLocalhost && ArchipelagoClient.ServerData.Hostname != "localhost")
            SetConnectionSetting("Hostname", "localhost");

        var setArchipelagoHost =
            GUI.Toggle(new Rect(10f * _guiScale, 200f * _guiScale, 140f * _guiScale, 30f * _guiScale),
                ArchipelagoClient.ServerData.Hostname == "archipelago.gg", "archipelago.gg");
        if (setArchipelagoHost && ArchipelagoClient.ServerData.Hostname != "archipelago.gg")
            SetConnectionSetting("Hostname", "archipelago.gg");

        var editHostname = GUI.Button(new Rect(10f * _guiScale, 240f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            EditingFlags["Hostname"] ? "Save" : "Edit");
        if (editHostname) HandleEditButton("Hostname");

        var pasteHostname = GUI.Button(new Rect(100f * _guiScale, 240f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Paste");
        if (pasteHostname) HandlePasteButton("Hostname");

        var clearHostname = GUI.Button(new Rect(190f * _guiScale, 240f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Clear");
        if (clearHostname) HandleClearButton("Hostname");

        //Port
        GUI.Label(new Rect(10f * _guiScale, 300f * _guiScale, 300f * _guiScale, 30f * _guiScale),
            $"Port: {TextWithCursor(GetConnectionSetting("Port"), EditingFlags["Port"], _showPort)}");

        _showPort = GUI.Toggle(new Rect(270f * _guiScale, 305f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            _showPort, "Show");

        var editPort = GUI.Button(new Rect(10f * _guiScale, 340f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            EditingFlags["Port"] ? "Save" : "Edit");
        if (editPort) HandleEditButton("Port");

        var pastePort = GUI.Button(new Rect(100f * _guiScale, 340f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Paste");
        if (pastePort) HandlePasteButton("Port");

        var clearPort = GUI.Button(new Rect(190f * _guiScale, 340f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Clear");
        if (clearPort) HandleClearButton("Port");

        //Password
        GUI.Label(new Rect(10f * _guiScale, 400f * _guiScale, 300f * _guiScale, 30f * _guiScale),
            $"Password: {TextWithCursor(GetConnectionSetting("Password"), EditingFlags["Password"], _showPassword)}");

        _showPassword = GUI.Toggle(new Rect(270f * _guiScale, 405f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            _showPassword, "Show");
        var editPassword = GUI.Button(new Rect(10f * _guiScale, 440f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            EditingFlags["Password"] ? "Save" : "Edit");
        if (editPassword) HandleEditButton("Password");

        var pastePassword = GUI.Button(new Rect(100f * _guiScale, 440f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Paste");
        if (pastePassword) HandlePasteButton("Password");

        var clearPassword = GUI.Button(new Rect(190f * _guiScale, 440f * _guiScale, 75f * _guiScale, 30f * _guiScale),
            "Clear");
        if (clearPassword) HandleClearButton("Password");

        var doConnect = GUI.Button(new Rect(190f * _guiScale, 480f * _guiScale, 150f * _guiScale, 30f * _guiScale),
            "Connect");
        if (doConnect) ArchipelagoClient?.Connect();

        var doClose = GUI.Button(new Rect(405f * _guiScale, 2f * _guiScale, 23f * _guiScale, 23f * _guiScale), "x");
        if (!doClose) return;
        ShowApConnectionUI = false;
        MovementPatches.BlockInput = false;
    }

    private static string TextWithCursor(string text, bool isEditing, bool showText)
    {
        var baseText = showText ? text : new string('*', text.Length);
        if (!isEditing) return baseText;
        if (_stringCursorPosition > baseText.Length) _stringCursorPosition = baseText.Length;
        return baseText.Insert(_stringCursorPosition, "<color=#EAA614>|</color>");
    }

    //Get a connection setting value by fieldname
    private static string GetConnectionSetting(string fieldName)
    {
        return fieldName switch
        {
            "Player" => ArchipelagoClient.ServerData.SlotName,
            "Hostname" => ArchipelagoClient.ServerData.Hostname,
            "Port" => ArchipelagoClient.ServerData.Port,
            "Password" => ArchipelagoClient.ServerData.Password,
            _ => ""
        };
    }

    //Set a connection setting value by fieldname
    private static void SetConnectionSetting(string fieldName, string value)
    {
        switch (fieldName)
        {
            case "Player":
                ArchipelagoClient.ServerData.SlotName = value;
                return;

            case "Hostname":
                ArchipelagoClient.ServerData.Hostname = value;
                return;

            case "Port":
                ArchipelagoClient.ServerData.Port = value;
                return;

            case "Password":
                ArchipelagoClient.ServerData.Password = value;
                return;

            default:
                return;
        }
    }

    //Clear all field editing flags (since we do this in a few places)
    private static void ClearAllEditingFlags()
    {
        List<string> fieldKeys = new(EditingFlags.Keys);
        foreach (var fieldKey in fieldKeys) EditingFlags[fieldKey] = false;
    }

    //Initialize a text field for editing
    private static void BeginEditingTextField(string fieldName)
    {
        if (EditingFlags[fieldName]) return; //can't begin if we're already editing this field

        //check and finalize if another field was mid-edit
        List<string> fieldKeys = new(EditingFlags.Keys);
        foreach (var fieldKey in fieldKeys.Where(fieldKey => EditingFlags[fieldKey])) FinishEditingTextField(fieldKey);

        _stringToEdit = GetConnectionSetting(fieldName);
        _stringCursorPosition = _stringToEdit.Length;
        EditingFlags[fieldName] = true;
    }

    //finalize editing a text field and save the changes
    private static void FinishEditingTextField(string fieldName)
    {
        if (!EditingFlags[fieldName]) return; //can't finish if we're not editing this field

        _stringToEdit = "";
        _stringCursorPosition = 0;
        EditingFlags[fieldName] = false;
    }

    private static void HandleEditButton(string fieldName)
    {
        if (EditingFlags[fieldName])
            FinishEditingTextField(fieldName);
        else
            BeginEditingTextField(fieldName);
    }

    private static void HandlePasteButton(string fieldName)
    {
        SetConnectionSetting(fieldName, GUIUtility.systemCopyBuffer);
        if (!EditingFlags[fieldName]) return;
        _stringToEdit = GUIUtility.systemCopyBuffer;
        FinishEditingTextField(fieldName);
    }

    private static void HandleClearButton(string fieldName)
    {
        SetConnectionSetting(fieldName, "");
        if (EditingFlags[fieldName]) _stringToEdit = "";
    }
}