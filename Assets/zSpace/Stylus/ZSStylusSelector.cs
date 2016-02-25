////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using zSpace.Common;

/// <summary>
/// Maintains a list of the objects that overlap with the stylus, sorted by distance from the tip.
/// Uses collision detection system to detect overlaps.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item> Checks LeftBracket and RightBracket keys to decrease/increase stylus depth when stylus simulation is enabled. </item>
/// <item> Checks CTRL key as modifier to enable multiple selection. </item>
/// </list>
/// </remarks>
public class HoverQueue : Comparer<RaycastHit>
{
		RaycastHit[] _hitInfos;
		ZSStylusSelector _stylusSelector;
		int _layerMask;
		ZSCore _core;

		/// <summary>
		/// Creates a HoverQueue for the given ZSStylusSelector.
		/// Only objects in the given bitmask of layers will be considered.
		/// </summary>
		public HoverQueue (ZSStylusSelector stylusSelector, int layerMask, ZSCore core)
		{
				_layerMask = layerMask;
				_stylusSelector = stylusSelector;
				_core = core;
		}


		/// <summary>
		/// Perform collision detection and sort the list of contacts.
		/// </summary>
		public void update ()
		{
				ZSLinearShape shape = _stylusSelector.activeStylus as ZSLinearShape;
				//float maxLength = (shape != null) ? _core.GetWorldScale() * shape._defaultLength : Mathf.Infinity;
				float maxLength = (shape != null) ? _core.GetViewerScale () * shape._defaultLength : Mathf.Infinity;

				float distance = Vector3.Distance (_stylusSelector.transform.position, _stylusSelector.activeStylus.hotSpot);
				float castLength = (Mathf.Approximately (distance, 0.0f)) ? maxLength : distance;

				//TODO: Sweeps/casts don't notice initial contacts.  And OnCollisionEnter doesn't work between a plain Collider and a Rigidbody.
				_hitInfos = Physics.RaycastAll (_stylusSelector.transform.position, _stylusSelector.transform.forward, castLength, _layerMask);
				Array.Sort<RaycastHit> (_hitInfos, this);
		}


		/// <summary>
		/// Returns the first GameObject in the hover queue as a RaycastHit to encode the distance and hit postion.
		/// </summary>
		public RaycastHit getFirst ()
		{
				if (_hitInfos != null && _hitInfos.Length > 0)
						return _hitInfos [0];
				RaycastHit result = new RaycastHit ();
				result.distance = Mathf.Infinity;
				return result;
		}


		/// <summary>
		/// This callback is called for comparison sorting to order the objects in the hover queue.
		/// It prefers objects closer to the hotspot.
		/// </summary>
		public override int Compare (RaycastHit x, RaycastHit y)
		{
				Vector3 hotSpot = _stylusSelector.activeStylus.hotSpot;
				float xDistance = (x.point - hotSpot).magnitude;
				float yDistance = (y.point - hotSpot).magnitude;

				return (int)Mathf.Sign (xDistance - yDistance);
		}
}


/// <summary>
/// Class for tracking the stylus configuration and managing its interaction with scene objects.
/// </summary>
public class ZSStylusSelector : ZSUIStylusInput
{
		/// <summary> Modes for emulating the stylus using mouse input. </summary>
		public enum StylusSimulatorMode
		{
				/// <summary> Do not emulate the stylus with mouse input. </summary>
				None = 0,

				/// <summary>
				/// The stylus will be positioned and oriented along a ray from the camera
				/// through the mouse position on the display plane.
				/// The mouse wheel will cause translation along this vector.
				/// </summary>
				Projection = 1,

				/// <summary>
				/// The stylus will be positioned along a ray as in projection, but the rotation will not change.
				/// The mouse wheel will cause translation along this vector.
				/// </summary>
				Position = 2,

				/// <summary>
				/// Rotation about the Y and X axes will correspond to horizontal and vertical mouse movement, respectively.
				/// The mouse wheel will cause rotation about the z axis.
				/// </summary>
				Rotation = 3,
		};
    
    
		protected bool _useCollision = true;
		protected ZSCore _zsCore;

