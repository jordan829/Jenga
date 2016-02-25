//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2014 zSpace, Inc.  All Rights Reserved.
//
//  File:       ZSCore.cs
//  Content:    The zSpace Core Interface for Unity.
//
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// Exposes the zSpace stereo and tracker APIs.
///
/// Use this class with the ZSCore prefab to maintain a zSpace stereo camera rig in Unity. 
/// It provides head-tracked stereo rendering. 
/// You can also use it to access information about zSpace tracker target poses and stereo frustum settings.
public class ZSCore : MonoBehaviour
{
    #region ENUMS
    /// @ingroup General
    /// Identifies rendering events defined by the native zSpace Unity plugin. 
    public enum GlPluginEventType
    {
        RenderTargetLeft         = 10000,
        RenderTargetRight        = 10001,
        FrameDone                = 10002,
        DisableStereo            = 10003,
        InitializeLRDetect       = 10004,
        UpdateLRDetectFullscreen = 10005,
        UpdateLRDetectWindowed   = 10006,
        SyncLRDetectFullscreen   = 10007,
        SyncLRDetectWindowed     = 10008
    }

    /// Identifies an error in the native zSpace Unity plugin.
    /// @ingroup General
    public enum PluginError
    {
        Okay                  = 0,
        NotImplemented        = 1,
        NotInitialized        = 2,
        AlreadyInitialized    = 3,
        InvalidParameter      = 4,
        InvalidContext        = 5,
        InvalidHandle         = 6,
        RuntimeIncompatible   = 7,
        RuntimeNotFound       = 8,
        SymbolNotFound        = 9,
        DisplayNotFound       = 10,
        DeviceNotFound        = 11,
        TargetNotFound        = 12,
        CapabilityNotFound    = 13,
        BufferTooSmall        = 14
    }

    /// Defines the coordinate spaces used by %ZSCore. 
    /// @ingroup CoordinateSpace
    public enum CoordinateSpace
    {
        Tracker  = 0,
        Display  = 1,
        Viewport = 2,
        Camera   = 3
    }

    /// @ingroup StereoFrustum
    /// Defines the attributes that you can set and query for the stereo frustum.
    /// These attributes are important for comfortable viewing of stereoscopic 3D.
    public enum FrustumAttribute
    {
        /// The physical separation, or inter-pupillary distance, between the eyes in meters.
        /// (Default: 0.06)
        Ipd           = 0,

        /// Viewer scale adjusts the display and head tracking for larger and smaller scenes. (Default: 1)
        ViewerScale   = 1, 
 
        /// Field of view scale for the frustum. (Default: 1)
        FovScale      = 2,

        /// Uniform scale factor applied to the frustum's incoming head pose. (Default: 1)
        HeadScale     = 3,

        /// Near clipping plane for the frustum in meters. (Default: 0.1)
        NearClip      = 4,

        /// Far clipping plane for the frustum in meters. (Default: 1000)
        FarClip       = 5,

        /// Distance between the bridge of the glasses and the bridge of the nose in meters. (Default: 0.01)
        GlassesOffset = 6,

        /// Maximum pixel disparity for crossed images (negative parallax) in the coupled zone. (Default: -100)
        /// The coupled zone refers to the area where our eyes can both comfortably converge and focus on an object. 
        CCLimit       = 7,

        /// Maximum pixel disparity for uncrossed images (positive parallax) in the coupled zone. (Default: 100)
        UCLimit       = 8,

        /// Maximum pixel disparity for crossed images (negative parallax) in the uncoupled zone. (Default: -200)
        CULimit       = 9,

        /// Maximum pixel disparity for uncrossed images (positive parallax) in the uncoupled zone. (Default: 250)
        UULimit       = 10,

        /// Maximum depth in meters for negative parallax in the coupled zone. (Default: 0.13)
        CCDepth       = 11,

        /// Maximum depth in meters for positive parallax in the coupled zone. (Default: -0.30)
        UCDepth       = 12
    }

    /// @ingroup StereoFrustum
    /// Defines options for positioning the scene relative to the physical display or relative to the viewport.
    public enum PortalMode
    {
        None     =  0, ///< The scene is positioned relative to the viewport.
        Angle    =  1, ///< The scene's orientation is fixed relative to the physical desktop.
        Position =  2, ///< The scene's position is fixed relative to the center of the display.
        All      = ~0  ///< All portal modes except "none" are enabled.
    }

    /// @ingroup StereoFrustum 
    /// Defines the eyes for the stereo frustum.
    public enum Eye
    {
        Left   = 0,
        Right  = 1,
        Center = 2,
        NumEyes
    }

    /// @ingroup General
    /// Identifies a specific camera managed by the %ZSCore stereo camera rig.
    public enum CameraType
    {
        Left  = 0,
        Right = 1,
        Final = 2,
        NumTypes
    }

    /// @ingroup TrackerTarget
    /// Identifies a 6-degree-of-freedom tracker target, such as the zSpace stylus or glasses.
    public enum TrackerTargetType
    {
        Unknown   = -1,  ///< The tracker target's type is unknown.
        Head      =  0,  ///< The tracker target corresponding to the user's head.
        Primary   =  1,  ///< The tracker target corresponding to the user's primary hand.
        Secondary =  2,  ///< The tracker target corresponding to the user's secondary hand. (Reserved for future use.)
        NumTypes
    }

    /// @ingroup TargetLed
    /// Defines a color to be displayed by the stylus' LED.
    public enum LedColor
    {
        Black   = 0,
        White   = 1,
        Red     = 2,
        Green   = 3,
        Blue    = 4,
        Cyan    = 5,
        Magenta = 6,
        Yellow  = 7
    }

    /// @ingroup MouseEmulation
    /// Determines how the stylus and mouse control the cursor when both are used.
    public enum MouseMovementMode
    {
        /// The stylus uses absolute positions.  
        /// In this mode, the mouse and stylus can fight for control of the cursor if both are in use.
        /// This is the default mode. 
        Absolute = 0,

        /// The stylus applies delta positions to the mouse cursor's current position.
        /// Movements by the mouse and stylus are compounded without fighting. 
        Relative = 1
    }

    /// @ingroup MouseEmulation
    /// Defines mouse buttons to be used when mapping a tracker target's buttons to a mouse.
    public enum MouseButton
    {
        Unknown = -1,
        Left    =  0,
        Right   =  1,
        Center  =  2
    }

    public enum AutoStereoState 
    { 
        IdleMono, 
        IdleStereo, 
        AnimateToMono, 
        AnimateToStereo 
    }

    #endregion


    #region STRUCTS

    /// @ingroup Display
    /// @brief Struct representing display intersection information.
    public struct DisplayIntersectionInfo
    {
        public bool  hit;      ///< Whether the display was intersected.
        public int   x;        ///< The x pixel coordinate on the virtual desktop.
        public int   y;        ///< The y pixel coordinate on the virtual desktop.
        public int   nx;       ///< The normalized absolute x pixel coordinate on the virtual desktop.
        public int   ny;       ///< The normalized absolute y pixel coordinate on the virtual desktop.
        public float distance; ///< The distance in meters from the raycast's origin to the intersection point on the display.

    }

    #endregion


    #region UNITY INSPECTOR

    /// @ingroup General
    /// The camera around which the stereo rig will be placed.
    public GameObject CurrentCamera  = null;

    /// @ingroup General
    /// Whether stereo rendering is enabled.
    public bool EnableStereo         = true;

    /// @ingroup General
    /// Whether stereo rendering will automatically turn on/off
    /// based on the visibility of the glasses and stylus.
    public bool EnableAutoStereo     = true;

    /// @ingroup General
    /// Whether head and stylus tracking are enabled.
    public bool EnableTracking       = true;

    /// @ingroup MouseEmulation
    /// Whether mouse emulation is enabled.
    public bool EnableMouseEmulation = false;
    
    /// @ingroup General
    /// Whether mouse will auto-hide after a specified time in seconds (MouseAutoHideDuration).
    public bool EnableMouseAutoHide  = false;

    /// @ingroup General
    /// The physical separation, or inter-pupillary distance, between the eyes in meters. 
    [Range(0.01f, 0.2f)]
    public float InterPupillaryDistance = 0.06f;

    /// @ingroup StereoFrustum
    /// Viewer scale adjusts the display and head tracking for larger and smaller scenes.
    /// Use larger values for scenes with large models and smaller values for smaller models.
    [Range(0.01f, 1000.0f)]
    public float ViewerScale = 1;

    /// @ingroup StereoFrustum
    /// Field of view scale for the frustum. 
    /// A value greater than 1 causes a wide angle effect, while a value less than 1 causes a zoom effect.  
    [Range(0.01f, 1000.0f)]
    public float FieldOfViewScale = 1;

    /// @ingroup StereoFrustum
    /// Uniform scale factor applied to the frustum's incoming head pose.   
    [Range(0, 1)]
    public float HeadTrackingScale = 1;

    /// @ingroup StereoFrustum
    /// Distance between the bridge of the glasses and the bridge of the nose in meters. 
    [Range(0.0f, 0.2f)]
    public float GlassesOffset = 0.01f;

