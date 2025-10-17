using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP.Archipelago;

// shamelessly stolen from oc2-modding https://github.com/toasterparty/oc2-modding/blob/main/OC2Modding/GameLog.cs
public static class ArchipelagoConsole
{
    private const int MaxLogLines = 80;
    private const float HideTimeout = 15f;
    private static bool _hidden = true;

    private static readonly List<string> LogLines = new();
    private static Vector2 _scrollView;
    private static Rect _window;
    private static Rect _scroll;
    private static Rect _text;
    private static Rect _hideShowButton;

    private static readonly GUIStyle TextStyle = new();
    private static string _scrollText = "";
    private static float _lastUpdateTime = Time.time;

    private static string _commandText = "!help";
    private static Rect CommandTextRect;
    private static Rect SendCommandButton;

    public static void OnInitialize()
    {
        UpdateWindow();
    }

    public static void LogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (LogLines.Count == MaxLogLines) LogLines.RemoveAt(0);
        LogLines.Add(message);
        Melon<LwnApMod>.Logger.Msg(message);
        _lastUpdateTime = Time.time;
        UpdateWindow();
    }

    public static void OnGUI()
    {
        if (LogLines.Count == 0) return;

        if (!_hidden || Time.time - _lastUpdateTime < HideTimeout)
        {
            _scrollView = GUI.BeginScrollView(_window, _scrollView, _scroll);
            GUI.Box(_text, "");
            GUI.Box(_text, _scrollText, TextStyle);
            GUI.EndScrollView();
        }

        if (GUI.Button(_hideShowButton, _hidden ? "Show Console" : "Hide Console"))
        {
            _hidden = !_hidden;
            UpdateWindow();
        }

        // draw client/server commands entry
        if (_hidden || !ArchipelagoClient.IsAuthenticated) return;

        /*CommandText = GUI.TextField(CommandTextRect, CommandText);
        if (!CommandText.IsNullOrWhiteSpace() && GUI.Button(SendCommandButton, "Send"))
        {
            ApHandler.ArchipelagoClient.SendMessage(CommandText);
            CommandText = "";
        }*/
    }

    public static void UpdateWindow()
    {
        _scrollText = "";

        if (_hidden)
        {
            if (LogLines.Count > 0) _scrollText = LogLines[^1];
        }
        else
        {
            for (var i = 0; i < LogLines.Count; i++)
            {
                _scrollText += "> ";
                _scrollText += LogLines.ElementAt(i);
                if (i < LogLines.Count - 1) _scrollText += "\n\n";
            }
        }

        var width = (int)(Screen.width * 0.4f);
        int height;
        int scrollDepth;
        if (_hidden)
        {
            height = (int)(Screen.height * 0.03f);
            scrollDepth = height;
        }
        else
        {
            height = (int)(Screen.height * 0.3f);
            scrollDepth = height * 10;
        }

        _window = new Rect(Screen.width / 2 - width / 2, 0, width, height);
        _scroll = new Rect(0, 0, width * 0.9f, scrollDepth);
        _scrollView = new Vector2(0, scrollDepth);
        _text = new Rect(0, 0, width, scrollDepth);

        TextStyle.alignment = TextAnchor.LowerLeft;
        TextStyle.fontSize = _hidden ? (int)(Screen.height * 0.0165f) : (int)(Screen.height * 0.0185f);
        TextStyle.normal.textColor = Color.white;
        TextStyle.wordWrap = !_hidden;

        var xPadding = (int)(Screen.width * 0.01f);
        var yPadding = (int)(Screen.height * 0.01f);

        TextStyle.padding = _hidden
            ? new RectOffset(xPadding / 2, xPadding / 2, yPadding / 2, yPadding / 2)
            : new RectOffset(xPadding, xPadding, yPadding, yPadding);

        var buttonWidth = (int)(Screen.width * 0.12f);
        var buttonHeight = (int)(Screen.height * 0.03f);

        _hideShowButton = new Rect(Screen.width / 2 + width / 2 + buttonWidth / 3, Screen.height * 0.004f, buttonWidth,
            buttonHeight);

        // draw server command text field and button
        width = (int)(Screen.width * 0.4f);
        var xPos = (int)(Screen.width / 2.0f - width / 2.0f);
        var yPos = (int)(Screen.height * 0.307f);
        height = (int)(Screen.height * 0.022f);

        CommandTextRect = new Rect(xPos, yPos, width, height);

        width = (int)(Screen.width * 0.035f);
        yPos += (int)(Screen.height * 0.03f);
        SendCommandButton = new Rect(xPos, yPos, width, height);
    }
}