////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Stylus tool for dragging objects with or without physics.
/// </summary>
/// <remarks>
/// Due to limitations in Unity, you cannot drag an object whose parent has a non-uniform scale.
/// </remarks>
public class ZSDragTool : ZSStylusTool
{  
    /// <summary>
    /// Chooses between physical or non-physical dragging.
    /// During non-physical drag, the dragged object's transform is directly updated.
    /// During physical drag, the dragged object is physically constrained to the stylus.
    /// </summary>
    public bool _isPhysical = false;

    /// <summary> Modifier key (if any) that enables physical dragging while pressed. </summary>
    public KeyCode[] _physicalDragButtons = new KeyCode[] {};
    protected Vector3 _contactPoint;
    protected Vector3[] _focusOffsets;
    protected Quaternion[] _focusRotations;
    protected Dictionary<GameObject, Joint> _focusJoints = new Dictionary<GameObject, Joint>();
    protected List<GameObject> _oldDynamicBodies = new List<GameObject>();
    protected bool _wasPhysical;

    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSDragTool";
        _wasPhysical = _isPhysical;
    }

    public override void OnStylus()
    {
        base.OnStylus();

        bool isButtonDown = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonDown(buttonId));
        if (isButtonDown && _stylusSelector.HoverObject != null)
            ToolBegin();

        bool isButton = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
        if (isButton)
            ToolStay();

        bool isButtonUp = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonUp(buttonId));
        if (isButtonUp)
            ToolEnd();
    }

    protected override void ToolBegin()
    {
        _focusObjects.AddRange(_stylusSelector.selectedObjects);
        BeginDrag();
        NotifyBegin();
    }

    protected void BeginDrag()
    {
        _contactPoint = _stylusSelector.HoverPoint;

        _focusRotations = new Quaternion[_focusObjects.Count];
        _focusOffsets = new Vector3[_focusObjects.Count];

        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];

            // Save the relative transform from the stylus to the focus object.
            Quaternion invRotation = Quaternion.Inverse(transform.rotation);
            _focusOffsets[i] = invRotation * (focusObject.transform.position - _stylusSelector.HoverPoint);
            _focusRotations[i] = invRotation * focusObject.transform.rotation;

            // Temporarily remove any nested dynamic Rigidbodies so dragging is predictable.

            foreach (Rigidbody rb in focusObject.transform.GetComponentsInChildren<Rigidbody>(true))
            {
                if (!rb.isKinematic)
                {
                    rb.isKinematic = true;
                    _oldDynamicBodies.Add(rb.gameObject);
                }
            }
        }

        if (_focusObjects.Count > 0)
            _stylusSelector.useCollision = false;

        _wasPhysical = false;
    }

    protected void BeginPhysicalDrag()
    {
        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];

            foreach (Rigidbody rb in focusObject.transform.GetComponentsInChildren<Rigidbody>(true))
			{
				rb.isKinematic = false;

	            // Constrain the dragged object to the stylus.
	
	            ConfigurableJoint joint = rb.gameObject.AddComponent<ConfigurableJoint>();
	            joint.targetPosition = Vector3.zero;
	            joint.targetRotation = Quaternion.identity;
	            joint.targetVelocity = Vector3.zero;
	            joint.targetAngularVelocity = Vector3.zero;
	            joint.connectedBody = GetComponent<Rigidbody>();
	            joint.anchor = rb.gameObject.transform.InverseTransformPoint(_contactPoint);
	            joint.axis = Vector3.right;
	            joint.secondaryAxis = Vector3.forward;
	
	            JointDrive drive = new JointDrive();
	            drive.mode = JointDriveMode.Position;
	            drive.maximumForce = 10000.0f;
	            drive.positionSpring = 100.0f;
	            drive.positionDamper = 1.0f;
	            joint.xDrive = drive;
	            joint.yDrive = drive;
	            joint.zDrive = drive;
	            joint.angularXDrive = drive;
	            joint.angularYZDrive = drive;

            	_focusJoints[rb.gameObject] = joint;
			}
        }
    }

    protected override void ToolStay()
    {
        Drag();
        NotifyStay();
    }

    protected void Drag()
    {
        // Seamlessly switch between physical and non-physical drag.
        _isPhysical = _physicalDragButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= Input.GetKey(buttonId));

        if (_isPhysical != _wasPhysical)
        {
            if (_wasPhysical)
                EndPhysicalDrag();
            else
                BeginPhysicalDrag();

            _wasPhysical = _isPhysical;
        }

        // For non-physical dragging, update the dragged object transform by applying the saved relative transform to the new stylus transform.
        if (!_isPhysical)
            TransformDragObjects();
    }
 
    protected void TransformDragObjects()
    {
        Quaternion snappedRotation = SnappedRotation;
        Vector3 snappedHoverPoint = SnappedHoverPoint;

        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];
            focusObject.transform.rotation = snappedRotation * _focusRotations[i];
            focusObject.transform.position = snappedHoverPoint + snappedRotation * _focusOffsets[i];
        }
    }

    protected override void ToolEnd()
    {
        NotifyEnd();
        EndDrag();
        _focusObjects.Clear();
    }

    protected void EndDrag()
    {
		Vector3[] velocities = new Vector3[_oldDynamicBodies.Count];
		Vector3[] angularVelocities = new Vector3[_oldDynamicBodies.Count];
		for (int i = 0; i < _oldDynamicBodies.Count; ++i)
		{
			velocities[i] = _oldDynamicBodies[i].GetComponent<Rigidbody>().velocity;
			angularVelocities[i] = _oldDynamicBodies[i].GetComponent<Rigidbody>().angularVelocity;
		}
		
        // End drag.

        if (_isPhysical)
            EndPhysicalDrag();

        // Restore dynamics to any Rigidbodies that previously had them.

        for (int i = 0; i < _oldDynamicBodies.Count; ++i)
        {
			GameObject go = _oldDynamicBodies[i];
			
            if (go != null && go.GetComponent<Rigidbody>() != null)
			{
                go.GetComponent<Rigidbody>().isKinematic = false;
				go.GetComponent<Rigidbody>().velocity = velocities[i];
				go.GetComponent<Rigidbody>().angularVelocity = angularVelocities[i];
			}
        }
        
        _oldDynamicBodies.Clear();

        _stylusSelector.useCollision = true;
    }

    protected void EndPhysicalDrag()
    {
        foreach (KeyValuePair<GameObject, Joint> pair in _focusJoints)
            DestroyImmediate(pair.Value);
        _focusJoints.Clear();
     
        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];

			foreach (Rigidbody rb in focusObject.GetComponentsInChildren<Rigidbody>(true))
				rb.isKinematic = true;
        }
    }
}