    /// @ingroup General
    /// The time in seconds before the mouse cursor will be hidden if EnableMouseAutoHide is true.
    [Range(0.1f, 1000.0f)]
    public float MouseAutoHideTimeout = 3.0f;

    #endregion


    #region UNITY CALLBACKS

    void Awake()
    {
        // Check if the app is running in fake fullscreen mode.
        var commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        _isAlwaysWindowed = commandLineArgs.Contains("-always-windowed") && commandLineArgs.Contains("-popupwindow");

        // Grab the ZSCoreSingleton and verify that it is initialized.
        _coreSingleton = ZSCoreSingleton.Instance;

        // If the CurrentCamera is null, default to Camera.main.
        if (!this.IsCurrentCameraValid() && Camera.main != null)
            this.CurrentCamera = Camera.main.gameObject;

        // Initialization.
        this.Initialize();
        this.InitializeStereoCameras();
        this.CheckForUpdates();
        _wasFullScreen = Screen.fullScreen;

        // Temporarily re-enable the camera in case other MonoBehaviour scripts
        // want to reference Camera.main in their Awake() method.
        if (this.IsCurrentCameraValid())
            this.CurrentCamera.GetComponent<Camera>().enabled = true;

        if (_coreSingleton.IsInitialized)
        {
            // Set the window size.
            zsupSetViewportSize(Screen.width, Screen.height);

            // Start the update coroutine.
            StartCoroutine("UpdateCoroutine");
        }
    }

    void OnDestroy()
    {
        // Stop the update coroutine.
        StopCoroutine("UpdateCoroutine");

        if (_coreSingleton.IsInitialized)
        {
            GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.DisableStereo);
            GL.InvalidateState();
        }
    }

    void Update()
    {
        this.UpdateInternal();
    }

    void LateUpdate()
    {
        if (this.IsStereoEnabled())
            this.CurrentCamera.GetComponent<Camera>().enabled = false;

        if (_isAlwaysWindowed)
        {
            Screen.fullScreen = false;
        }
    }

    void OnApplicationFocus(bool focusStatus)
    {
        if (_coreSingleton.IsFocusSyncEnabled && focusStatus)
        {
            if (SystemInfo.graphicsDeviceVendorID == 4098)
            {
                if (Screen.fullScreen == true)
                    GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.SyncLRDetectFullscreen);
            }
        }

        _coreSingleton.IsFocusSyncEnabled = true;
    }

    #endregion


    #region ZSPACE APIS
    //////////////////////////////////////////////////////////////////
    /// @defgroup General General 
    /// The functions in this group provide general information about
    /// the status of stereo output and device tracking.
    /// @{
    //////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Set whether stereoscopic 3D is enabled.
    /// </summary>
    /// <param name="isStereoEnabled">True to enable stereoscopic 3D.  False if not.</param>
    public void SetStereoEnabled(bool isStereoEnabled)
    {
        if (!_coreSingleton.IsInitialized)
            return;

        if (this.IsCurrentCameraValid())
        {
            this.CurrentCamera.GetComponent<Camera>().enabled = !isStereoEnabled;

            if (_stereoCameras[(int)CameraType.Left] != null)
                _stereoCameras[(int)CameraType.Left].enabled = isStereoEnabled;

            if (_stereoCameras[(int)CameraType.Right] != null)
                _stereoCameras[(int)CameraType.Right].enabled = isStereoEnabled;

            this.EnableStereo = isStereoEnabled;
            _isStereoEnabled  = isStereoEnabled;
        }
    }

    /// <summary>
    /// Check whether stereoscopic 3D rendering is enabled.
    /// </summary>
    /// <returns>True if stereoscopic 3D is enabled.  False if not.</returns>
    public bool IsStereoEnabled()
    {
        return _isStereoEnabled;
    }

    /// <summary>
    /// Set whether tracking is enabled.
    /// </summary>
    /// <param name="isEnabled">True if tracking is enabled. False if not.</param>
    public void SetTrackingEnabled(bool isEnabled)
    {
        this.EnableTracking = isEnabled;
        zsupSetTrackingEnabled(isEnabled);
    }

    /// <summary>
    /// Check whether tracking is enabled.
    /// </summary>
    /// <returns>True if tracking is enabled. False if not.</returns>
    public bool IsTrackingEnabled()
    {
        bool isTrackingEnabled = false;
        zsupIsTrackingEnabled(out isTrackingEnabled);
        return isTrackingEnabled;
    }

    /// <summary>
    /// Set whether the left and right eyes are swapped.
    /// </summary>
    /// <param name="areEyesSwapped">True if the left and right eyes are swapped. False if not.</param>
    public void SetEyesSwapped(bool areEyesSwapped)
    {
        _areEyesSwapped = areEyesSwapped;
    }

    /// <summary>
    /// Check whether the left and right eyes are swapped.
    /// </summary>
    /// <returns>True if the left and right eyes are swapped. False if not.</returns>
    public bool AreEyesSwapped()
    {
        return _areEyesSwapped;
    }
