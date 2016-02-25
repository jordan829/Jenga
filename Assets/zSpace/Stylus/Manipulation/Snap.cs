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
/// A binding site for use in object snapping.
/// </summary>
public abstract class Snap : ZSUMonoBehavior
{
		/// <summary>
		/// Gets or sets the control polygon that defines the shape of this Snap.
		/// </summary>
		/// <remarks>
		/// Each Snap has a trigger volume corresponding to facets in the snappable object.
		/// The trigger volume's shape is a function of the snap type and this control polygon.
		/// For example, a co-planar snap may extrude the polygon along its normal vector.
		/// When the trigger volume intersects with another snappable object's trigger volume,
		/// the object being dragged will snap into alignment with the other object.
		/// Vertices are in meters in the snap site's local frame of reference.
		/// When a new polygon is specified, its center is amortized into its transform,
		/// so when the polygon is retrieved, the average of its vertices will always be 0, 0, 0.
		/// </remarks>
		public Vector3[] Polygon {
				set {
						_polygon = value;
						if (_polygon != null) {
								//Amortize the polygon basis into the GameObject transform.

								Vector3 tangent = (_polygon [1] - _polygon [0]).normalized;
								Vector3 normal = Vector3.Cross (tangent, _polygon [2] - _polygon [1]).normalized;
								gameObject.transform.localRotation = Quaternion.LookRotation (tangent, normal);
    
								Vector3 center = Vector3.zero;
								foreach (Vector3 vertex in _polygon)
										center += vertex;
								center /= _polygon.Length;
								gameObject.transform.localPosition = center;

								Quaternion invRotation = Quaternion.Inverse (transform.localRotation);
								for (int i = 0; i < _polygon.Length; ++i)
										_polygon [i] = invRotation * (_polygon [i] - transform.localPosition);
						}
				}
				get { return _polygon; }
		}

		/// <summary>
		/// The distance in meters this object must be from another snappable object before snapping takes effect.
		/// </summary>
		public float snapMargin;

		/// <summary>
		/// When the current object is being dragged, a render-only clone will be made for snapping.  This is the snapObject.
		/// </summary>
		[HideInInspector]
		public GameObject
				snapObject;

		/// <summary>
		/// If snapping is in effect, the mateObject is the object to which the current object is being snapped.
		/// </summary>
		[HideInInspector]
		public GameObject
				mateObject;

		/// <summary>
		/// This function determines the dragged object based on the Snap.
		/// By default, the Snap is assumed to be a child of the dragged object.
		/// </summary>
		public Utility.ObjectResolver objectResolver = Utility.ParentResolver;

		/// <summary>
		/// The internal set of vertices (in meters) controlled by the polygon property.
		/// </summary>
		protected Vector3[] _polygon;

		/// <summary>
		/// Has this snap site been initialized yet?
		/// </summary>
		protected bool _isInitialized = false;
		static int s_bindSiteIdSuffix = 0;


