////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Linear stylus shape with laser-like behavior.
/// Auto-sizes to the nearest intersecting object.
/// </summary>
public class ZSLaserShape : ZSLinearShape
{
  protected bool _doHideTip = false;

  protected override void OnScriptStart()
  {
    base.OnScriptStart();

    if (_tip != null && !_tip.activeSelf)
      _doHideTip = true;
  }


  /// <summary>
  /// This callback defines the way the stylus responds to overlap with an object in the scene.
  /// It is called whenever an overlap begins.
  /// </summary>
  public override void OnHoverBegin(GameObject gameObject, Vector3 point)
  {
    base.OnHoverBegin(gameObject, point);

    if (_doHideTip)
      _tip.SetActive(true);
  }


  /// <summary>
  /// This callback defines the way the stylus responds to overlap with an object in the scene.
  /// It is called for each frame as long as the stylus is overlapping with the object.
  /// </summary>
  public override void OnHoverStay(GameObject gameObject, Vector3 point)
  {
    base.OnHoverStay(gameObject, point);

    float hoverDistance = Mathf.Max(transform.InverseTransformPoint(point).z, _tipLength + _baseLength);

    if (_beam != null)
      _beam.transform.localScale = new Vector3(1.0f, 1.0f, (hoverDistance - _tipLength - _baseLength) / _beamLength);

    if (_tip != null)
      _tip.transform.localPosition = hoverDistance * Vector3.forward;
  }


  /// <summary>
  /// This callback defines the way the stylus responds to overlap with an object in the scene.
  /// It is called whenever an overlap begins.
  /// </summary>
  public override void OnHoverEnd(GameObject gameObject)
  {
    base.OnHoverEnd(gameObject);

    ScaleLengthBy(1f);
    if (_doHideTip)
      _tip.SetActive(false);
  }
}