/// @}


    //////////////////////////////////////////////////////////////////
    /// @defgroup Display Display 
    /// The functions in this group provide information about the display.
    /// @{
    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the virtual (x, y) position of the current display.
    /// </summary>
    /// <returns>The display position (virtual x, y coordinates) in Vector2 format.</returns>
    public Vector2 GetDisplayPosition()
    {
        int x = 0;
        int y = 0;
        zsupGetDisplayPosition(out x, out y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get the size of the current display.
    /// </summary>
    /// <returns>The display size (in meters) in Vector2 format.</returns>
    public Vector2 GetDisplaySize()
    {
        Vector2 displaySize = new Vector2();
        zsupGetDisplaySize(out displaySize.x, out displaySize.y);
        return displaySize;
    }

    /// <summary>
    /// Get the resolution of the current display.
    /// </summary>
    /// <returns>The display resolution (in pixels) in Vector2 format.</returns>
    public Vector2 GetDisplayResolution()
    {
        int x = 0;
        int y = 0;
        zsupGetDisplayResolution(out x, out y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get the angle of the current display.
    /// </summary>
    /// <returns>The display angle (in degrees) in Vector2 format.</returns>
    public Vector3 GetDisplayAngle()
    {
        Vector3 displayAngle = new Vector3();
        zsupGetDisplayAngle(out displayAngle.x, out displayAngle.y, out displayAngle.z);
        return displayAngle;
    }
    
    /// <summary>
    /// DEPRECATED: Use GetCameraOffset() instead.
    /// </summary>
    public Vector3 GetDisplayOffset()
    {
        return -(this.GetCameraOffset());
    }

    /// <summary>
    /// Check whether the display hardware is present (USB connected).
    /// </summary>
    /// <returns>True if the hardware is present. False if not.</returns>
    public bool IsDisplayHardwarePresent()
    {
        bool isHardwarePresent = false;
        zsupIsDisplayHardwarePresent(out isHardwarePresent);
        return isHardwarePresent;
    }

    /// <summary>
    /// Perform a raycast against the zSpace display given
    /// a specified tracker space pose.
    /// </summary>
    /// <param name="pose">Pose in tracker space.</param>
    /// <returns>Display intersection information.</returns>
    public DisplayIntersectionInfo IntersectDisplay(Matrix4x4 pose)
    {
        DisplayIntersectionInfo info = new DisplayIntersectionInfo();
        zsupIntersectDisplay(this.ConvertToFloatArray(pose), out info.hit, out info.x, out info.y, out info.nx, out info.ny, out info.distance);
        return info;
    }
/// @}

    //////////////////////////////////////////////////////////////////////////
    /// @defgroup StereoViewport Stereo Viewport 
    /// %ZSCore creates a stereo viewport for you, as well as the associated stereo frustum.
    /// Use these functions to get the stereo viewport's position and size.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the viewport's top-left corner in pixels.
    /// </summary>
    /// <param name="x">Left-most position.</param>
    /// <param name="y">Top-most position.</param>
    public void GetViewportPosition(out int x, out int y)
    {
        zsupGetViewportPosition(out x, out y);
    }

    /// <summary>
    /// Get the viewport's size in pixels.
    /// </summary>
    /// <param name="width">Viewport width.</param>
    /// <param name="height">Viewport height.</param>
    public void GetViewportSize(out int width, out int height)
    {
        zsupGetViewportSize(out width, out height);
    }
    
    /// <summary>
    /// DEPRECATED: Use GetCoordinateSpaceTransform() instead.
    /// </summary>
    public Vector3 GetViewportOffset()
    {
        Matrix4x4 viewportToDisplaySpace = this.GetCoordinateSpaceTransform(CoordinateSpace.Viewport, CoordinateSpace.Display);
        return viewportToDisplaySpace.GetColumn(3);
    }    
/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup CoordinateSpace Coordinate Space 
    /// Use these functions for operations that require coordinate space transformations.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the transformation matrix from coordinate space a to b.
    /// </summary>
    /// <param name="a">The source coordinate space.</param>
    /// <param name="b">The destination coordinate space.</param>
    /// <returns>Coordinate space transformation matrix.</returns>
    public Matrix4x4 GetCoordinateSpaceTransform(CoordinateSpace a, CoordinateSpace b)
    {
        float[] transformData = new float[16];
        zsupGetCoordinateSpaceTransform((int)a, (int)b, transformData);
        return this.ConvertToMatrix4x4(transformData);
    }
    
    /// <summary>
    /// DEPRECATED: Use GetCoordinateSpaceTransform() instead.
    /// </summary>
    public Matrix4x4 GetTrackerToCameraSpaceTransform()
    {
        return this.GetCoordinateSpaceTransform(CoordinateSpace.Tracker, CoordinateSpace.Camera);
    }
/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup StereoFrustum Stereo Frustum 
    /// Use these functions to fine tune the stereo frustum's stereoscopic settings 
    /// and to query information such as projections and view matrices.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set the physical separation, or inter-pupillary distance, between the eyes in meters.
    /// </summary>
    /// <remarks>
    /// An IPD of 0 will effectively disable stereo since the eyes are assumed
    /// to be at the same location.
    /// </remarks>
    /// <param name="interPupillaryDistance">The physical distance in meters. Default: 0.06</param>
    public void SetInterPupillaryDistance(float interPupillaryDistance)
    {
        this.InterPupillaryDistance = interPupillaryDistance;
        if (!EnableAutoStereo)
            zsupSetFrustumAttribute((int)FrustumAttribute.Ipd, interPupillaryDistance);
    }

    /// <summary>
    /// Get the physical distance between the user's eyes.
    /// </summary>
    /// <returns>The physical distance in meters.</returns>
    public float GetInterPupillaryDistance()
    {
        if (EnableAutoStereo)
            return this.InterPupillaryDistance;

        float interPupillaryDistance = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.Ipd, out interPupillaryDistance);
        return interPupillaryDistance;
    }

    /// <summary>
    /// Set the viewer scale.
    /// </summary>
    /// <remarks>
    /// Viewer scale adjusts the display and head tracking for larger and smaller scenes.
    /// Use larger values for scenes with large models and smaller values for smaller models.
    /// </remarks>
    /// <param name="viewerScale">The viewer scale. Default: 1</param>
    public void SetViewerScale(float viewerScale)
    {
        this.ViewerScale = viewerScale;
        zsupSetFrustumAttribute((int)FrustumAttribute.ViewerScale, viewerScale);
    }

    /// <summary>
    /// Get the viewer scale.
    /// </summary>
    /// <returns>The viewer scale.</returns>
    public float GetViewerScale()
    {
        float viewerScale = 1.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.ViewerScale, out viewerScale);
        return viewerScale;
    }

    /// <summary>
    /// Set the field of view scale for the frustum. 
    /// </summary>
    /// <remarks>
    /// A value greater than 1 causes a wide angle effect, while a value less than 1 causes a zoom effect.
    /// However, large changes to the field of view scale will interfere 
    /// with the mapping between the physical and virtual stylus.
    /// </remarks> 
    /// <param name="fieldOfViewScale">The field of view scale. Default: 1</param>
    public void SetFieldOfViewScale(float fieldOfViewScale)
    {
        this.FieldOfViewScale = fieldOfViewScale;
        zsupSetFrustumAttribute((int)FrustumAttribute.FovScale, fieldOfViewScale);
    }

    /// <summary>
    /// Get the field of view scale.
    /// </summary>
    /// <returns>The field of view scale.</returns>
    public float GetFieldOfViewScale()
    {
        float fieldOfViewScale = 1.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.FovScale, out fieldOfViewScale);
        return fieldOfViewScale;
    }

    /// <summary>
    /// Set the uniform scale to be applied to the head tracked position. 
    /// </summary>
    /// <param name="headTrackingScale">The scale applied to head tracking. Default: 1</param>
    public void SetHeadTrackingScale(float headTrackingScale)
    {
        this.HeadTrackingScale = headTrackingScale;
        zsupSetFrustumAttribute((int)FrustumAttribute.HeadScale, headTrackingScale);
    }

    /// <summary>
    /// Get the uniform scale that is applied to the head tracked position.
    /// </summary>
    /// <returns>The scale applied to head tracking.</returns>
    public float GetHeadTrackingScale()
    {
        float headTrackingScale = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.HeadScale, out headTrackingScale);
        return headTrackingScale;
    }

    /// <summary>
    /// Set the glasses offset for the stereo frustum in meters. 
    /// </summary>
    /// <param name="glassesOffset">
    /// The distance between the bridge of the glasses and the bridge of the nose in meters.
    /// Default: 1</param>
    public void SetGlassesOffset(float glassesOffset)
    {
        this.GlassesOffset = glassesOffset;
        zsupSetFrustumAttribute((int)FrustumAttribute.GlassesOffset, glassesOffset);
    }

    /// <summary>
    /// Get the glasses offset for the stereo frustum.
    /// </summary>
    /// <returns>The distance between the bridge of the glasses and the bridge of the nose in meters.</returns>
    public float GetGlassesOffset()
    {
        float glassesOffset = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.GlassesOffset, out glassesOffset);
        return glassesOffset;
    }

    /// <summary>
    /// Set the near clip distance for the frustum in meters. 
    /// </summary>
    /// <param name="nearClip">The near clip distance in meters. Default: 0.1</param>
    public void SetNearClip(float nearClip)
    {
        zsupSetFrustumAttribute((int)FrustumAttribute.NearClip, nearClip);

        if (_stereoCameras[(int)CameraType.Left] != null)
            _stereoCameras[(int)CameraType.Left].nearClipPlane = nearClip;

        if (_stereoCameras[(int)CameraType.Right] != null)
            _stereoCameras[(int)CameraType.Right].nearClipPlane = nearClip;
    }

    /// <summary>
    /// Get the near clip distance.
    /// </summary>
    /// <returns>The near clip distance in meters.</returns>
    public float GetNearClip()
    {
        float nearClip = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.NearClip, out nearClip);
        return nearClip;
    }

    /// <summary>
    /// Set the far clip distance for the frustum in meters. 
    /// </summary>
    /// <param name="farClip">The far clip distance in meters. Default: 1000</param>
    public void SetFarClip(float farClip)
    {
        zsupSetFrustumAttribute((int)FrustumAttribute.FarClip, farClip);

        if (_stereoCameras[(int)CameraType.Left] != null)
            _stereoCameras[(int)CameraType.Left].farClipPlane = farClip;

        if (_stereoCameras[(int)CameraType.Right] != null)
            _stereoCameras[(int)CameraType.Right].farClipPlane = farClip;
    }

    /// <summary>
    /// Get the far clip distance.
    /// </summary>
    /// <returns>The far clip distance in meters.</returns>
    public float GetFarClip()
    {
        float farClip = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.FarClip, out farClip);
        return farClip;
    }

    /// <summary>
    /// Set the frustum's portal mode.
    /// </summary>
    /// <param name="portalModeFlags">A bitmask for the portal mode flags.</param>
    public void SetPortalMode(int portalModeFlags)
    {
        zsupSetFrustumPortalMode(portalModeFlags);
    }

    /// <summary>
    /// Get the frustum's portal mode.
    /// </summary>
    /// <returns>A bitmask for the portal mode flags.</returns>
    public int GetPortalMode()
    {
        int portalModeFlags = 0;
        zsupGetFrustumPortalMode(out portalModeFlags);
        return portalModeFlags;
    }

    /// <summary>
    /// Set the frustum's camera offset. This is the distance from the center of
    /// viewport to the virtual camera.
    /// </summary>
    /// <param name="cameraOffset">The desired camera offset in meters.</param>
    public void SetCameraOffset(Vector3 cameraOffset)
    {
        zsupSetFrustumCameraOffset(this.ConvertToFloatArray(cameraOffset));
    }

    /// <summary>
    /// Get the frustum's camera offset. This is the distance from the center of
    /// viewport to the virtual camera.
    /// </summary>
    /// <returns>The camera offset in meters.</returns>
    public Vector3 GetCameraOffset()
    {
        float[] cameraOffsetData = new float[3];
        zsupGetFrustumCameraOffset(cameraOffsetData);
        return this.ConvertToVector3(cameraOffsetData);
    }
    
    /// <summary>
    /// Get the view matrix for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The view matrix in Matrix4x4 format.</returns>
    public Matrix4x4 GetViewMatrix(Eye eye)
    {
        return _viewMatrices[(int)eye];
    }

    /// <summary>
    /// Get the projection matrix for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The projection matrix in Matrix4x4 format.</returns>
    public Matrix4x4 GetProjectionMatrix(Eye eye)
    {
        return _projectionMatrices[(int)eye];
    }

    /// <summary>
    /// Get the position of a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The position of the eye in Vector3 format.</returns>
    public Vector3 GetEyePosition(Eye eye)
    {
        float[] positionData = new float[3];
        zsupGetFrustumEyePosition((int)eye, positionData);
        return this.ConvertToVector3(positionData);
    }

    /// <summary>
    /// Get the frustum bounds for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <param name="bounds">The frustum bounds corresponding to a specified eye laid out as follows:<br>
    /// [left, right, bottom, top, nearClip, farClip]</param>
    public void GetFrustumBounds(Eye eye, float[/*6*/] bounds)
    {
        zsupGetFrustumBounds((int)eye, bounds);
    }
