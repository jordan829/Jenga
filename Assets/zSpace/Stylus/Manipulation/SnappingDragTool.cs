////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Stylus drag tool with support for snapping. Maintains Snap and dragging layers.
/// During dragging, moves Snap sites into a parallel GameObject to track overlap with other Snap sites.
/// </summary>
public class SnappingDragTool : ZSDragTool
{
    Dictionary<GameObject, GameObject> _snapObjects = new Dictionary<GameObject, GameObject>();
    protected Dictionary<Snap, Transform> _oldParents = new Dictionary<Snap, Transform>();

    protected override void ToolBegin()
    {
	    _focusObjects.AddRange(_stylusSelector.selectedObjects);
		SplitDragObjects();
        BeginDrag();
		NotifyBegin();
    }
	
	
    /// <summary>
    /// When dragging begins, this divides the dragged object into 2 GameObjects.
    /// The first is a "snap object".  It contains everything in the original object except for the snap sites.
    /// The second is a "drag object", containing only the snap sites.  The drag object is transformed directly by
    /// the stylus.  When one of its snap sites overlaps with a compatible snap site in the scene, the snap object
    /// jumps to a snapped location, but the drag object does not.
    /// </summary>
	protected void SplitDragObjects()
	{
        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject snapObject = _focusObjects[i];
    
            GameObject dragObject = new GameObject();
            dragObject.name = "Drag Object";
            dragObject.transform.position = snapObject.transform.position;
            dragObject.transform.rotation = snapObject.transform.rotation;
            dragObject.transform.localScale = snapObject.transform.lossyScale;
            foreach (NetworkView snapView in snapObject.GetComponents<NetworkView>())
            {
                if (snapView.owner == Network.player)
                {
                    NetworkView dragView = dragObject.AddComponent<NetworkView>();
                    dragView.viewID = snapView.viewID;
                    DestroyImmediate(snapView);
                }
                else
                {
                    snapView.enabled = false;
                }
            }

            dragObject.transform.parent = snapObject.transform.parent;
            snapObject.transform.parent = dragObject.transform;

            _snapObjects[dragObject] = snapObject;
            _focusObjects[i] = dragObject;
			
            GameObject rbObject = new GameObject();
			rbObject.name = "Rigidbody Object";
			rbObject.transform.position = dragObject.transform.position;
			rbObject.transform.rotation = dragObject.transform.rotation;
			rbObject.transform.localScale = dragObject.transform.localScale;
			rbObject.transform.parent = dragObject.transform;
    
            foreach (Snap snap in snapObject.transform.GetComponentsInChildren<Snap>(true))
            {
              _oldParents[snap] = snap.transform.parent;
              snap.transform.parent = rbObject.transform;
              snap.snapObject = snapObject;
              snap.gameObject.layer = LayerMask.NameToLayer(snap.GetType().Name + "Dragging");
              snap.GetComponent<Collider>().isTrigger = true;
            }

            Rigidbody rb = rbObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
	}


    protected override void ToolStay()
    {
        Drag();
        SuspendPhysicsIfSnapping();
        NotifyStay();

        foreach (GameObject dragObject in _focusObjects)
        {
            foreach (Snap snap in dragObject.transform.GetComponentsInChildren<Snap>(true))
            {
                if (snap.mateObject != null)
                    snap.OnSnapStay();
            }
        }
    }
	
	
    /// <summary>
    /// If the snap object is not currently snapping to something in the scene, this updates
    /// its transform to correspond to the drag object.
    /// </summary>
    /// <remarks>
    /// This approach is used instead of parenting because of callback timing, which can
    /// result in jitter while the snap object is snapped.
    /// </remarks>
	protected void SuspendPhysicsIfSnapping()
	{
        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject focusObject = _focusObjects[i];

            bool isMated = false;
    
            foreach (Snap Snap in focusObject.transform.GetComponentsInChildren<Snap>(true))
                isMated |= Snap.mateObject != null;
			if (isMated)
			{
				if (_isPhysical)
				{
					_isPhysical = false;
					_isPhysicalDragSuspended = true;
				}
			}
			else
            {
				if (_isPhysicalDragSuspended)
				{
					_isPhysical = true;
					_isPhysicalDragSuspended = false;
				}
			}
        }
	}
	bool _isPhysicalDragSuspended = false;


    protected override void ToolEnd()
    {
        EndDrag();
        MergeDragObjects();
        NotifyEnd();
        _focusObjects.Clear();
    }
	
	
    /// <summary>
    /// Returns the snap object and snap sites to their original layers, re-assembles the
    /// original game object, and destroys the drag object.  Before the merge, the drag
    /// object is transformed to the final location of the snap object.
    /// </summary>
	protected void MergeDragObjects()
	{
        for (int i = 0; i < _focusObjects.Count; ++i)
        {
            GameObject dragObject = _focusObjects[i];
            GameObject snapObject = _snapObjects[dragObject];
            snapObject.transform.parent = dragObject.transform.parent;
            dragObject.transform.position = snapObject.transform.position;
            dragObject.transform.rotation = snapObject.transform.rotation;
            foreach (Snap snap in dragObject.GetComponentsInChildren<Snap>(true))
            {
                snap.transform.parent = _oldParents[snap];
                snap.GetComponent<Collider>().isTrigger = false;
                snap.gameObject.layer = LayerMask.NameToLayer(snap.GetType().Name);
                snap.Deactivate();
            }

            foreach (NetworkView dragView in dragObject.GetComponents<NetworkView>())
            {
                NetworkView snapView = snapObject.AddComponent<NetworkView>();
                snapView.viewID = dragView.viewID;
            }

            foreach (NetworkView snapView in snapObject.GetComponents<NetworkView>())
                snapView.enabled = true;

            _focusObjects[i] = snapObject;
            DestroyImmediate(dragObject);
        }

        _snapObjects.Clear();
        _oldParents.Clear();
	}
}