		/// <summary> Loads binding sites and constraints from the specified file. </summary>
		// TODO: Don't make this static.  Add a manager to a GameObject and expose editor fields for resolver, search patterns, etc.
		public static GameObject[] BuildTriggerVolumes (GameObject bindingMesh, GameObject hostMesh)
		{
				List<GameObject> triggerVolumes = new List<GameObject> ();

				foreach (MeshFilter meshFilter in bindingMesh.GetComponentsInChildren<MeshFilter>(true)) {
						Type snapType = default(Type);
						MeshRenderer meshRenderer = meshFilter.gameObject.GetComponent<MeshRenderer> ();

						string parentName = meshRenderer.sharedMaterial.name;
						string BindCoplanar = "_bindCoplanar";
						string BindCoradial = "_bindCoradial";
						string BindFixed = "_bindFixed";
      
						if (meshRenderer.sharedMaterial.name.EndsWith (BindCoplanar)) {
								snapType = typeof(CoplanarSnap);
								parentName = parentName.Substring (0, parentName.Length - BindCoplanar.Length);
						} else if (meshRenderer.sharedMaterial.name.EndsWith (BindCoradial)) {
								snapType = typeof(CoradialSnap);
								parentName = parentName.Substring (0, parentName.Length - BindCoradial.Length);
						} else if (meshRenderer.sharedMaterial.name.EndsWith (BindFixed)) {
								snapType = typeof(FixedSnap);
								parentName = parentName.Substring (0, parentName.Length - BindFixed.Length);
						} else {
								Debug.Log ("WARNING: Invalid snap material: " + meshRenderer.sharedMaterial.name);
								continue;
						}

						bool wasActive = hostMesh.activeSelf;
						hostMesh.SetActive (true);
						GameObject parent = GameObject.Find (parentName);
						if (parent == null) {
								Debug.Log ("WARNING: Failed to find parent object: " + parentName);
								continue;
						}
						hostMesh.SetActive (wasActive);

						Vector3[] vertices = meshFilter.sharedMesh.vertices;

						int[] triangles = meshFilter.sharedMesh.triangles;

						var polygons = new List<Vector3[]> ();

						// See if we can coalesce 2 triangles into a quad.
						for (int triangleBase = 0; triangleBase + 6 <= triangles.Length; triangleBase += 6) {
								bool combinedTriangles = false;
								Vector3[] polygon = new Vector3[4];

								int matchCount = 0;
								for (int i = 0; i < 3; i++)
										polygon [i] = vertices [triangles [triangleBase + i]];

								for (int j = 3; j < 6; j++) {
										bool foundMatch = false;
										for (int i = 0; i < 3; i++) {
												if (Mathf.Approximately (polygon [i].x, vertices [triangles [triangleBase + j]].x) &&
														Mathf.Approximately (polygon [i].y, vertices [triangles [triangleBase + j]].y) &&
														Mathf.Approximately (polygon [i].z, vertices [triangles [triangleBase + j]].z)) {
														foundMatch = true;
														matchCount++;
														break;
												}
										}
										if (!foundMatch)
												polygon [3] = vertices [triangles [triangleBase + j]];
								}
								if (matchCount == 2) {
										UnityEngine.Plane plane = new UnityEngine.Plane (polygon [0], polygon [1], polygon [2]);
										float dist = plane.GetDistanceToPoint (polygon [3]);
										if (Mathf.Approximately (dist, 0.0f)) {
												polygons.Add (polygon);
												combinedTriangles = true;
										}
								}

								if (!combinedTriangles) {
										for (int i = 0; i < 6; i += 3) {
												Vector3[] triangle = new Vector3[3];
												triangle [0] = vertices [triangles [triangleBase + i]];
												triangle [1] = vertices [triangles [triangleBase + i + 1]];
												triangle [2] = vertices [triangles [triangleBase + i + 2]];
												polygons.Add (triangle);
										}
								}
						}

						foreach (Vector3[] polygon in polygons) {
								float snapMargin = 0.005f;
								++s_bindSiteIdSuffix;

								GameObject go = new GameObject (snapType.Name + s_bindSiteIdSuffix);

								go.transform.parent = parent.transform;

								go.transform.localScale = Vector3.one;
								go.transform.localPosition = Vector3.zero;
								go.transform.localRotation = Quaternion.identity;

								triggerVolumes.Add (go);

								Snap Snap = go.AddComponent (snapType) as Snap;
								Snap.snapMargin = snapMargin;
								Snap.Polygon = polygon;
						}

						UnityEngine.Object.Destroy (meshFilter.gameObject.GetComponent<MeshRenderer> ());
						UnityEngine.Object.Destroy (meshFilter);
				}

				return triggerVolumes.ToArray ();
		}
  
		protected override void OnScriptAwake ()
		{
				base.OnScriptAwake ();
    
				gameObject.layer = LayerMask.NameToLayer (GetType ().Name);
		}

		protected override void OnScriptStart ()
		{
				base.OnScriptStart ();

				GameObject stylusSelectorObject = GameObject.Find ("ZSStylusSelector");
				if (stylusSelectorObject != null)
						objectResolver = stylusSelectorObject.GetComponent<ZSStylusSelector> ().objectResolver;
		}

		protected override void OnScriptTriggerEnter (Collider collider)
		{
				base.OnScriptTriggerEnter (collider);

				System.Type type = GetType ();
				Snap otherSite = collider.gameObject.GetComponent (type.Name) as Snap;
				if (otherSite == null)
						return;

				if (mateObject == null) {
						mateObject = collider.gameObject;
				}
		}

		protected override void OnScriptTriggerExit (Collider collider)
		{
				base.OnScriptTriggerExit (collider);

				if (collider.gameObject != mateObject)
						return;

				Deactivate ();
		}

		public void Deactivate ()
		{
				if (mateObject != null)
						mateObject = null;

				if (snapObject != null) {
						GameObject go = objectResolver (gameObject);
						snapObject.transform.position = go.transform.position;
						snapObject.transform.rotation = go.transform.rotation;
				}
		}

		/// <summary>
		/// Called once every frame while this snap is overlapping with another snap (mateObject).
		/// </summary>
		public abstract void OnSnapStay ();
}