		/// <summary>
		/// The ID of the tracker target driving this ZSStylusSelector.
		/// </summary>
		public ZSCore.TrackerTargetType _targetType = ZSCore.TrackerTargetType.Primary;

		/// <summary>
		/// The set of selected objects.
		/// Object membership can be queried in constant time.
		/// </summary>
		public HashSet<GameObject> selectedObjects = new HashSet<GameObject> ();

		/// <summary>
		/// A bitmask encoding all the layers that will be considered in stylus interaction.
		/// </summary>
		public int layerMask = 1;

		/// <summary>
		/// The minimum distance of stylus movement (in world space meters) required for a click-release interval to be considered a drag.
		/// </summary>
		public float minDragDistance = 0.01f;

		/// <summary>
		/// A special layer for UI and other elements where hovering is desired but selection is not.
		/// </summary>
		public int uiLayer;

		/// <summary>
		/// The mapping from a collidable object to what should be hovered or selected when that object collides with the stylus.
		/// </summary>
		public Utility.ObjectResolver objectResolver = Utility.FindRigidBodyResolver;

		/// <summary>
		/// The stylus simulator mode.
		/// </summary>
		public StylusSimulatorMode stylusSimulatorMode = StylusSimulatorMode.None;

		/// <summary> Use zSpace tracking data to update the stylus pose? </summary>
		public bool _useTracking = true;

		/// <summary>
		/// If the stylus stays still for this amount of time, the mouse will be used instead.
		/// </summary>
		public float StylusTimeout = 0.1f;

		/// <summary>
		/// Determines how quickly the simulated stylus's depth responds to mouse wheel and bracket input.
		/// </summary>
		public float WheelSensitivity = 0.1f;
		[HideInInspector]
		[SerializeField]
		protected ZSStylusShape _activeStylusShape;
		[HideInInspector]
		[SerializeField]
		protected ZSStylusTool _activeStylusTool;
		protected bool[] _isStylusButtonPressed = new bool[] {};
		protected bool[] _wasStylusButtonPressed = new bool[] { };
		protected bool[] _isMouseButtonPressed = new bool[] { };
		protected bool[] _wasMouseButtonPressed = new bool[] { };
		protected bool _wasHoveredObjectSelected;
		protected Vector3 _lastMousePosition;
		protected float _mouseWheel = 0.01f;
		protected Matrix4x4 _previousStylusPose = Matrix4x4.identity;
		protected float _timeSinceStylusMoved = 0f;
		protected Vector3 _buttonDownPosition;
		protected HashSet<GameObject> _oldSelectedObjects = new HashSet<GameObject> ();
		protected HoverQueue _hoverQueue;
		protected GameObject _HoverObject;
		protected Vector3 _localHoverPoint = Vector3.zero;
		protected Camera _centerCamera;
    
    
		/// <summary>
		/// The object (if any) that is overlapping the stylus and is closest to the hot spot.
		/// </summary>
		public override GameObject HoverObject {
				get { return _HoverObject; }
        
				protected set {
						if (value != _HoverObject) {
								if (_HoverObject != null) {
										activeStylus.OnHoverEnd (_HoverObject);
										_HoverObject.SendMessage ("OnUnhovered", SendMessageOptions.DontRequireReceiver);
								}
								if (value != null) {
										value.SendMessage ("OnHovered", SendMessageOptions.DontRequireReceiver);
										activeStylus.OnHoverBegin (value, HoverPoint);
								}
								_HoverObject = value;
						}
				}
		}

		/// <summary>
		/// The number of buttons the user can press on the current stylus.
		/// </summary>
		public int numButtons { 
				get {
						return Mathf.Max (3, _zsCore.GetNumTrackerTargetButtons (ZSCore.TrackerTargetType.Primary));
				}
		}

		/// <summary>
		/// The contact point between the stylus and the hovered object.
		/// If there is no hovered object, this is the last such point.
		/// </summary>
		public override Vector3 HoverPoint { get; protected set; }
    
