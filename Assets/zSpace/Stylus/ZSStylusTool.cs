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
using zSpace.Common;

/// <summary>
/// Base class for a "tool", which changes the scene based on stylus input.
/// </summary>
public abstract class ZSStylusTool : ZSUMonoBehavior
{
		/// <summary> The ID of the stylus button that will make this tool start and stop acting. </summary>
		public int[] _toolButtons = new int[] {0};

		/// <summary> Modifier key (if any) for enabling facet snapping. </summary>
		public KeyCode[] _facetSnapButtons = new KeyCode[] {};

		/// <summary> Modifier key (if any) for enabling axis snapping. </summary>
		public KeyCode[] _axisSnapButtons = new KeyCode[] {};

		/// <summary> The stylus selector controlling this stylus's transform and the set of hovered/selected objects. </summary>
		protected ZSStylusSelector _stylusSelector;

		/// <summary> The set of objects this tool will act on. </summary>
		protected List<GameObject> _focusObjects = new List<GameObject> ();

		/// <summary> Snap the start and end points to the nearest vertex, edge, or face? </summary>
		public bool _useFacetSnapping = false;

		/// <summary> Snap nearest increment of axis or angle snap resolution? </summary>
		public bool _useAxisSnapping = false;

		/// <summary>
		/// If a component of this vector is non-zero, the same component of SnappedHoverPoint
		/// will be snapped to increments of it.  This is not related to facet snapping, which takes priority.
		/// </summary>
		public Vector3 _axisSnapResolution = Vector3.zero;

		/// <summary>
		/// If a component of this fector is non-zero, the same component of the euler angles of SnappedAngle
		/// will be snapped to increments of it.
		/// </summary>
		public Vector3 _angleSnapResolution = Vector3.zero;
  
		/// <summary>
		/// The name of the tool.
		/// Corresponds to callbacks On<ToolName>Begin, On<ToolName>Stay, and On<ToolName>End,
		/// which are called when the tool starts, continues, or stops operating, respectively.
		/// </summary>
		public string ToolName { protected set; get; }
 
		public bool IsOperating { protected set; get; }

		/// <summary> Saves references to collaborating objects and components. </summary> 
		protected override void OnScriptAwake ()
		{
				base.OnScriptAwake ();

				_stylusSelector = GameObject.Find ("ZSStylusSelector").GetComponent<ZSStylusSelector> ();
		}


		/// <summary> Responds to stylus input as soon as it has been updated. </summary>
		public virtual void OnStylus ()
		{
				// Update the hover point snapping state.

				_useFacetSnapping = _facetSnapButtons.Aggregate (false, (isPressed, buttonId) => isPressed |= Input.GetKey (buttonId));
				_useAxisSnapping = _axisSnapButtons.Aggregate (false, (isPressed, buttonId) => isPressed |= Input.GetKey (buttonId));
		}


		/// <summary>
		/// A filtered version of the ZSStylusSelector's HoverPoint.
		/// May be snapped to the nearest vertex, axis increment, or clamped length based on snap settings.
		/// </summary>
		public Vector3 SnappedHoverPoint {
				get {
						Vector3 p = _stylusSelector.HoverPoint;

						if (_stylusSelector.HoverObject != null) {
								if (_useFacetSnapping)
										return Utility.ComputeNearestVertexToPoint (_stylusSelector.HoverObject, p);
						}

						if (_useAxisSnapping) {
								for (int i = 0; i < 3; ++i) {
										if (_axisSnapResolution [i] != 0.0f)
												p [i] -= p [i] % _axisSnapResolution [i];
								}
						}
  
						return p;
				}
		}


		/// <summary> A filtered version </summary>
		public Quaternion SnappedRotation {
				get {
						if (_angleSnapResolution == Vector3.zero)
								return transform.rotation;

						Quaternion deltaRotation = transform.rotation * _invStartRotation;
						Vector3 euler = deltaRotation.eulerAngles;
						for (int i = 0; i < 3; ++i) {
								if (_angleSnapResolution [i] != 0.0f)
										euler [i] -= euler [i] % _angleSnapResolution [i];
						}

						return Quaternion.Euler (euler) * _startRotation;
				}
		}

		protected Quaternion _startRotation = Quaternion.identity;
		protected Quaternion _invStartRotation = Quaternion.identity;


		/// <summary> This virtual method is called each time the tool starts operating. </summary>
		protected virtual void ToolBegin ()
		{
				NotifyBegin ();
		}


		/// <summary> Notifies all focused objects and the active stylus that the tool has started acting. </summary>
		protected void NotifyBegin ()
		{
				IsOperating = true;
				_startRotation = transform.rotation;
				_invStartRotation = Quaternion.Inverse (transform.rotation);

				foreach (GameObject focusObject in _focusObjects)
						focusObject.BroadcastMessage ("On" + ToolName + "Begin", SendMessageOptions.DontRequireReceiver);

				_stylusSelector.activeStylus.BroadcastMessage ("On" + ToolName + "Begin", _focusObjects.ToArray (), SendMessageOptions.DontRequireReceiver);
		}


		/// <summary> This virtual method is called for each frame during tool operation. </summary>
		protected virtual void ToolStay ()
		{
				NotifyStay ();
		}


		/// <summary> Notifies all focused objects and the active stylus that the tool is still acting. </summary>
		protected void NotifyStay ()
		{
				foreach (GameObject focusObject in _focusObjects)
						focusObject.BroadcastMessage ("On" + ToolName + "Stay", SendMessageOptions.DontRequireReceiver);

				_stylusSelector.activeStylus.BroadcastMessage ("On" + ToolName + "Stay", _focusObjects.ToArray (), SendMessageOptions.DontRequireReceiver);
		}


		/// <summary> This virtual method is called each time the tool stops operating. </summary>
		protected virtual void ToolEnd ()
		{
				NotifyEnd ();
		}


		/// <summary> Notifies all focused objects and the active stylus that the tool has stopped acting. </summary>
		protected void NotifyEnd ()
		{
				IsOperating = false;

				string messageName = "On" + ToolName + "End";
				foreach (GameObject focusObject in _focusObjects)
						focusObject.BroadcastMessage (messageName, SendMessageOptions.DontRequireReceiver);

				_stylusSelector.activeStylus.BroadcastMessage (messageName, _focusObjects.ToArray (), SendMessageOptions.DontRequireReceiver);
		}
}
