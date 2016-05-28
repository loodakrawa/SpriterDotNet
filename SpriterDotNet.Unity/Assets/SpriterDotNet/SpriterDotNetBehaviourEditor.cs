// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

namespace SpriterDotNetUnity
{
    [CustomEditor(typeof(SpriterDotNetBehaviour))]
    public class SpriterDotNetBehaviourEditor : Editor
    {
        string[] GetSortingLayerNames()
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SpriterDotNetBehaviour sdnb = target as SpriterDotNetBehaviour;

            string[] layers = GetSortingLayerNames();
            int currentIndex = Array.IndexOf(layers, sdnb.SortingLayer);
            if (currentIndex < 0) currentIndex = 0;
            int choiceIndex = EditorGUILayout.Popup("Sorting Layer", currentIndex, layers);
            sdnb.SortingLayer = layers[choiceIndex];
            sdnb.SortingOrder = EditorGUILayout.IntField("Sorting Order", sdnb.SortingOrder);
            EditorUtility.SetDirty(target);
        }
    }
}

#endif