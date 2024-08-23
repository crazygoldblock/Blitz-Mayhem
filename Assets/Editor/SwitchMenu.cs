using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script který se používá pro měnění oken v menu při práci v Unity editoru 
/// Přidá do inspectoru v scriptu Menu seznam všech oken
/// při vybrání okna dané okno zobrazí a aktuálně zobrazené okno vypne  
/// </summary>
[CustomEditor(typeof(Menu))]
public class SwitchMenu : Editor
{
    public List<string> options = new();
    public int index = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (options == null || options.Count == 0 )
        {
            UpdateOptions();
        }

        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        GUILayout.Label("Editor - Switch Menu");

        int temp = EditorGUILayout.Popup(index, options.ToArray());

        if (temp != index)
        {
            index = temp;

            ((Menu)target).SwitchTab(index);

            UpdateOptions();
        }
    }
    void UpdateOptions()
    {
        Menu m = (Menu)target;

        m.tabs.Clear();

        foreach (Transform child in GameObject.Find("Canvas").transform)
        {
            m.tabs.Add(child.gameObject);
            options.Add(child.name);
        }
    }
}