/// @}


    /////////////////////////////////////////////////////////////////////////
    /// @defgroup TrackerTarget Tracker Target 
    /// Upon initialization, %ZSCore creates instances of the tracker target 
    /// and registers them to the tracker device.
    /// The zSpace display's built-in tracking cameras are an example of a tracker device.
    /// The zSpace stylus and polarized glasses are the default tracker targets.
    /// Other peripheral devices, such as mice, can also be tracker targets.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set whether a specified tracker target is enabled.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="isEnabled">True to enable the target. False if not.</param>
    public void SetTrackerTargetEnabled(TrackerTargetType trackerTargetType, bool isEnabled)
    {
        zsupSetTargetEnabled((int)trackerTargetType, isEnabled);
    }

    /// <summary>
    /// Check whether a specified tracker target is enabled.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if  the target is enabled. False if not.</returns>
    public bool IsTrackerTargetEnabled(TrackerTargetType trackerTargetType)
    {
        bool isEnabled = false;
        zsupIsTargetEnabled((int)trackerTargetType, out isEnabled);
        return isEnabled;
    }

    /// <summary>
    /// Check whether a specified tracker target is visible.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if the target is visible. False if not.</returns>
    public bool IsTrackerTargetVisible(TrackerTargetType trackerTargetType)
    {
        this.UpdateInternal();

        return _isTrackerTargetVisible[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the tracker space pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>The Matrix4x4 pose in tracker space.</returns>
    public Matrix4x4 GetTrackerTargetPose(TrackerTargetType trackerTargetType)
    {
        this.UpdateInternal();

        return _trackerTargetPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the camera space pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>The Matrix4x4 pose in camera space.</returns>
    public Matrix4x4 GetTrackerTargetCameraPose(TrackerTargetType trackerTargetType)
    {
        this.UpdateInternal();

        return _trackerTargetCameraPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the world space pose of a specified default tracker target.
    /// This forces a recalculation based on the current camera's local
    /// to world matrix.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>The Matrix4x4 pose in world space.</returns>
    public Matrix4x4 GetTrackerTargetWorldPose(TrackerTargetType trackerTargetType)
    {
        this.UpdateInternal();

        Matrix4x4 trackerTargetWorldPose = _trackerTargetCameraPoses[(int)trackerTargetType];

        // Scale the position based on world and field of view scales.
        trackerTargetWorldPose[0, 3] *= this.FieldOfViewScale;
        trackerTargetWorldPose[1, 3] *= this.FieldOfViewScale;
      
        // Convert the camera space pose to world space.
        if (this.IsCurrentCameraValid())
            trackerTargetWorldPose = this.CurrentCamera.transform.localToWorldMatrix * trackerTargetWorldPose;

        return trackerTargetWorldPose;
    }

    /// <summary>
    /// Get the cached world space pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>The Matrix4x4 pose in world space.</returns>
    public Matrix4x4 GetCachedTrackerTargetWorldPose(TrackerTargetType trackerTargetType)
    {
        this.UpdateInternal();

        return _trackerTargetWorldPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Set whether pose buffering is enabled for a specified tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="isPoseBufferingEnabled">True to enable pose buffering. False if not.</param>
    public void SetTrackerTargetPoseBufferingEnabled(TrackerTargetType trackerTargetType, bool isPoseBufferingEnabled)
    {
        zsupSetTargetPoseBufferingEnabled((int)trackerTargetType, isPoseBufferingEnabled);
    }

    /// <summary>
    /// Check whether pose buffering is enabled for a specified tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if pose buffering is enabled.  False if not.</returns>
    public bool IsTrackerTargetPoseBufferingEnabled(TrackerTargetType trackerTargetType)
    {
        bool isPoseBufferingEnabled = false;
        zsupIsTargetPoseBufferingEnabled((int)trackerTargetType, out isPoseBufferingEnabled);
        return isPoseBufferingEnabled;
    }

    /// <summary>
    /// Get the tracker space buffered pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="lookBackTime">The amount of time in seconds to look back from the current time.</param>
    /// <returns>The most recent buffered pose in tracker space since the lookBackTime.</returns>
    public Matrix4x4 GetTrackerTargetBufferedPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        double timestamp = 0.0;
        zsupGetTargetBufferedPose((int)trackerTargetType, lookBackTime, matrixData, out timestamp);
        return this.ConvertToMatrix4x4(matrixData);
    }

    /// <summary>
    /// Get the camera space buffered pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="lookBackTime">The amount of time in seconds to look back from the current time.</param>
    /// <returns>The most recent buffered pose in camera space since the lookBackTime.</returns>
    public Matrix4x4 GetTrackerTargetBufferedCameraPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        double timestamp = 0.0;
        zsupGetTargetBufferedCameraPose((int)trackerTargetType, lookBackTime, matrixData, out timestamp);

        Matrix4x4 trackerTargetCameraPose = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(matrixData));
        return trackerTargetCameraPose;
    }

    /// <summary>
    /// Get the world space buffered pose of a specified default tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="lookBackTime">The amount of time in seconds to look back from the current time.</param>
    /// <returns>The most recent buffered pose in world space since the lookBackTime.</returns>
    public Matrix4x4 GetTrackerTargetBufferedWorldPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        double timestamp = 0.0;
        zsupGetTargetBufferedCameraPose((int)trackerTargetType, lookBackTime, matrixData, out timestamp);

        Matrix4x4 trackerTargetWorldPose = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(matrixData));

        // Scale the position based on world and field of view scales.
        trackerTargetWorldPose[0, 3] *= this.FieldOfViewScale;
        trackerTargetWorldPose[1, 3] *= this.FieldOfViewScale;
      
        // Convert the camera space pose to world space.
        if (this.IsCurrentCameraValid())
            trackerTargetWorldPose = CurrentCamera.transform.localToWorldMatrix * trackerTargetWorldPose;

        return trackerTargetWorldPose;
    }
/// @}


    //////////////////////////////////////////////////////////////////////////
    ///@defgroup TargetButton Target Button 
    /// Use these functions to determine how many buttons are on a tracker target
    /// and determine the button state.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the number of buttons associated with a specified tracker target.
    /// </summary>
    /// <param name="trackerTargetType">The type of the tracker target.</param>
    /// <returns>The number of buttons contained by a tracker target.</returns>
    public int GetNumTrackerTargetButtons(TrackerTargetType trackerTargetType)
    {
        int numButtons = 0;
        zsupGetNumTargetButtons((int)trackerTargetType, out numButtons);
        return numButtons;
    }

    /// <summary>
    /// Check whether a specified target button is pressed.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="buttonId">The ID of the button.</param>
    /// <returns>True if the button is pressed. False if not.</returns>
    public bool IsTrackerTargetButtonPressed(TrackerTargetType trackerTargetType, int buttonId)
    {
        bool isPressed = false;
        zsupIsTargetButtonPressed((int)trackerTargetType, buttonId, out isPressed);
        return isPressed;
    }
/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup TargetLed Target LED 
    /// Use these functions to control the LED light on a tracker target.
    /// Currently, only the zSpace stylus has this capability.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set whether the tracker target's LED is enabled.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="isLedEnabled">True to enable the LED. False if not.</param>
    public void SetTrackerTargetLedEnabled(TrackerTargetType trackerTargetType, bool isLedEnabled)
    {
        zsupSetTargetLedEnabled((int)trackerTargetType, isLedEnabled);
    }
    
    /// <summary>
    /// Check whether the tracker target's LED is enabled.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if the LED is enabled. False if not.</returns>
    public bool IsTrackerTargetLedEnabled(TrackerTargetType trackerTargetType)
    {
        bool isLedEnabled = false;
        zsupIsTargetLedEnabled((int)trackerTargetType, out isLedEnabled);
        return isLedEnabled;
    }

    /// <summary>
    /// Check whether the tracker target's LED is on.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if the LED is on. False if not.</returns>
    public bool IsTrackerTargetLedOn(TrackerTargetType trackerTargetType)
    {
        bool isLedOn = false;
        zsupIsTargetLedOn((int)trackerTargetType, out isLedOn);
        return isLedOn;
    }

    /// <summary>
    /// Set the tracker target's LED color.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="ledColor">The color of the LED.</param>
    public void SetTrackerTargetLedColor(TrackerTargetType trackerTargetType, LedColor ledColor)
    {
        int[] color = _ledColors[(int)ledColor];
        zsupSetTargetLedColor((int)trackerTargetType, color[0], color[1], color[2]);
    }


    /// <summary>
    /// Get the tracker target's LED color.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>The color of the LED.</returns>
    public LedColor GetTrackerTargetLedColor(TrackerTargetType trackerTargetType)
    {
        float r = 0;
        float g = 0;
        float b = 0;
        zsupGetTargetLedColor((int)trackerTargetType, out r, out g, out b);

        for (int i = 0; i < _ledColors.Count; ++i)
        {
            int[] color = _ledColors[i];

            if ((int)r == color[0] && (int)g == color[1] && (int)b == color[2])
                return (LedColor)i;
        }

        return LedColor.Black;
    }
