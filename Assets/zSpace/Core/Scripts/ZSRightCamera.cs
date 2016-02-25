//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2014 zSpace, Inc.  All Rights Reserved.
//
//  File:       ZSRightCamera.cs
//  Content:    The zSpace right camera script.
//
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class ZSRightCamera : MonoBehaviour
{
    void Start()
    {
        _core = GameObject.FindObjectOfType<ZSCore>();
    }

    void OnPreCull()
    {
        if (_core != null)
        {
            // Specify the correct eye.
            ZSCore.Eye eye = ZSCore.Eye.Right;

            if (_core.AreEyesSwapped())
                eye = ZSCore.Eye.Left;

            // Calculate right camera's transform.
            Matrix4x4 viewMatrixInverse = ZSCore.ConvertFromRightToLeft(_core.GetViewMatrix(eye).inverse);
            transform.localPosition = viewMatrixInverse.GetColumn(3);
            transform.localRotation = Quaternion.LookRotation(viewMatrixInverse.GetColumn(2), viewMatrixInverse.GetColumn(1));

            // Set the right camera's projection matrix.
            gameObject.GetComponent<Camera>().projectionMatrix = _core.GetProjectionMatrix(eye);
        }

        GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.RenderTargetRight);
    }

    private ZSCore _core = null;
}
