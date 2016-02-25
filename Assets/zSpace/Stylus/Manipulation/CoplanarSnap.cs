////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Binding site for making two polygons coplanar.
/// </summary>
/// <remarks>
/// Forms a trigger volume by extruding its polygon along the normal by its snapMargin.
/// When 2 CoplanarSnaps overlap, their polygons become coplanar.
/// While this snap is active, in-plane translation and about-normal rotation are still allowed.
/// </remarks>
public class CoplanarSnap : Snap
{
  protected override void OnScriptStart()
  {
    base.OnScriptStart();
    
    if (!_isInitialized)
    {
      if (GetComponent<Collider>() == null)
      {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        float scaleFactor = Mathf.Abs(1.0f / Vector3.Dot(transform.localScale, transform.localRotation * Vector3.up));
        meshCollider.convex = true;
        meshCollider.sharedMesh = Utility.extrudePolygon(Polygon, snapMargin * scaleFactor);
      }

      _isInitialized = true;
    }
  }


  public override void OnSnapStay()
  {
    GameObject dragObject = objectResolver(gameObject);

    Quaternion deltaRotation = Quaternion.FromToRotation(transform.up, -mateObject.transform.up);
    snapObject.transform.rotation = deltaRotation * dragObject.transform.rotation;

    Vector3 rotatedPosition = dragObject.transform.position + snapObject.transform.rotation * Quaternion.Inverse(dragObject.transform.rotation) * (transform.position - dragObject.transform.position);
    Vector3 impliedTranslation = rotatedPosition - transform.position;
    Vector3 inPlaneTranslation = impliedTranslation - Vector3.Project(impliedTranslation, -mateObject.transform.up);
    Vector3 offset = mateObject.transform.position - rotatedPosition;
    snapObject.transform.position = dragObject.transform.position + Vector3.Project(offset, -mateObject.transform.up) - inPlaneTranslation;
  }
}