/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup TargetVibration Target Vibration 
    /// Use these functions to control the vibration of a tracker target.
    /// Currently only the zSpace stylus supports this capability.
    /// The vibration consists of alternating on and off periods.
    /// You can specify the length of the on and off periods, as well as
    /// how many times the vibration repeats.
    /// @{
    //////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Set whether the tracker target's vibration is enabled.  </summary>
    /// <remarks>This only determines
    /// whether the appropriate command is sent to the hardware if StartTrackerTargetVibration()
    /// is called.  If the tracker target is already vibrating, call StopTrackerTargetVibration() 
    /// to stop the current vibration.
    /// </remarks>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="isVibrationEnabled">True to enable vibration. False if not.</param>
    public void SetTrackerTargetVibrationEnabled(TrackerTargetType trackerTargetType, bool isVibrationEnabled)
    {
        zsupSetTargetVibrationEnabled((int)trackerTargetType, isVibrationEnabled);
    }

    /// <summary>
    /// Check whether the tracker target's vibration is enabled.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if vibration is enabled. False if not.</returns>
    public bool IsTrackerTargetVibrationEnabled(TrackerTargetType trackerTargetType)
    {
        bool isVibrationEnabled = false;
        zsupIsTargetVibrationEnabled((int)trackerTargetType, out isVibrationEnabled);
        return isVibrationEnabled;
    }

    /// <summary>
    /// Check whether the tracker target is currently vibrating.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if vibrating. False if not.</returns>
    public bool IsTrackerTargetVibrating(TrackerTargetType trackerTargetType)
    {
        bool isVibrating = false;
        zsupIsTargetVibrating((int)trackerTargetType, out isVibrating);
        return isVibrating;
    }

    /// <summary>
    /// Start vibrating the tracker target based on a specified on period, off period,
    /// and number of times.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <param name="onPeriod">
    /// The duration in seconds of the vibration.
    /// </param>
    /// <param name="offPeriod">
    /// The duration in seconds between vibrations.
    /// </param>
    /// <param name="numTimes">
    /// The number of times the vibration occurs:<br>
    /// -1 -> Vibrate infinitely<br>
    ///  0 -> Do nothing<br>
    ///  N -> Vibrate N times
    /// </param>
    public void StartTrackerTargetVibration(TrackerTargetType trackerTargetType, float onPeriod, float offPeriod, int numTimes)
    {
        zsupStartTargetVibration((int)trackerTargetType, onPeriod, offPeriod, numTimes);
    }

    /// <summary>
    /// Stop vibrating the tracker target if it is currently vibrating.  
    /// </summary>
    /// <remarks>If StartTrackerTargetVibration() is
    /// called again, the tracker target will start vibrating the full sequence of "on" and "off" cycles.
    /// </remarks>
    public void StopTrackerTargetVibration(TrackerTargetType trackerTargetType)
    {
        zsupStopTargetVibration((int)trackerTargetType);
    }

/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup TargetTap Target Tap 
    /// Use this function to detect when the tracker target touches the physical screen.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Check whether the tracker target is tapping the display.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    /// <returns>True if the tracker target is currently tapping the display. False if not.</returns>
    public bool IsTrackerTargetTapPressed(TrackerTargetType trackerTargetType)
    {
        bool isTapPressed = false;
        zsupIsTargetTapPressed((int)trackerTargetType, out isTapPressed);
        return isTapPressed;
    }
/// @}


    //////////////////////////////////////////////////////////////////////////
    /// @defgroup MouseEmulation Mouse Emulation 
    /// Use these functions to enable any tracker target to emulate a mouse.
    /// @{
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set whether mouse emulation is enabled.
    /// </summary>
    /// <param name="isMouseEmulationEnabled">True to enable mouse emulation. False if not.</param>
    public void SetMouseEmulationEnabled(bool isMouseEmulationEnabled)
    {
        this.EnableMouseEmulation = isMouseEmulationEnabled;
        zsupSetMouseEmulationEnabled(isMouseEmulationEnabled);
    }

    /// <summary>
    /// Check whether mouse emulation is enabled.
    /// </summary>
    /// <returns>True if mouse emulation is enabled.  False if not.</returns>
    public bool IsMouseEmulationEnabled()
    {
        bool isEnabled = false;
        zsupIsMouseEmulationEnabled(out isEnabled);
        return isEnabled;
    }

    /// <summary>
    /// Specify the tracker target that will emulate the mouse.
    /// </summary>
    /// <param name="trackerTargetType">The type of tracker target.</param>
    public void SetMouseEmulationTarget(TrackerTargetType trackerTargetType)
    {
        zsupSetMouseEmulationTarget((int)trackerTargetType);
    }

    /// <summary>
    /// Set the movement mode for mouse emulation.
    /// </summary>
    /// <param name="movementMode">Movement is either absolute or relative to the mouse's current position.</param>
    public void SetMouseEmulationMovementMode(MouseMovementMode movementMode)
    {
        zsupSetMouseEmulationMovementMode((int)movementMode);
    }

    /// <summary>
    /// Get the movement mode of mouse emulation. Refer to #MouseMovementMode for details.
    /// </summary>
    /// <returns>The current movement mode.</returns>
    public MouseMovementMode GetMouseEmulationMovementMode()
    {
        int movementMode = 0;
        zsupGetMouseEmulationMovementMode(out movementMode);
        return (MouseMovementMode)movementMode;
    }

    /// <summary>
    /// Set the distance at which mouse emulation will be enabled.
    /// </summary>
    /// <param name="mouseEmulationDistance">The mouse emulation distance.</param>
    public void SetMouseEmulationDistance(float mouseEmulationDistance)
    {
        zsupSetMouseEmulationMaxDistance(mouseEmulationDistance);
    }

    /// <summary>
    /// Get the distance at which mouse emulation will be enabled.
    /// </summary>
    /// <returns>The mouse emulation distance.</returns>
    public float GetMouseEmulationDistance()
    {
        float maxDistance = 0.0f;
        zsupGetMouseEmulationMaxDistance(out maxDistance);
        return maxDistance;
    }

    /// <summary>
    /// Map a specified tracker target button to a mouse button.
    /// </summary>
    /// <param name="buttonId">Tracker target button ID.</param>
    /// <param name="mouseButton">Mouse button.</param>
    public void SetMouseEmulationButtonMapping(int buttonId, MouseButton mouseButton)
    {
        zsupSetMouseEmulationButtonMapping(buttonId, (int)mouseButton);
    }

    /// <summary>
    /// Get the mouse button that the specified button ID is mapped to.
    /// </summary>
    /// <param name="buttonId">Tracker target button ID.</param>
    /// <returns>Mouse button.</returns>
    public MouseButton GetMouseEmulationButtonMapping(int buttonId)
    {
        int mouseButton = 0;
        zsupGetMouseEmulationButtonMapping(buttonId, out mouseButton);
        return (MouseButton)mouseButton;
    }
