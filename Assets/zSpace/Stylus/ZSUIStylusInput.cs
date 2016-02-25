////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Interface class representing the current state of a stylus.
/// </summary>
public abstract class ZSUIStylusInput : ZSUMonoBehavior
{
    /// <summary>
    /// The contact point between the stylus and the hovered object.
    /// If there is no contact, this is the "hot spot" or point of interest, such as the end of the virtual or physical stylus.
    /// </summary>
    public abstract Vector3 HoverPoint { get; protected set; }

    /// <summary>
    /// The object that is currently under the stylus, if any.  At most one object can be hovered at a time.
    /// </summary>
    public abstract GameObject HoverObject { get; protected set; }

    /// <summary>
    /// The forward vector of the stylus.
    /// </summary>
    public abstract Vector3 Direction { get; protected set; }

    /// <summary>
    /// Is the given button currently down?
    /// </summary>
    public abstract bool GetButton(int buttonId);

    /// <summary>
    /// Was the given button pressed since the last update?
    /// </summary>
    public abstract bool GetButtonDown(int buttonId);

    /// <summary>
    /// Was the given button released during the last update?
    /// </summary>
    public abstract bool GetButtonUp(int buttonId);

    /// <summary>
    /// The ID of the stylus button that will be used for selecting objects.
    /// </summary>
    public int SelectButton = 0;
}