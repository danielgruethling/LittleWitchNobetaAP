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
    
    public static String GetObjectPath(GameObject obj) {
        string path = "/" + obj.name;
        Transform current = obj.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = "/" + current.name + path;
        }
        return path;
    }
    
    public static GameObject? FindObjectByPath(string path)
    {
        var root = GameObject.Find(path.Split('/')[1]); 
        if (root is null) return null;
        
        if (path.Split('/').Length == 2) return root;
    
        var transform = root.transform.Find(path.Substring(path.IndexOf('/', 1) + 1));
        return transform?.gameObject;
    }
}