using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP;

public class LwnApMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        MelonEvents.OnGUI.Subscribe(DrawApMenu, 100); // The higher the value, the lower the priority.
    }
    
    private void DrawApMenu()
    {
        GUI.Box(new Rect(0, 0, 300, 500), "My Menu");
    }
}