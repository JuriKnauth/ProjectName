#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Configs = CompanyName.ProductName.Scripts.Runtime.Configurations;

public static class SettingsContextMenu
{
    [MenuItem("CONTEXT/Settings/Add Missing Sections")]
    private static void AddMissingSections(MenuCommand cmd)
    {
        Configs.Settings root = cmd.context as Configs.Settings;
        if (root == null) return;

        string path = AssetDatabase.GetAssetPath(root);
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog(
                "Save the asset first",
                "Save your Settings.asset to disk, then try again.",
                "OK");

            return;
        }

        SerializedObject so = new SerializedObject(root);
        SerializedProperty
            sectionsProp = so.FindProperty("sections");

        HashSet<Type> existing = new HashSet<Type>();
        if (sectionsProp != null)
        {
            for (int i = 0; i < sectionsProp.arraySize; i++)
            {
                Configs.SettingsSection elem =
                    sectionsProp.GetArrayElementAtIndex(i).objectReferenceValue as Configs.SettingsSection;

                if (elem) existing.Add(elem.GetType());
            }
        }

        IEnumerable<Type> all = TypeCache.GetTypesDerivedFrom<Configs.SettingsSection>()
                                         .Where(t => !t.IsAbstract && !t.IsGenericType);

        int created = 0;
        foreach (Type t in all)
        {
            if (existing.Contains(t)) continue;

            Configs.SettingsSection section = ScriptableObject.CreateInstance(t) as Configs.SettingsSection;
            section.name = t.Name;

            AssetDatabase.AddObjectToAsset(section, root);
            sectionsProp.InsertArrayElementAtIndex(sectionsProp.arraySize);
            sectionsProp.GetArrayElementAtIndex(sectionsProp.arraySize - 1).objectReferenceValue = section;

            created++;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        if (created > 0)
        {
            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Settings] Added {created} missing section(s) to {path}");
        }
        else
        {
            Debug.Log("[Settings] No missing sections.");
        }
    }
}
#endif