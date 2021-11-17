using UnityEditor;
using UnityEngine;

public class ExplorerEditor : Editor
{
    [MenuItem("Tools/Open")]
    private static void Open()
    {
        string path = Application.persistentDataPath;
        System.Diagnostics.Process.Start(path);
    }

    [MenuItem("Tools/Select")]
    private static void Select()
    {
        string src = Application.persistentDataPath;
        string dst = EditorUtility.OpenFilePanel("Select png", src, "png");
        Debug.Log($"dst={dst}");
    }
}