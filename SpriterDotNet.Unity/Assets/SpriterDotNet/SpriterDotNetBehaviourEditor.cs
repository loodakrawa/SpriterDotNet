// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace SpriterDotNetUnity
{
    [CustomEditor(typeof(SpriterDotNetBehaviour))]
    public class SpriterDotNetBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SpriterDotNetBehaviour sdnb = target as SpriterDotNetBehaviour;

            string[] layers = SortingLayer.layers.Select(l => l.name).ToArray();
            int currentIndex = Array.IndexOf(layers, sdnb.SortingLayer);
            if (currentIndex < 0) currentIndex = 0;
            int choiceIndex = EditorGUILayout.Popup("Sorting Layer", currentIndex, layers);
            sdnb.SortingLayer = layers[choiceIndex];
            sdnb.SortingOrder = EditorGUILayout.IntField("Sorting Order", sdnb.SortingOrder);
			
			if (GUI.changed)
            {
                EditorUtility.SetDirty(sdnb);
                EditorSceneManager.MarkSceneDirty(sdnb.gameObject.scene);
            }
        }
    }
}

#endif
