////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using zSpace.Common;

[RequireComponent(typeof(ZSStylusShape))]

/// <summary>
/// Stylus tool for scaling objects.
/// </summary>
public class ZSScaleTool : ZSStylusTool
{
    /// <summary>
    /// The rate of scaling when the user drags the stylus along the X or Z axis.
    /// </summary>
    public float _scaleRate = 5.0f;
    protected Vector3 _focusCenter;
    protected Vector3 _stylusSpaceContact;
    protected float _startX;
    protected float _startZ;
    protected float _oldScale = 1.0f;
    protected Vector3[] _startScales;
    protected Vector3[] _startOffsets;
    protected ZSCore _core;
    protected Camera _camera;

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        _core = GameObject.Find("ZSCore").GetComponent<ZSCore>();
        _camera = _core.CurrentCamera.GetComponent<Camera>();
    }
  
    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSScaleTool";
    }

    public override void OnStylus()
    {
        base.OnStylus();

        bool isButtonDown = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonDown(buttonId));
        if (isButtonDown && _stylusSelector.HoverObject != null)
            ScaleBegin();

        bool isButton = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
        if (isButton)
            ScaleStay();

        bool isButtonUp = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonUp(buttonId));
        if (isButtonUp)
            ScaleEnd();
    }
 
    protected void ScaleBegin()
    {
        // Initiate scaling.

        _startScales = new Vector3[_stylusSelector.selectedObjects.Count];
        _startOffsets = new Vector3[_stylusSelector.selectedObjects.Count];
        Bounds focusBounds = new Bounds();
        foreach (GameObject selectedObject in _stylusSelector.selectedObjects)
        {
            _focusObjects.Add(selectedObject);

			Bounds objectBounds = ZSScaleTool.ComputeBounds(selectedObject.transform);
            if (focusBounds.extents == Vector3.zero)
                focusBounds = objectBounds;
            else
                focusBounds.Encapsulate(objectBounds);
        }

        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];
            _startScales[i] = focusObject.transform.localScale;
            _startOffsets[i] = focusObject.transform.position - focusBounds.center;
        }

        _focusCenter = focusBounds.center;
        _startX = Vector3.Dot(_stylusSelector.HoverPoint, _camera.transform.right);
        _startZ = Vector3.Dot(_stylusSelector.HoverPoint, _camera.transform.forward);

        _stylusSpaceContact = _stylusSelector.transform.InverseTransformPoint(_stylusSelector.HoverPoint);

        ToolBegin();
    }

	public static Bounds ComputeBounds (Transform transform, bool useVisibleOnly = true)
	{
		GameObject gameObject = transform.gameObject;
		Bounds result = default(Bounds);
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer> (useVisibleOnly);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = componentsInChildren [i];
			if (!useVisibleOnly || renderer.enabled)
			{
				if (result.extents == Vector3.zero)
				{
					result = renderer.bounds;
				}
				else
				{
					result.Encapsulate (renderer.bounds);
				}
			}
		}
		if (!useVisibleOnly)
		{
			Collider[] componentsInChildren2 = gameObject.GetComponentsInChildren<Collider> (true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				Collider collider = componentsInChildren2 [i];
				if (result.extents == Vector3.zero)
				{
					result = collider.bounds;
				}
				else
				{
					result.Encapsulate (collider.bounds);
				}
			}
		}
		return result;
	}
    protected void ScaleStay()
    {
        // Set scales based on direct interaction along Z axis and indirect interaction along X axis.
        // Direct interaction uses change in radius along Z axis between contact point and combined center of all focus objects.
        Vector3 newHoverPoint = _stylusSelector.transform.TransformPoint(_stylusSpaceContact);
      
        float scaleRate = _scaleRate / _core.ViewerScale;
      
        // Indirect interaction uses a constant multiple of movement along Z axis.
        float zOffset = Vector3.Dot(newHoverPoint, _camera.transform.forward) - _startZ;
        float zScale = 1.0f;
        if (zOffset > Utility.Epsilon)
            zScale = 1.0f + -scaleRate * zOffset;
        else if (zOffset < -Utility.Epsilon)
            zScale = 1.0f / (1.0f + scaleRate * zOffset);

        // Indirect interaction uses a constant multiple of movement along X axis.
        float xOffset = Vector3.Dot(newHoverPoint, _camera.transform.right) - _startX;
        float xScale = 1.0f;
        if (xOffset > Utility.Epsilon)
            xScale = 1.0f + scaleRate * xOffset;
        else if (xOffset < -Utility.Epsilon)
            xScale = 1.0f / (1.0f - scaleRate * xOffset);

        float newScale = xScale * zScale;
        newScale = Mathf.Max(newScale, Utility.Epsilon);
        newScale = Mathf.Min(newScale, 1f/Utility.Epsilon);

        float scaleFactor = newScale / _oldScale;

        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];

            // Translate the object so its bounds center is still the same.
            Vector3 offset = (newScale - _oldScale) * (_startOffsets[i]);

            Utility.RecursivelyScale(focusObject, scaleFactor * Vector3.one);

            focusObject.transform.Translate(offset, Space.World);
        }

        _oldScale = newScale;

        ToolStay();
    }
 
    protected void ScaleEnd()
    {
        ToolEnd();

        _focusObjects.Clear();
        _oldScale = 1.0f;
    }
}