/// @}

    /// @ingroup General
    /// <summary>
    /// Get a camera from the %ZSCore stereo rig based on a
    /// specified camera type.
    /// </summary>
    /// <param name="cameraType">The camera type: Left, Right, or Final.</param>
    /// <returns>Reference to the underlying Unity camera.</returns>
    public Camera GetStereoCamera(CameraType cameraType)
    {
        return _stereoCameras[(int)cameraType];
    }

    /// @ingroup General
    /// <summary>
    /// Convert a matrix in right-handed space to left-handed space.
    /// Sets whether automatic stereo is enabled.
    /// </summary>
    public void SetAutoStereoEnabled(bool enable)
    {
        EnableAutoStereo = enable;
    }

    /// <summary>
    /// Checks whether automatic stereo is enabled.
    /// </summary>
    /// <returns>True if automatic stereo is enabled. False if not.</returns>
    public bool IsAutoStereoEnabled()
    {
        return EnableAutoStereo;
    }

    /// <summary>
    /// Convert a matrix in right handed space to left handed space.
    /// </summary>
    /// <param name="right">A right-handed matrix.</param>
    /// <returns>A left-handed matrix.</returns>
    public static Matrix4x4 ConvertFromRightToLeft(Matrix4x4 right)
    {
        return RIGHT_TO_LEFT * right * RIGHT_TO_LEFT;
    }

    #endregion


    #region EVENTS

    /// @ingroup General
    /// A function for handling events raised by a %ZSCore object.
    public delegate void CoreEventHandler(ZSCore sender);

    /// @ingroup General 
    /// Raised after this instance's tracking data and stereo frustum has been refreshed.
    /// Called immediately before the beginning of a Unity frame.
    public event CoreEventHandler Updated;

    protected void RaiseUpdated()
    {
        if (Updated != null)
            Updated(this);
    }

    #endregion


    #region PRIVATE HELPERS

    /// <summary>
    /// Check whether the CurrentCamera is valid.
    /// </summary>
    private bool IsCurrentCameraValid()
    {
        return (this.CurrentCamera != null && this.CurrentCamera.GetComponent<Camera>() != null);
    }


    private void Initialize()
    {
        // Initialize the cached stereo information.
        for (int i = 0; i < (int)Eye.NumEyes; ++i)
        {
            _viewMatrices[i]        = Matrix4x4.identity;
            _projectionMatrices[i]  = Matrix4x4.identity;
        }

        // Initialize the cached tracker information.
        for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
        {
            _isTrackerTargetVisible[i] = false;
            _trackerTargetPoses[i]       = Matrix4x4.identity;
            _trackerTargetCameraPoses[i] = Matrix4x4.identity;
            _trackerTargetWorldPoses[i]  = Matrix4x4.identity;
        }

        // Initialize the mouse information.
        // Note: Initialize _wasMouseAutoHideEnabled to true to make sure
        //       the mouse cursor state is properly initialized if the
        //       EnableMouseAutoHide field is initialized to false.
        _wasMouseAutoHideEnabled    = true;
        _mouseAutoHideTimeRemaining = this.MouseAutoHideTimeout;

        // Initialize auto stereo information.
        _autoStereoIdleTimeRemaining = _autoStereoToMonoTimeout;
        _autoStereoCurrentHeadPose   = this.CalculateDefaultHeadPose(this.GetDisplayAngle());
    }

    /// <summary>
    /// Initialize the left and right stereo cameras.
    /// </summary>
    private void InitializeStereoCameras()
    {
        Transform stereoRig = this.transform.Find("ZSStereoRig");
        if (stereoRig == null)
        {
            Debug.LogError("InitializeStereoCameras Error: Could not find ZSStereoRig.");
            return;
        }

        Transform leftCamera = stereoRig.Find("ZSLeftCamera");
        if (leftCamera == null)
        {
            Debug.LogError("InitializeStereoCameras Error: Could not find ZSLeftCamera.");
            return;
        }
        ZSLeftCamera zsLeftCamera = leftCamera.GetComponent<ZSLeftCamera>();
        if (zsLeftCamera == null)
        {
            Debug.LogError("InitializeStereoCameras Error: Could not find ZSLeftCamera Monobehaviour script.");
            return;
        }
        zsLeftCamera.PreCull += this.PreCullUpdate;

        Transform rightCamera = stereoRig.Find("ZSRightCamera");
        if (rightCamera == null)
        {
            Debug.LogError("InitializeStereoCameras Error: Could not find ZSRightCamera.");
            return;
        }

        Transform finalCamera = stereoRig.Find("ZSFinalCamera");
        if (finalCamera == null)
        {
            Debug.LogError("InitializeStereoCameras Error: Could not find ZSFinalCamera.");
            return;
        }

        _stereoCameras[(int)CameraType.Left]  = leftCamera.GetComponent<Camera>();
        _stereoCameras[(int)CameraType.Right] = rightCamera.GetComponent<Camera>();
        _stereoCameras[(int)CameraType.Final] = finalCamera.GetComponent<Camera>();

        _stereoCameras[(int)CameraType.Left].enabled  = false;
        _stereoCameras[(int)CameraType.Right].enabled = false;
        _stereoCameras[(int)CameraType.Final].enabled = false;

        this.CheckCurrentCameraChanged();
    }

    /// <summary>
    /// Copy a certain subset of camera attributes from a 
    /// source camera to a destination camera.
    /// </summary>
    private void CopyCameraAttributes(Camera source, ref Camera destination)
    {
        if (source != null && destination != null)
        {
            destination.clearFlags      = source.clearFlags;
            destination.backgroundColor = source.backgroundColor;
            destination.cullingMask     = source.cullingMask;
        }
    }

    /// <summary>
    /// Check to see if the current camera has changed.
    /// </summary>
    private void CheckCurrentCameraChanged()
    {
        if (_previousCamera != this.CurrentCamera)
        {
            float currentCameraDepth = 0.0f;

            if (this.IsCurrentCameraValid())
            {
                Camera currentCamera = this.CurrentCamera.GetComponent<Camera>();

                // Grab the current camera depth.
                currentCameraDepth = currentCamera.depth;
        
                // Set the near/far clip planes.
                this.SetNearClip(currentCamera.nearClipPlane);
                this.SetFarClip(currentCamera.farClipPlane);

                // Copy a subset of camera attributes from the
                // CurrentCamera to the Left/Right cameras.
                this.CopyCameraAttributes(currentCamera, ref _stereoCameras[(int)CameraType.Left]);
                this.CopyCameraAttributes(currentCamera, ref _stereoCameras[(int)CameraType.Right]);
            }

            // Set the Left, Right, and Final Camera depth values.
            if (_stereoCameras[(int)CameraType.Left] != null)
                _stereoCameras[(int)CameraType.Left].depth = currentCameraDepth + 1.0f;

            if (_stereoCameras[(int)CameraType.Right] != null)
                _stereoCameras[(int)CameraType.Right].depth = currentCameraDepth + 2.0f;

            if (_stereoCameras[(int)CameraType.Final] != null)
                _stereoCameras[(int)CameraType.Final].depth = currentCameraDepth + 3.0f;

            _previousCamera = this.CurrentCamera;
        }
    }

    /// <summary>
    /// Check for any updates to public properties.
    /// </summary>
    private void CheckForUpdates()
    {
        if (this.EnableStereo != this.IsStereoEnabled())
            this.SetStereoEnabled(this.EnableStereo);

        if (this.EnableTracking != this.IsTrackingEnabled())
            this.SetTrackingEnabled(this.EnableTracking);

        if (this.EnableMouseEmulation != this.IsMouseEmulationEnabled())
            this.SetMouseEmulationEnabled(this.EnableMouseEmulation);

        if (this.InterPupillaryDistance != this.GetInterPupillaryDistance())
            this.SetInterPupillaryDistance(this.InterPupillaryDistance);

        if (this.ViewerScale != this.GetViewerScale())
            this.SetViewerScale(this.ViewerScale);

        if (this.FieldOfViewScale != this.GetFieldOfViewScale())
            this.SetFieldOfViewScale(this.FieldOfViewScale);

        if (this.HeadTrackingScale != this.GetHeadTrackingScale())
            this.SetHeadTrackingScale(this.HeadTrackingScale);

        if (this.GlassesOffset != this.GetGlassesOffset())
            this.SetGlassesOffset(this.GlassesOffset);

        if (this.EnableAutoStereo != this.IsAutoStereoEnabled())
            this.SetAutoStereoEnabled(this.EnableAutoStereo);
    }

    /// <summary>
    /// Update all of the stereo and tracker information.
    /// </summary>
    private void UpdateInternal()
    {
        if (!_forceUpdate)
        {
            return;
        }

        _forceUpdate = false;

        if (_coreSingleton.IsInitialized)
        {
            this.CheckCurrentCameraChanged();
            this.CheckForUpdates();

            // Perform an update on the TrackerTargets and StereoFrustum.
            zsupUpdate();

            this.UpdateStereoInternal();
            this.UpdateTrackerInternal();
            this.UpdateMouseInternal();
            this.UpdateAutoStereoInternal();

            // Raise the Updated event.
            this.RaiseUpdated();

            // Cache previous state.
            _wasFullScreen = Screen.fullScreen;
        }
    }

    /// <summary>
    /// Update L/R Detect.
    /// </summary>
    private void UpdateLRDetectInternal()
    {
        // Update L/R Detect.
        if (Screen.fullScreen || _isAlwaysWindowed)
        {
            GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.UpdateLRDetectFullscreen);
        }
        else
        {
            GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.UpdateLRDetectWindowed);
        }

        // For ATI cards, transitioning from windowed mode to fullscreen mode
        // can sometimes cause the left/right frames to become out of sync.
        // Force a sync for this case.
        if (SystemInfo.graphicsDeviceVendorID == 4098)
        {
            if (_wasFullScreen == false && Screen.fullScreen == true)
                GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.SyncLRDetectFullscreen);
        }
    }

    /// <summary>
    /// Update all of the stereo information.
    /// </summary>
    private void UpdateStereoInternal()
    {
        // Update the window dimensions if they have changed.
        zsupSetViewportSize(Screen.width, Screen.height);

        // Get the view and projection matrices.
        for (int i = 0; i < (int)Eye.NumEyes; ++i)
        {
            zsupGetFrustumViewMatrix(i, _matrixData);
            _viewMatrices[i] = this.ConvertToMatrix4x4(_matrixData);

            zsupGetFrustumProjectionMatrix(i, _matrixData);
            _projectionMatrices[i] = this.ConvertToMatrix4x4(_matrixData);
        }
    }

    /// <summary>
    /// Update all of the tracker information.
    /// </summary>
    private void UpdateTrackerInternal()
    {
        double timestamp = 0.0;

        // Get the tracker, camera, and world space target poses.
        for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
        {
            // Get whether or not pose is valid. 
            zsupIsTargetVisible(i, out _isTrackerTargetVisible[i]);
            
            if (_isTrackerTargetVisible[i])
            {
                // Tracker space poses.
                zsupGetTargetPose(i, _matrixData, out timestamp);
                _trackerTargetPoses[i] = this.ConvertToMatrix4x4(_matrixData);

                // Camera space poses.
                zsupGetTargetCameraPose(i, _matrixData, out timestamp);
                _trackerTargetCameraPoses[i] = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(_matrixData));

                // World space poses.
                _trackerTargetWorldPoses[i] = _trackerTargetCameraPoses[i];

                // Scale the position based on world and field of view scales.
                _trackerTargetWorldPoses[i][0, 3] *= this.FieldOfViewScale;
                _trackerTargetWorldPoses[i][1, 3] *= this.FieldOfViewScale;
              
                // Convert the camera space pose to world space.
                if (this.IsCurrentCameraValid())
                    _trackerTargetWorldPoses[i] = this.CurrentCamera.transform.localToWorldMatrix * _trackerTargetWorldPoses[i];
            }
        }
    }

    /// <summary>
    /// Update all of the mouse information.
    /// </summary>
    private void UpdateMouseInternal()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        if (this.EnableMouseAutoHide == true)
        {
            if (currentMousePosition != _previousMousePosition ||
                Input.GetMouseButton(0) || 
                Input.GetMouseButton(1) || 
                Input.GetMouseButton(2))
            {
                // If the cursor was previously disabled and the mouse
                // moved or button was pressed, show the cursor and reset 
                // the remaining time before the mouse auto-hides.
                Cursor.visible = true;
                _mouseAutoHideTimeRemaining = this.MouseAutoHideTimeout;
            }
            else if (_mouseAutoHideTimeRemaining > 0.0f)
            {   
                // Update the remaining time before the mouse auto-hides.
                // If the remaining time falls below zero, we know that the
                // mouse has not been moved.
                _mouseAutoHideTimeRemaining -= Time.unscaledDeltaTime;
                if (_mouseAutoHideTimeRemaining < 0.0f)
                {
                    Cursor.visible = false;
                }
            }
        }
        else if (this.EnableMouseAutoHide == false && _wasMouseAutoHideEnabled == true)
        {
            // Restore the mouse cursor's visibility to its default state.
            Cursor.visible = _coreSingleton.DefaultMouseCursorState;
        }

        _wasMouseAutoHideEnabled = this.EnableMouseAutoHide;
        _previousMousePosition   = currentMousePosition;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateAutoStereoInternal()
    {
        // If the user doesn't want automatic stereo or tracking is not enabled, 
        // use defaults.
        if (this.EnableAutoStereo == false || this.EnableTracking == false)
        {
            _coreSingleton.AutoStereoState = AutoStereoState.IdleStereo;
            zsupOverrideFrustumHeadPose(false);
            zsupSetFrustumAttribute((int)FrustumAttribute.Ipd, this.InterPupillaryDistance);
            return;
        }

        // Update the current head pose.
        if (this.IsTrackerTargetVisible(TrackerTargetType.Head))
        {
            _autoStereoCurrentHeadPose = this.GetTrackerTargetPose(TrackerTargetType.Head);
        }

        // Update the target head pose.
        Vector3 displayAngle = this.GetDisplayAngle();
        if (_autoStereoDisplayAngle != displayAngle)
        {
            _autoStereoTargetHeadPose = this.CalculateDefaultHeadPose(displayAngle);
            _autoStereoDisplayAngle = displayAngle;
        }

        // Get the current IPD.
        float currentIpd = 0.0f;
        zsupGetFrustumAttribute((int)FrustumAttribute.Ipd, out currentIpd);

        // Determine if any target is visible.
        bool isAnyTargetVisible = this.IsTrackerTargetVisible(TrackerTargetType.Head)    ||
                                  this.IsTrackerTargetVisible(TrackerTargetType.Primary) ||
                                  this.IsTrackerTargetVisible(TrackerTargetType.Secondary);

        switch (_coreSingleton.AutoStereoState)
        {
            case AutoStereoState.IdleMono:
                {
                    zsupSetFrustumHeadPose(this.ConvertToFloatArray(_autoStereoTargetHeadPose));

                    // If the head target is visible, immediately begin the animation to stereo.
                    if (this.IsTrackerTargetVisible(TrackerTargetType.Head))
                    {
                        _coreSingleton.AutoStereoState = AutoStereoState.AnimateToStereo;
                        _autoStereoAnimationTimeRemaining = _autoStereoAnimationDuration;
                    }
                }
                break;

            case AutoStereoState.IdleStereo:
                {
                    // If all targets are not visible, update the idle time remaining.
                    if (!isAnyTargetVisible)
                    {
                        _autoStereoIdleTimeRemaining -= Time.unscaledDeltaTime;

                        // If the idle time remaining has reached 0.0f, begin the animation
                        // to mono.
                        if (_autoStereoIdleTimeRemaining <= 0.0f)
                        {
                            _coreSingleton.AutoStereoState = AutoStereoState.AnimateToMono;
                            _autoStereoAnimationTimeRemaining = _autoStereoAnimationDuration;
                        }
                    }
                    else
                    {
                        _autoStereoIdleTimeRemaining = _autoStereoToMonoTimeout;
                    }
                }
                break;

            case AutoStereoState.AnimateToMono:
                {
                    // Calculate the normalized time.
                    float time = Mathf.Clamp01(1.0f - (_autoStereoAnimationTimeRemaining / _autoStereoAnimationDuration));

                    // Animate the IPD.
                    float ipd = Mathf.Lerp(this.GetInterPupillaryDistance(), 0.0f, time);
                    zsupSetFrustumAttribute((int)FrustumAttribute.Ipd, ipd);

                    // Animate the head pose.
                    Matrix4x4 headPose = this.MatrixLerp(_autoStereoCurrentHeadPose, _autoStereoTargetHeadPose, time);
                    zsupSetFrustumHeadPose(this.ConvertToFloatArray(headPose));
                    zsupOverrideFrustumHeadPose(true);

                    // Update the remaining time for the animation.
                    _autoStereoAnimationTimeRemaining -= Time.unscaledDeltaTime;

                    // If the animation is finished, transition to the idle mono state.
                    if (_autoStereoAnimationTimeRemaining < 0.0f && currentIpd == 0.0f)
                    {
                        headPose = this.MatrixLerp(_autoStereoCurrentHeadPose, _autoStereoTargetHeadPose, 1.0f);
                        zsupSetFrustumHeadPose(this.ConvertToFloatArray(headPose));

                        _coreSingleton.AutoStereoState = AutoStereoState.IdleMono;
                    }
                }
                break;

            case AutoStereoState.AnimateToStereo:
                {
                    // Calculate the normalized time.
                    float time = Mathf.Clamp01(1.0f - (_autoStereoAnimationTimeRemaining / _autoStereoAnimationDuration));

                    // Animate the IPD.
                    float ipd = Mathf.Lerp(0.0f, this.GetInterPupillaryDistance(), time);
                    zsupSetFrustumAttribute((int)FrustumAttribute.Ipd, ipd);

                    // Animate the head pose.
                    Matrix4x4 headPose = this.MatrixLerp(_autoStereoTargetHeadPose, _autoStereoCurrentHeadPose, time);
                    zsupSetFrustumHeadPose(this.ConvertToFloatArray(headPose));
                    zsupOverrideFrustumHeadPose(true);

                    // Update the remaining time for the animation.
                    _autoStereoAnimationTimeRemaining -= Time.unscaledDeltaTime;

                    // If the animation is finished, transition to the idle stereo state.
                    if (_autoStereoAnimationTimeRemaining < 0.0f && currentIpd == this.InterPupillaryDistance)
                    {
                        _coreSingleton.AutoStereoState = AutoStereoState.IdleStereo;
                        _autoStereoIdleTimeRemaining   = _autoStereoToMonoTimeout;
                        zsupOverrideFrustumHeadPose(false);
                    }
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Calculate the default head pose based on the current display rotation.
    /// </summary>
    /// <returns></returns>
    private Matrix4x4 CalculateDefaultHeadPose(Vector3 displayAngle)
    {
        Vector3 offset = new Vector3(0.0f, 0.0f, this.GetCameraOffset().magnitude);
        Matrix4x4 displayRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(displayAngle.x - 90.0f, 0.0f, 0.0f), Vector3.one);
        Matrix4x4 defaultHeadPose = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        defaultHeadPose = displayRotation * defaultHeadPose;
        
        return defaultHeadPose;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
    {
        // Normalize the time.
        time = Mathf.Clamp(time, 0.0f, 1.0f);

        // Interpolate the position
        Vector3 newPosition = Vector3.Lerp(from.GetColumn(3), to.GetColumn(3), time);

        // Interpolate the rotation
        Quaternion fromRotation = Quaternion.LookRotation(from.GetColumn(2), from.GetColumn(1));
        Quaternion toRotation   = Quaternion.LookRotation(to.GetColumn(2), to.GetColumn(1));
        Quaternion newRotation  = Quaternion.Lerp(fromRotation, toRotation, time);

        // Create the interpolated matrix
        Matrix4x4 newMatrix = Matrix4x4.identity;
        newMatrix.SetTRS(newPosition, newRotation, Vector3.one);

        return newMatrix;
    }

    /// <summary>
    /// Convert an array of 16 floats to Unity's Matrix4x4 format.
    /// </summary>
    /// <param name="matrixData">The matrix data stored in a float array.</param>
    /// <returns>The matrix data in Matrix4x4 format.</returns>
    private Matrix4x4 ConvertToMatrix4x4(float[/*16*/] matrixData)
    {
        Matrix4x4 matrix = new Matrix4x4();

        for (int i = 0; i < 16; i++)
            matrix[i] = matrixData[i];

        return matrix;
    }

    /// <summary>
    /// Convert an array of 2 floats to Unity's Vector2 format.
    /// </summary>
    /// <param name="vectorData">The vector data stored in a float array.</param>
    /// <returns>The vector data in Vector2 format.</returns>
    private Vector2 ConvertToVector2(float[/*2*/] vectorData)
    {
        return new Vector2(vectorData[0], vectorData[1]);
    }

    /// <summary>
    /// Convert an array of 3 floats to Unity's Vector3 format.
    /// </summary>
    /// <param name="vectorData">The vector data stored in a float array.</param>
    /// <returns>The vector data in Vector3 format.</returns>
    private Vector3 ConvertToVector3(float[/*3*/] vectorData)
    {
        return new Vector3(vectorData[0], vectorData[1], vectorData[2]);
    }

    /// <summary>
    /// Convert a Vector3 to a float array.
    /// </summary>
    /// <param name="vector3">Vector3 data in Unity's Vector3 format.</param>
    /// <returns>Vector data in float array format.</returns>
    private float[] ConvertToFloatArray(Vector3 vector3)
    {
        float[] array = new float[3];
        array[0] = vector3.x;
        array[1] = vector3.y;
        array[2] = vector3.z;

        return array;
    }

    /// <summary>
    /// Convert a Matrix4x4 to a float array.
    /// </summary>
    /// <param name="matrix">Matrix data in Unity's Matrix4x4 format.</param>
    /// <returns>Matrix data in float array format.</returns>
    private float[] ConvertToFloatArray(Matrix4x4 matrix)
    {
        float[] array = new float[16];

        for (int i = 0; i < 16; i++)
            array[i] = matrix[i];

        return array;
    }

    /// <summary>
    /// The update coroutine.
    /// This will continue after the end of the frame has been hit.
    /// </summary>
    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            _forceUpdate = true;

            this.UpdateLRDetectInternal();

            // Set the final camera to be enabled so that it can reset the draw buffer
            // to the back buffer for the next frame.
            if (this.IsStereoEnabled() && _stereoCameras[(int)CameraType.Final] != null)
                _stereoCameras[(int)CameraType.Final].enabled = true;

            // Set the current camera to be enabled so that Camera.main does not return null
            // when referenced in Awake, Start, Update, etc. 
            if (this.IsStereoEnabled() && this.IsCurrentCameraValid())
                this.CurrentCamera.GetComponent<Camera>().enabled = true;

            // Wait for the end of the frame.
            yield return new WaitForEndOfFrame();
        }
    }

    private void PreCullUpdate(ZSLeftCamera sender)
    {
        if (_coreSingleton.AutoStereoState == AutoStereoState.IdleStereo)
        {
            zsupUpdate();

            this.UpdateStereoInternal();
        }
    }

    #endregion


    #region PRIVATE MEMBERS

    // Constants
    private readonly static Matrix4x4 RIGHT_TO_LEFT = Matrix4x4.Scale(new Vector4(1.0f, 1.0f, -1.0f));

    private readonly static int[] BLACK     = { 0, 0, 0 };
    private readonly static int[] WHITE     = { 1, 1, 1 };
    private readonly static int[] RED       = { 1, 0, 0 };
    private readonly static int[] GREEN     = { 0, 1, 0 };
    private readonly static int[] BLUE      = { 0, 0, 1 };
    private readonly static int[] CYAN      = { 0, 1, 1 };
    private readonly static int[] MAGENTA   = { 1, 0, 1 };
    private readonly static int[] YELLOW    = { 1, 1, 0 };


    // Non-Constants
    private ZSCoreSingleton _coreSingleton    = null;

    private bool            _forceUpdate      = false;

    private bool            _isStereoEnabled  = false;
    private bool            _areEyesSwapped   = false;
    private bool            _wasFullScreen    = false;
    private bool            _isAlwaysWindowed = false;

    private float[]         _matrixData         = new float[16];
    private Matrix4x4[]     _viewMatrices       = new Matrix4x4[(int)Eye.NumEyes];
    private Matrix4x4[]     _projectionMatrices = new Matrix4x4[(int)Eye.NumEyes];

    private bool[]          _isTrackerTargetVisible   = new bool[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetPoses       = new Matrix4x4[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetCameraPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetWorldPoses  = new Matrix4x4[(int)TrackerTargetType.NumTypes];

    private bool            _wasMouseAutoHideEnabled    = false;
    private Vector3         _previousMousePosition      = new Vector3();
    private float           _mouseAutoHideTimeRemaining = 0.0f;

    private GameObject      _previousCamera  = null;
    private Camera[]        _stereoCameras   = new Camera[(int)CameraType.NumTypes];
    
    private List<int[]>     _ledColors = new List<int[]>() { BLACK, WHITE, RED, GREEN, BLUE, CYAN, MAGENTA, YELLOW };

    private Matrix4x4       _autoStereoCurrentHeadPose;
    private Matrix4x4       _autoStereoTargetHeadPose;
    private Vector3         _autoStereoDisplayAngle;
    private float           _autoStereoToMonoTimeout          = 5.0f;
    private float           _autoStereoAnimationDuration      = 1.0f;
    private float           _autoStereoIdleTimeRemaining      = 1.0f;
    private float           _autoStereoAnimationTimeRemaining = 0.0f;

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
    private static extern int  zsupUpdate();
    [DllImport("zSpaceUnity")]
    private static extern int  zsupShutdown();
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTrackingEnabled(bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTrackingEnabled(out bool isEnabled);

    // Display API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetDisplayPosition(out int x, out int y);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetDisplaySize(out float width, out float height);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetDisplayResolution(out int x, out int y);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetDisplayAngle(out float x, out float y, out float z);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsDisplayHardwarePresent(out bool isHardwarePresent);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIntersectDisplay(float[/*16*/] poseMatrix, out bool hit, out int x, out int y, out int nx, out int ny, out float distance);

    // StereoViewport API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetViewportPosition(int x, int y);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetViewportPosition(out int x, out int y);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetViewportSize(int width, int height);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetViewportSize(out int width, out int height);

    // Coordinate Space API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetCoordinateSpaceTransform(int a, int b, float[/*16*/] transform);

    // StereoFrustum API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetFrustumAttribute(int attribute, float value);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumAttribute(int attribute, out float value);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetFrustumPortalMode(int portalModeFlags);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumPortalMode(out int portalModeFlags);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetFrustumCameraOffset(float[/*3*/] cameraOffset);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumCameraOffset(float[/*3*/] cameraOffset);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetFrustumHeadPose(float[/*16*/] headPose);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumHeadPose(float[/*16*/] headPose);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupOverrideFrustumHeadPose(bool overrideHeadPose);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumViewMatrix(int eye, float[/*16*/] viewMatrix);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumProjectionMatrix(int eye, float[/*16*/] projectionMatrix);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumEyePosition(int eye, float[/*3*/] eyePosition);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetFrustumBounds(int eye, float[/*6*/] frustumBounds);

    // TrackerTarget API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetEnabled(int targetType, bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetEnabled(int targetType, out bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetVisible(int targetType, out bool isVisible);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetPose(int targetType, float[/*16*/] poseMatrix, out double timestamp);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetCameraPose(int targetType, float[/*16*/] poseMatrix, out double timestamp);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetPoseBufferingEnabled(int targetType, bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetPoseBufferingEnabled(int targetType, out bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetBufferedPose(int targetType, float seconds, float[/*16*/] poseMatrix, out double timestamp);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetBufferedCameraPose(int targetType, float seconds, float[/*16*/] poseMatrix, out double timestamp);

    // TrackerTarget Button API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetNumTargetButtons(int targetType, out int numButtons);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetButtonPressed(int targetType, int buttonId, out bool isPressed);

    // TrackerTarget Led API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetLedEnabled(int targetType, bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetLedEnabled(int targetType, out bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetLedOn(int targetType, out bool isOn);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetLedColor(int targetType, float r, float g, float b);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetLedColor(int targetType, out float r, out float g, out float b);

    // TrackerTarget Vibration API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetVibrationEnabled(int targetType, bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetVibrationEnabled(int targetType, out bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetVibrating(int targetType, out bool isVibrating);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupStartTargetVibration(int targetType, float onPeriod, float offPeriod, int numTimes);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupStopTargetVibration(int targetType);
    
    // TrackerTarget Tap API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsTargetTapPressed(int targetType, out bool isPressed);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetTargetTapHoldThreshold(int targetType, float seconds);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetTargetTapHoldThreshold(int targetType, out float seconds);

    // Mouse Emulation API
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetMouseEmulationEnabled(bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupIsMouseEmulationEnabled(out bool isEnabled);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetMouseEmulationTarget(int targetType);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetMouseEmulationTarget(out int targetType);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetMouseEmulationMovementMode(int movementMode);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetMouseEmulationMovementMode(out int movementMode);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetMouseEmulationMaxDistance(float maxDistance);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetMouseEmulationMaxDistance(out float maxDistance);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupSetMouseEmulationButtonMapping(int buttonId, int mouseButton);
    [DllImport("zSpaceUnity")]
    private static extern int  zsupGetMouseEmulationButtonMapping(int buttonId, out int mouseButton);

    #endregion
}
