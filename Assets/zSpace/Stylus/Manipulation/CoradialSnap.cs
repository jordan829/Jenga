////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Binding site for making two line segments collinear.
/// </summary>
/// <remarks>
/// Forms a trigger volume by tracing a ray from its polygon's center and along its normal and
/// expanding to a cylinder with a radius equal to its snapMargin.  The ray is its "axis".
/// When 2 CoradialSnaps overlap, the axes become collinear.
/// While this snap is active, translation is still allowed along it and rotation is still allowed
/// around it.
/// </remarks>
public class CoradialSnap : Snap
{
  CapsuleCollider _capsuleCollider;

  protected override void OnScriptStart()
  {
    base.OnScriptStart();
    
    if (!_isInitialized)
    {
      if (GetComponent<Collider>() == null)
      {
        _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        _capsuleCollider.radius = snapMargin;
        _capsuleCollider.height = 1.0f;
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
    Vector3 onAxisTranslation = Vector3.Project(impliedTranslation, -mateObject.transform.up);
    Vector3 offset = mateObject.transform.position - rotatedPosition;
    snapObject.transform.position = dragObject.transform.position + offset - Vector3.Project(offset, -mateObject.transform.up) - onAxisTranslation;
  }


  void OnZSDragToolBegin()
  {
    if (_capsuleCollider != null)
    {
      _capsuleCollider.height = snapMargin;
      _capsuleCollider.center = Vector3.zero;
    }
  }


  void OnZSDragToolEnd()
  {
    if (_capsuleCollider != null)
    {
      GetComponent<CapsuleCollider>().height = 1;
      GetComponent<CapsuleCollider>().center = 0.5f * Vector3.up;
    }
  }
}