		public override Vector3 Direction {
				get { return transform.forward; }
				protected set{}
		}

		public bool useCollision {
				get { return _useCollision; }
				set {
						if (!value)
								_localHoverPoint = transform.InverseTransformPoint (HoverPoint);
						_useCollision = value;
				}
		}

		/// <summary>
		/// The currently active stylus shape.
		/// Any shape-specific tools (such as a dragger or scaler) should be adjacent to this script.
		/// </summary>
		public ZSStylusShape activeStylus {
				get { return _activeStylusShape; }
				set {
						if (_activeStylusShape != null)
								_activeStylusShape.gameObject.SetActive (false);

						_activeStylusShape = value;
						_activeStylusTool = (value == null) ? null : value.GetComponent<ZSStylusTool> ();

						if (_activeStylusShape != null)
								_activeStylusShape.gameObject.SetActive (true);
				}
		}	
    
		/// <summary>
		/// Similar to Input.GetMouseButton(whichButton).  Returns true if the given stylus button is currently down.
		/// </summary>
		public override bool GetButton (int whichButton)
		{
				if (_isStylusButtonPressed == null || whichButton >= _isStylusButtonPressed.Length)
						return false;

				bool result = _isStylusButtonPressed [whichButton] ||
						stylusSimulatorMode != StylusSimulatorMode.None && _isMouseButtonPressed [whichButton];
				return result;
		}

		/// <summary>
		/// Similar to Input.GetMouseButtonDown(whichButton).  Returns true if the given stylus button was pressed during the last frame.
		/// </summary>
		public override bool GetButtonDown (int whichButton)
		{
				if (_isStylusButtonPressed == null || whichButton >= _isStylusButtonPressed.Length)
						return false;

				bool result = !_wasStylusButtonPressed [whichButton] && _isStylusButtonPressed [whichButton] ||
						stylusSimulatorMode != StylusSimulatorMode.None && !_wasMouseButtonPressed [whichButton] && _isMouseButtonPressed [whichButton];
				return result;
		}

		/// <summary>
		/// Similar to Input.GetMouseButtonUp(whichButton).  Returns true if the given stylus button was released during the last frame.
		/// </summary>
		public override bool GetButtonUp (int whichButton)
		{
				if (_isStylusButtonPressed == null || whichButton >= _isStylusButtonPressed.Length)
						return false;
            
				bool result = _wasStylusButtonPressed [whichButton] && !_isStylusButtonPressed [whichButton] ||
						stylusSimulatorMode != StylusSimulatorMode.None && _wasMouseButtonPressed [whichButton] && !_isMouseButtonPressed [whichButton];
				return result;
		}

		protected override void OnScriptAwake ()
		{
				base.OnScriptAwake ();

				_zsCore = GameObject.Find ("ZSCore").GetComponent<ZSCore> ();
		}

		protected override void OnScriptStart ()
		{
				base.OnScriptStart ();

				_zsCore.Updated += new ZSCore.CoreEventHandler (OnCoreUpdated);

				_lastMousePosition = Input.mousePosition;

				layerMask |= 1 << uiLayer;
				_hoverQueue = new HoverQueue (this, layerMask, _zsCore);

				_isStylusButtonPressed = new bool[numButtons];
				_wasStylusButtonPressed = new bool[numButtons];

				_isMouseButtonPressed = new bool[numButtons];
				_wasMouseButtonPressed = new bool[numButtons];

				for (int i = 0; i < numButtons; ++i) {
						_isStylusButtonPressed [i] = _wasStylusButtonPressed [i] = false;
						_isMouseButtonPressed [i] = _wasMouseButtonPressed [i] = false;
				}
      
				//TODO: For now, disable stylus collision so it doesn't conflict with raycasting.
				foreach (Collider collider in transform.GetComponentsInChildren<Collider>(true))
						collider.enabled = false;

				foreach (ZSStylusShape shape in GetComponentsInChildren<ZSStylusShape>(true)) {
						if (activeStylus == null)
								activeStylus = shape;

						shape.gameObject.SetActive (activeStylus == shape);
				}

				GameObject centerCameraObject = GameObject.Find ("ZSCenterCamera");
				if (centerCameraObject != null)
						_centerCamera = centerCameraObject.GetComponent<Camera>();
		}

