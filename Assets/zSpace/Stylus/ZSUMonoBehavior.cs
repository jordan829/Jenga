////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

/// <summary>
/// Wrapper class for MonoBehaviour with separate functions for in-editor and runtime events.
/// </summary>
public class ZSUMonoBehavior : MonoBehaviour
{
    #region Unity Callback Forwarding
    void Awake()
    {
        if (IsPlaying)
        {
            OnScriptAwake();
        }
        else
        {
            OnEditorAwake();
        }
    }

    void OnEnable()
    {
        if (IsPlaying)
        {
            OnScriptEnable();
        }
        else
        {
            OnEditorEnable();
        }
    }

    void Start()
    {
        if (IsPlaying)
        {
            OnScriptStart();
        }
        else
        {
            OnEditorStart();
        }
    }

    void Update()
    {
        if (IsPlaying)
        {
            OnScriptUpdate();
        }
        else
        {
            OnEditorUpdate();
        }
    }

    void LateUpdate()
    {
        if (IsPlaying)
        {
            OnScriptLateUpdate();
        }
        else
        {
            OnEditorLateUpdate();
        }
    }

    void OnGUI()
    {
        if (IsPlaying)
        {
            OnScriptGUI();
        }
        else
        {
            OnEditorGUI();
        }
    }

    void OnDisable()
    {
        if (IsPlaying)
        {
            OnScriptDisable();
        }
        else
        {
            OnEditorDisable();
        }
    }

    void OnDestroy()
    {
        if (IsPlaying)
        {
            OnScriptDestroy();
        }
        else
        {
            OnEditorDestroy();
        }
    }

    void OnDrawGizmos()
    {
        OnScriptDrawGizmos();
    }

    void OnDrawGizmosSelected()
    {
        OnScriptDrawGizmosSelected();
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        OnScriptTriggerEnter(otherCollider);
    }

    void OnTriggerExit(Collider otherCollider)
    {
        OnScriptTriggerExit(otherCollider);
    }
    #endregion


    protected virtual void OnScriptAwake()
    {

    }

    protected virtual void OnScriptEnable()
    {

    }

    protected virtual void OnScriptStart()
    {

    }

    protected virtual void OnScriptUpdate()
    {

    }

    protected virtual void OnScriptLateUpdate()
    {

    }

    protected virtual void OnScriptGUI()
    {

    }

    protected virtual void OnScriptDisable()
    {

    }

    protected virtual void OnScriptDestroy()
    {

    }

    protected virtual void OnScriptDrawGizmos()
    {
        // TODO: rename to OnEditorDrawGizmos()?
    }

    protected virtual void OnScriptDrawGizmosSelected()
    {

    }

    protected virtual void OnScriptTriggerEnter(Collider otherCollider)
    {

    }

    protected virtual void OnScriptTriggerExit(Collider otherCollider)
    {

    }

    protected virtual void OnEditorAwake()
    {

    }

    protected virtual void OnEditorEnable()
    {

    }

    protected virtual void OnEditorStart()
    {

    }

    protected virtual void OnEditorUpdate()
    {

    }

    protected virtual void OnEditorLateUpdate()
    {

    }

    protected virtual void OnEditorGUI()
    {

    }

    protected virtual void OnEditorDisable()
    {

    }

    protected virtual void OnEditorDestroy()
    {

    }
    

    private bool IsPlaying
    {
        get
        {
            return Application.isPlaying;
        }
    }
}
