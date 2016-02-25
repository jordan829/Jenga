////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Binding site for making two transforms line up in all six degrees of freedom.
/// </summary>
/// <remarks>
/// Forms a trigger volume by expanding a sphere about the polygon's center
/// with a radius equal to its snapMargin.
/// A basis is formed with a forward vector along the polygon's first edge,
/// an up vector along the polygon's normal,
/// and a right vector as the cross product of the two.
/// When two FixedSnaps overlap, the bases are abutted along the x = -z line,
/// so their polygon centers are at the same point and rotating by 180 degrees
/// about x = -z would swap their poses.
/// No motion is allowed while this snap is active.
/// </remarks>
public class FixedSnap : Snap
{
  protected override void OnScriptStart()
  {
    base.OnScriptStart();
    
    if (!_isInitialized)
    {
      if (GetComponent<Collider>() == null)
      {
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = snapMargin;
      }
  
      _isInitialized = true;
    }
  }


  public override void OnSnapStay()
  {
    GameObject dragObject = objectResolver(gameObject);

    Quaternion flipY = Quaternion.Euler(new Vector3(0, 0, 180));
    Quaternion deltaRotation = mateObject.transform.rotation * flipY * Quaternion.Inverse(transform.rotation);
    snapObject.transform.rotation = deltaRotation * dragObject.transform.rotation;

    Vector3 rotatedPosition = dragObject.transform.position + snapObject.transform.rotation * Quaternion.Inverse(dragObject.transform.rotation) * (transform.position - dragObject.transform.position);
    Vector3 offset = mateObject.transform.position - rotatedPosition;
    snapObject.transform.position = dragObject.transform.position + offset;
  }
}