		/// <summary>
		/// This function is called by ZSCore after each input update.
		/// </summary>
		private void OnCoreUpdated (ZSCore sender)
		{
				if (sender != _zsCore)
						return;

				//Update stylus button states.
				for (int i = 0; i < numButtons; ++i) {
						_wasStylusButtonPressed [i] = _isStylusButtonPressed [i];
						_wasMouseButtonPressed [i] = _isMouseButtonPressed [i];

						//Have to combine mouse state down here so asynchronous clients see it at the right time.
						_isStylusButtonPressed [i] = !_zsCore.IsMouseEmulationEnabled () && _zsCore.IsTrackerTargetButtonPressed (ZSCore.TrackerTargetType.Primary, i);
						_isMouseButtonPressed [i] = Input.GetMouseButton (i);
				}

				bool useMousePointer = false;
				if (_useTracking && !_zsCore.IsMouseEmulationEnabled ()) {
						Matrix4x4 pose = _zsCore.GetTrackerTargetWorldPose (_targetType);
						if (pose != _previousStylusPose)
								_timeSinceStylusMoved = 0f;
						else
								_timeSinceStylusMoved += Time.deltaTime;

						useMousePointer = _timeSinceStylusMoved > StylusTimeout;
						if (!useMousePointer) {
								_previousStylusPose = pose;
								transform.localScale = ZSStylusSelector.GetScale (pose);		
								transform.rotation = ZSStylusSelector.GetRotation (pose);		
								transform.position = ZSStylusSelector.GetPosition (pose);

								transform.localScale = _zsCore.GetViewerScale () * Vector3.one;
						}
				}

				//Simulate the stylus based on mouse input.

				Vector3 dMousePosition = Input.mousePosition - _lastMousePosition;
				dMousePosition [2] = WheelSensitivity * Input.GetAxis ("Mouse ScrollWheel");
				if (Input.GetKey (KeyCode.LeftBracket))
						dMousePosition [2] -= WheelSensitivity * Time.deltaTime;
				if (Input.GetKey (KeyCode.RightBracket))
						dMousePosition [2] += WheelSensitivity * Time.deltaTime;

				Camera mainCamera = (_zsCore.IsStereoEnabled ()) ? _centerCamera : _zsCore.CurrentCamera.GetComponent<Camera>();

				if (useMousePointer && mainCamera != null && mainCamera.enabled) {
						if (stylusSimulatorMode == StylusSimulatorMode.Projection || stylusSimulatorMode == StylusSimulatorMode.Position) {
								//Only update the wheel total if we aren't rotating.  Avoids extra Z translation artifact.
								_mouseWheel += dMousePosition [2];
								Ray ray = mainCamera.ScreenPointToRay (Input.mousePosition);
								Vector3 rayPoint = ray.GetPoint (0.1f + 0.5f * _mouseWheel * mainCamera.transform.localScale.magnitude);
								transform.position = rayPoint;

								if (stylusSimulatorMode == StylusSimulatorMode.Projection)
										transform.rotation = Quaternion.LookRotation (ray.GetPoint (1.0f) - mainCamera.transform.position, mainCamera.transform.up);
						} else if (stylusSimulatorMode == StylusSimulatorMode.Rotation) {
								Vector3 euler = transform.localRotation.eulerAngles;
								euler += new Vector3 (-0.1f * dMousePosition.y, 0.1f * dMousePosition.x, -1000.0f * dMousePosition.z);
								var oldHoverPoint = transform.TransformPoint (_localHoverPoint);
								transform.localRotation = Quaternion.Euler (euler);
								transform.localPosition += oldHoverPoint - transform.TransformPoint (_localHoverPoint);
						}
				}

				//Make the hovered object the closest one to the tip.

				if (_useCollision) {
						_hoverQueue.update ();
						RaycastHit hit = _hoverQueue.getFirst ();
						if (hit.collider != null) {
								HoverPoint = hit.point;
								HoverObject = (hit.collider.gameObject.layer == uiLayer) ?
                          hit.collider.gameObject :
                          objectResolver (hit.collider.gameObject);
						} else {
								ZSLinearShape linearStylus = activeStylus as ZSLinearShape;
								if (linearStylus != null && linearStylus._tip != null && linearStylus._tip.activeSelf)
										HoverPoint = linearStylus._tip.transform.position;
								else
										HoverPoint = activeStylus.hotSpot;
								HoverObject = null;
						}
				} else {
						HoverPoint = transform.TransformPoint (_localHoverPoint);
				}

				if (_HoverObject != null)
						activeStylus.OnHoverStay (_HoverObject, HoverPoint);

				//Update the set of selected objects based on clicking.

				if (GetButtonDown (SelectButton)) {
						if (!Input.GetKey (KeyCode.LeftControl))
								selectedObjects.Clear ();

						if (_HoverObject != null) {
								if (_HoverObject.layer == uiLayer) {
										_wasHoveredObjectSelected = false;
								} else {
										_wasHoveredObjectSelected = selectedObjects.Contains (_HoverObject);
										if (!selectedObjects.Contains (_HoverObject))
												selectedObjects.Add (_HoverObject);
								}
						}

						_buttonDownPosition = transform.position;
				}

				if (GetButtonUp (SelectButton)) {
						bool wasDrag = (transform.position - _buttonDownPosition).magnitude > minDragDistance;
						if (_HoverObject != null && _wasHoveredObjectSelected && !wasDrag && Input.GetKey (KeyCode.LeftControl))
								selectedObjects.Remove (_HoverObject);
				}

				// Send messages to objects whose selection state changed.

				foreach (GameObject selectedObject in _oldSelectedObjects) {
						if (selectedObject != null && !selectedObjects.Contains (selectedObject)) {
								activeStylus.OnDeselected (selectedObject);
								selectedObject.SendMessage ("OnDeselected", SendMessageOptions.DontRequireReceiver);
						}
				}
        
				GameObject[] tmpSelectedObjects = new GameObject[selectedObjects.Count];
				selectedObjects.CopyTo (tmpSelectedObjects); // So objects can de-select themselves.
				foreach (GameObject selectedObject in tmpSelectedObjects) {
						if (selectedObject != null && !_oldSelectedObjects.Contains (selectedObject)) {
								selectedObject.SendMessage ("OnSelected", SendMessageOptions.DontRequireReceiver);
								activeStylus.OnSelected (selectedObject, HoverPoint);
						}
				}

				_oldSelectedObjects.Clear ();
				foreach (GameObject selectedObject in selectedObjects) {
						if (selectedObject != null)
								_oldSelectedObjects.Add (selectedObject);
				}

				_lastMousePosition = Input.mousePosition;
        
				activeStylus.Tool.OnStylus ();
		}
	
		public static Quaternion GetRotation (Matrix4x4 matrix)
		{
				var qw = Mathf.Sqrt (1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
				var w = 4 * qw;
				var qx = (matrix.m21 - matrix.m12) / w;
				var qy = (matrix.m02 - matrix.m20) / w;
				var qz = (matrix.m10 - matrix.m01) / w;

				return new Quaternion (qx, qy, qz, qw);
		
		}

		public static Vector3 GetPosition (Matrix4x4 matrix)
		{
				var x = matrix.m03;
				var y = matrix.m13;
				var z = matrix.m23;

				return new Vector3 (x, y, z);
		}

		public static Vector3 GetScale (Matrix4x4 m)
		{
				var x = Mathf.Sqrt (m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
				var y = Mathf.Sqrt (m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
				var z = Mathf.Sqrt (m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);

				return new Vector3 (x, y, z);
		}
}
