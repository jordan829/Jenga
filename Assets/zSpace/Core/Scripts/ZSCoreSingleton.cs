//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2014 zSpace, Inc.  All Rights Reserved.
//
//  File:       ZSCoreSingleton.cs
//  Content:    The zSpace Core Singleton for Unity.
//
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ZSCoreSingleton : MonoBehaviour
{
    #region UNITY CALLBACKS

    void Start()
    {
        if (_isInitialized)
        {
            // Initialize left/right detect.
            GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetect);
        }
    }

    void OnApplicationQuit()
    {
        if (_isInitialized)
        {
            _isInitialized = false;
            zsupShutdown();
        }

        _instance = null;
    }

    #endregion


    #region PUBLIC PROPERTIES

    public static ZSCoreSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(ZSCoreSingleton)) as ZSCoreSingleton;

                if (_instance == null)
                {
                    // Create a new ZSCoreSingleton GameObject.
                    GameObject instanceObject = new GameObject("ZSCoreSingleton", typeof(ZSCoreSingleton));
                    instanceObject.hideFlags  = HideFlags.HideInHierarchy;
                    
                    // Do not destroy the instance's GameObject on scene change.
                    DontDestroyOnLoad(instanceObject);

                    // Get a reference to the ZSCoreSingleton script component.
                    _instance = instanceObject.GetComponent<ZSCoreSingleton>();

                    if (_instance == null)
                        Debug.Log("A serious error has occurred: Could not create ZSCoreSingleton GameObject.");
                }
            }

            return _instance;
        }
    }

    public bool                   IsInitialized           { get { return _isInitialized; } }
    public bool                   IsFocusSyncEnabled      { get; set; }
    public ZSCore.AutoStereoState AutoStereoState         { get; set; }
    public bool                   DefaultMouseCursorState { get; private set; }
    
    #endregion


    #region PRIVATE METHODS

    private ZSCoreSingleton()
    {
        // Initialize the zSpace Unity plugin.
        int error = zsupInitialize();

        if (error == (int)ZSCore.PluginError.Okay || 
            error == (int)ZSCore.PluginError.AlreadyInitialized)
        {
            _isInitialized = true;
        }
        else
        {
            _isInitialized = false;
            Debug.Log("Failed to initialize zSpace. [Error: " + (ZSCore.PluginError)error + "]");
        }

        // Check to see if the graphics device is initialized.
        // If not, report that stereo will be disabled.
        if (!zsupIsGraphicsDeviceInitialized())
        {
            Debug.Log("Failed to initialize graphics device. Disabling stereoscopic 3D. " +
                      "To enable stereoscopic 3D, please use -force-opengl and -enablestereoscopic3d flags.");
        }

        // Set whether or not Unity is running in the editor.
        zsupSetRunningInEditor(Application.isEditor);

        // Initialize the focus sync state.
        this.IsFocusSyncEnabled = false;

        // Initialize the auto stereo state to idle stereo.
        this.AutoStereoState = ZSCore.AutoStereoState.AnimateToStereo;

        // Initialize the default mouse cursor state.
        this.DefaultMouseCursorState = Cursor.visible;
    }

    #endregion


    #region PRIVATE MEMBERS

    private static ZSCoreSingleton _instance;
    private bool _isInitialized = false;

    #endregion


    #region ZSPACE PLUGIN IMPORT DECLARATIONS

    [DllImport("zSpaceUnity")]
    private static extern bool zsupIsGraphicsDeviceInitialized();
    [DllImport("zSpaceUnity")]
    private static extern void zsupSetRunningInEditor(bool isRunningInEditor);

    // General API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupInitialize();
    [DllImport("zSpaceUnity")]
    private static extern int  zsupShutdown();
    
    #endregion
}
