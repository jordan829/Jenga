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
/// Linear stylus shape with wand-like behavior.
/// Size is fixed, but can be increased or decreased manually by the user.
/// Contacts are sorted on distance from the stylus tip.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item> Checks + and - keys to increase/decrease stylus beam length. </item>
/// </list>
/// </remarks>
public class ZSWandShape : ZSLinearShape
{
    public override Vector3 hotSpot { get { return (_tip != null) ? _tip.transform.position : Vector3.zero; } }

    /// <summary>
    /// An object representing the volume selection (if any).
    /// </summary>
    public ZSSelectionVolume _selectionVolume;

    protected override void OnScriptEnable()
    {
        base.OnScriptEnable();
        if (_selectionVolume != null)
			_selectionVolume.gameObject.SetActive(true);
    }

    protected override void OnScriptDisable()
    {
        base.OnScriptDisable();
        if (_selectionVolume != null)
			_selectionVolume.gameObject.SetActive(false);
    }
}
