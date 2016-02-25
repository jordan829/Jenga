////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Stylus shape with a tip, beam, and base.
/// </summary>
/// <remarks>
/// Provides functions for fitting the beam and positioning the tip and base along it.
/// Supports an optional gap at the tip or base to allow for end-point geometry.
/// </remarks>
public abstract class ZSLinearShape : ZSStylusShape
{
    /// <summary>
    /// The stylus tip.  This visually represents the front end of the stylus.
    /// </summary>
    public GameObject _tip;

    /// <summary>
    /// The stylus beam.  This visually represents the middle of the stylus, connecting the tip and base.
    /// </summary>
    public GameObject _beam;

    /// <summary>
    /// The stylus base.  This visually represents the back end of the stylus.
    /// </summary>
    public GameObject _base;
 
    /// <summary>
    /// The nominal total length of the stylus.  If any auto-sizing occurs, this is the starting length.
    /// </summary>
    public float _defaultLength = 0.2f;
 
    /// <summary>
    /// The length of the stylus tip object in meters.  The beam will auto fit to this distance from the tip's origin.
    /// </summary>
    public float _tipLength = 0f;

    /// <summary>
    /// The length of the stylus base object.  The beam will auto fit to this distance from the base's origin.
    /// </summary>
    public float _baseLength = 0f;
 
    /// <summary>
    /// The increment used to increase or decrease the stylus length.
    /// </summary>
    public float _resizeSpeed = 1f;

    protected float _beamLength = 1f;
 
    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        _defaultLength = Mathf.Max(_defaultLength, _tipLength + _baseLength);

        if (_beam != null)
		  _beam.transform.localPosition = _baseLength * Vector3.forward;

        _beamLength = (_tip.transform.position - transform.position).magnitude - _tipLength - _baseLength;
     
        ScaleLengthBy(1f);
    }

    protected override void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
            ScaleLengthBy(1.0f + Time.deltaTime * _resizeSpeed);
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            ScaleLengthBy(1.0f / (1.0f + Time.deltaTime * _resizeSpeed));
    }

    protected void ScaleLengthBy(float scaleFactor)
    {
        _defaultLength = Mathf.Max(_defaultLength * scaleFactor, _tipLength + _baseLength);

        if (_beam != null)
          _beam.transform.localScale = new Vector3(1.0f, 1.0f, (_defaultLength - _tipLength - _baseLength) / _beamLength);

        if (_tip != null)
          _tip.transform.localPosition = _defaultLength * Vector3.forward;
    }
}
