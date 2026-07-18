using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;

public class SPUMComponentCopier : EditorWindow
{
    private GameObject sourceObject;
    public List<GameObject> targetObjects = new List<GameObject>();
    private bool copyValuesIfPresent = true;
    private bool replaceExistingComponents = false;
    private bool skipTransform = true;
    
    [MenuItem("Tools/Abyss Frontier/SPUM Component Copier")]
    public static void ShowWindow()
    {
        GetWindow<SPUMComponentCopier>("SPUM Component Copier");
    }

    private void OnGUI()
    {
        GUILayout.Label("Source Object (has fully configured components)", EditorStyles.boldLabel);
        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Character", sourceObject, typeof(GameObject), true);
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Target Objects (to copy components to)", EditorStyles.boldLabel);
        
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("targetObjects");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        skipTransform = EditorGUILayout.Toggle(new GUIContent("Skip Transform", "Do not copy Transform values (Position, Rotation, Scale)"), skipTransform);
        copyValuesIfPresent = EditorGUILayout.Toggle(new GUIContent("Copy Values if Present", "If the target already has the component, copy the values from the source over to it."), copyValuesIfPresent);
        replaceExistingComponents = EditorGUILayout.Toggle(new GUIContent("Replace Existing", "If the target already has the component, destroy it and add a fresh copy from the source."), replaceExistingComponents);
        
        EditorGUILayout.Space();
        
        GUI.enabled = sourceObject != null && targetObjects.Count > 0;
        
        if (GUILayout.Button("Copy Components to Targets", GUILayout.Height(30)))
        {
            CopyComponents();
        }
        
        GUI.enabled = true;
    }

    private void CopyComponents()
    {
        if (sourceObject == null) return;

        Component[] sourceComponents = sourceObject.GetComponents<Component>();

        int targetCount = 0;
        foreach (GameObject target in targetObjects)
        {
            if (target == null || target == sourceObject) continue;

            Undo.RegisterFullObjectHierarchyUndo(target, "Copy SPUM Components");

            foreach (Component sourceComp in sourceComponents)
            {
                if (sourceComp == null)
                {
                    Debug.LogWarning($"Source object {sourceObject.name} has a missing script!");
                    continue;
                }

                if (skipTransform && sourceComp is Transform) continue;

                Type compType = sourceComp.GetType();
                Component[] targetComps = target.GetComponents(compType);

                if (targetComps.Length > 0)
                {
                    if (replaceExistingComponents)
                    {
                        foreach (Component c in targetComps)
                        {
                            Undo.DestroyObjectImmediate(c);
                        }
                        ComponentUtility.CopyComponent(sourceComp);
                        ComponentUtility.PasteComponentAsNew(target);
                    }
                    else if (copyValuesIfPresent)
                    {
                        ComponentUtility.CopyComponent(sourceComp);
                        foreach (Component c in targetComps)
                        {
                            ComponentUtility.PasteComponentValues(c);
                        }
                    }
                }
                else
                {
                    // Target does not have this component, add it
                    ComponentUtility.CopyComponent(sourceComp);
                    ComponentUtility.PasteComponentAsNew(target);
                }
            }
            
            // If the target is a prefab, mark it dirty
            EditorUtility.SetDirty(target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            targetCount++;
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green>Successfully copied components from {sourceObject.name} to {targetCount} targets.</color>");
    }
}
