////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A draggable volume that selects all colliding objects.
/// Disable the GameObject to disable this behavior.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class ZSSelectionVolume : MonoBehaviour
{
  /// <summary>
  /// If true, the selection volume will be aligned to the X, Y, and Z axes in world space.
  /// If false, it will be aligned to the local X, Y, and Z axes of the stylus on button down.
  /// </summary>
  public bool _isAxisAligned = true;

  Vector3 _endPosition;
  Vector3 _startPosition;
  Quaternion _startRotation;
  ZSStylusSelector _stylusSelector;
  int[] _selectButtons = new int[] {0};
  Dictionary<GameObject, int> _overlapCounts = new Dictionary<GameObject, int>();

  void Awake()
  {
    _stylusSelector = GameObject.Find("ZSStylusSelector").GetComponent<ZSStylusSelector>();
    GetComponent<Collider>().enabled = false;
    GetComponent<Renderer>().enabled = false;
  }


  void Update()
  {
    bool isButtonDown = _selectButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonDown(buttonId));
    if (isButtonDown)
    {
      if (_stylusSelector.HoverObject == null)
      {
        _startPosition = _stylusSelector.activeStylus.hotSpot;
        _startRotation = (_isAxisAligned) ? Quaternion.identity : _stylusSelector.activeStylus.transform.rotation;
  
        GetComponent<Collider>().enabled = true;
        GetComponent<Renderer>().enabled = true;
        transform.rotation = _startRotation;
      }
    }

    bool isButton = _selectButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
    if (isButton && GetComponent<Renderer>().enabled)
    {
      _endPosition = _stylusSelector.activeStylus.hotSpot;

      Vector3 diagonal = _endPosition - _startPosition;
      transform.localScale = Quaternion.Inverse(transform.rotation) * diagonal;
      transform.position = 0.5f * (_endPosition + _startPosition);
    }

    bool isButtonUp = _selectButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonUp(buttonId));
    if (isButtonUp)
    {
      GetComponent<Collider>().enabled = false;
      GetComponent<Renderer>().enabled = false;
    }
  }


  void OnTriggerEnter(Collider selectedCollider)
  {
    if ((1 << selectedCollider.gameObject.layer & _stylusSelector.layerMask) == 0)
      return;
      
    GameObject go = _stylusSelector.objectResolver(selectedCollider.gameObject);

    if (_overlapCounts.ContainsKey(go))
      ++_overlapCounts[go];
    else
      _stylusSelector.selectedObjects.Add(go);
  }


  void OnTriggerExit(Collider selectedCollider)
  {
    GameObject go = _stylusSelector.objectResolver(selectedCollider.gameObject);
    if ((1 << go.layer & _stylusSelector.layerMask) == 0)
      return;

    if (_overlapCounts.ContainsKey(go) && _overlapCounts[go] > 1)
    {
      --_overlapCounts[go];
    }
    else
    {
      _overlapCounts.Remove(go);
      _stylusSelector.selectedObjects.Remove(go);
    }
  }
}
