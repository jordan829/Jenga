////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System;
using zSpace.Common;

/// <summary> Helper class for highlighting selected objects. </summary>
public class ObjectHighlighter : MonoBehaviour
{
    /// <summary>
    /// The material that will be applied in addition to an object's normal material when it is hovered by the stylus.
    /// </summary>
    public Material hoveredMaterial;

    /// <summary>
    /// The material that will be applied in addition to an object's normal material when it is selected by the stylus.
    /// </summary>
    public Material selectedMaterial;

    protected ZSStylusSelector _stylusSelector;

    void Awake()
    {
        _stylusSelector = GameObject.Find("ZSStylusSelector").GetComponent<ZSStylusSelector>();
    }


    void OnPostRender()
    {
        bool isButtonDown = false;
        for (int i = 0; i < _stylusSelector.numButtons; ++i)
            isButtonDown |= _stylusSelector.GetButton(i);

        if (!isButtonDown)
        {
			Utility.RenderMeshes(_stylusSelector.HoverObject, hoveredMaterial);

            foreach (GameObject selectedObject in _stylusSelector.selectedObjects)
                Utility.RenderMeshes(selectedObject, selectedMaterial);
        }
    }



}
