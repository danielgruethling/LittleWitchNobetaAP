using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LittleWitchNobetaAP.Utils;

public static class UnityUtils
{
    public static GameObject? FindGameObjectByNameForced(string name)
    {
        return Object.FindObjectsOfType<GameObject>(true)
            .FirstOrDefault(gameObject => gameObject.name == name);
    }

    public static Il2CppArrayBase<TComponent> FindComponentsByTypeForced<TComponent>() where TComponent : Component
    {
        return Object.FindObjectsOfType<TComponent>(true);
    }

    public static TComponent? FindComponentByNameForced<TComponent>(string name)
    {
        var gameObject = FindGameObjectByNameForced(name);

        return gameObject != null ? gameObject.GetComponent<TComponent>() : default;
    }
}