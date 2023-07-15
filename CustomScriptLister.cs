#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CustomScriptLister : EditorWindow
{
    private GameObject selectedGameObject;
    private Dictionary<MonoScript, List<GameObject>> customScripts = new Dictionary<MonoScript, List<GameObject>>();
    private Dictionary<MonoScript, int> scriptLineCounts = new Dictionary<MonoScript, int>();
    private Vector2 scrollPosition;

    [MenuItem("Window/Custom Script Lister")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CustomScriptLister));
    }

    private void OnGUI()
    {
        selectedGameObject = EditorGUILayout.ObjectField("Selected GameObject", selectedGameObject, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("List Custom Scripts"))
        {
            customScripts.Clear();
            scriptLineCounts.Clear();

            if (selectedGameObject != null)
            {
                CollectCustomScripts(selectedGameObject);
                CalculateScriptLineCounts();
            }
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var pair in customScripts)
        {
            EditorGUILayout.LabelField(pair.Key.name, EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var reference in pair.Value)
            {
                EditorGUILayout.ObjectField("Attached to", reference, typeof(GameObject), true);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Line Count", scriptLineCounts[pair.Key].ToString());
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
    }

    private void CollectCustomScripts(GameObject gameObject)
    {
        var scripts = gameObject.GetComponents<MonoBehaviour>();

        foreach (var script in scripts)
        {
            var scriptAsset = MonoScript.FromMonoBehaviour(script);
            if (!IsUnityScript(scriptAsset))
            {
                if (customScripts.ContainsKey(scriptAsset))
                {
                    if (!customScripts[scriptAsset].Contains(gameObject))
                    {
                        customScripts[scriptAsset].Add(gameObject);
                    }
                }
                else
                {
                    customScripts.Add(scriptAsset, new List<GameObject> { gameObject });
                }
            }
        }

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            var child = gameObject.transform.GetChild(i);
            CollectCustomScripts(child.gameObject);
        }
    }

    private void CalculateScriptLineCounts()
    {
        foreach (var script in customScripts.Keys)
        {
            int lineCount = CountScriptLines(script);
            scriptLineCounts.Add(script, lineCount);
        }
    }

    private int CountScriptLines(MonoScript script)
    {
        string scriptPath = AssetDatabase.GetAssetPath(script);
        FileInfo fileInfo = new FileInfo(scriptPath);
        int lineCount = 0;

        using (StreamReader reader = fileInfo.OpenText())
        {
            while (reader.ReadLine() != null)
            {
                lineCount++;
            }
        }

        return lineCount;
    }

    private bool IsUnityScript(MonoScript script)
    {
        var scriptAssembly = script.GetClass().Assembly;
        return scriptAssembly.FullName.StartsWith("UnityEngine") ||
               scriptAssembly.FullName.StartsWith("UnityEditor");
    }
}
#endif
