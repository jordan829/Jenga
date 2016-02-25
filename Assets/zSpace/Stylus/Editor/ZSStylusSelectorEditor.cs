////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor for the ZSStylusSelector class. Allows the developer to set an active stylus shape.
/// </summary>
[CustomEditor(typeof(ZSStylusSelector))]
public class ZSStylusSelectorEditor : Editor
{	
    public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
        _stylusSelector = (ZSStylusSelector)this.target;
		bool isSceneObject = !EditorUtility.IsPersistent(_stylusSelector);
		_stylusSelector.activeStylus = (ZSStylusShape)EditorGUILayout.ObjectField("Active Stylus", _stylusSelector.activeStylus, typeof(ZSStylusShape), isSceneObject);
		EditorUtility.SetDirty(_stylusSelector);
	}
	
	ZSStylusSelector _stylusSelector;
}