//#########[---------------------------]#########
//#########[  GENERATED FROM TEMPLATE  ]#########
//#########[---------------------------]#########
#define USE_4D
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleCameraController4D : BasicStaticCamera4D
{
    public const float MOVE_SPEED = 60.0f;
    public const float JUMP_SPEED = 6.0f;
    public const float PLAYER_RADIUS = 0.3f;
    public const float CAM_HEIGHT = 1.62f;
    public const float VOLUME_TIME = 0.75f;
    public const float ZOOM_RATE = 1.1f;
    public const float ZOOM_MAX = 8.0f;
    public const float ZOOM_MIN = 0.3f;
    public const float ZOOM_SMOOTH = 0.05f;
    public static int PROJECTION_MODE = 0;

    float cameraDistance = 2.5f;

    public CompassContainer compass;
    public float lookYZ = 0.0f;

    [System.NonSerialized] public Quaternion m0Quaternion = Quaternion.identity;
    [System.NonSerialized] public Quaternion m1 = Quaternion.identity;
    [System.NonSerialized] public bool locked = false;
    [System.NonSerialized] public bool lockViews = false; //Locks volume, shadow, and slice views
    [System.NonSerialized] public bool volumeMode = false;

    float orientationChangeTime = 1e8f;
    Vector4 oldOrientationVec = Vector3.up;
    Vector4 currentOrientationVec = Vector3.up;
    Vector4 newOrientationVec = Vector3.up;

    [System.NonSerialized] public Matrix4x4 nonVCameraMatrix;

    Vector4 gravityDirection = (Vector4)Vector3.up;
    protected Matrix4x4 gravityMatrix = Matrix4x4.identity;
    protected bool fastGravity = false;
    private float volumeInterp = 0.0f;
    protected float volumeSmooth = 0.0f;
    protected float volumeStartYZ = 0.0f;
    protected float smoothAngX = 0.0f;
    protected float smoothAngY = 0.0f;
    protected float zoom = 1.0f;
    protected float targetZoom = 1.0f;
    protected float volumeHeight = 0.05f;
    protected float runMultiplier = 2.0f;

    float yaw = 0f;
    float tilt = 0f;

    [System.NonSerialized] public bool sliceEnabled = true;
    [System.NonSerialized] public int shadowMode = 1;

    private Quaternion vrLastOrientation = Quaternion.identity;
    private Vector3 vrLastPosition = Vector3.zero;
    private Quaternion vrM1Unfiltered = Quaternion.identity;
    public static Quaternion vrCompassShift = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 0, 180);

    public GameObject targetMarble;
    Vector4 targetPos = Vector4.one;

    public bool isOOB = false;

    protected override void Start()
    {
        base.Start();
        InputManager.HideCursor(true);
    }

    public override void Reset()
    {
        base.Reset();
        lookYZ = 0.0f;
        m0Quaternion = Quaternion.identity;
        m1 = Quaternion.identity;
        locked = false;
        smoothAngX = 0.0f;
        smoothAngY = 0.0f;
        volumeMode = false;
        volumeInterp = 0.0f;
        volumeSmooth = 0.0f;
        volumeStartYZ = 0.0f;
        gravityMatrix = Matrix4x4.identity;
        fastGravity = false;
        zoom = 1.0f;
        targetZoom = 1.0f;
    }

    public virtual void StartGame() { }

    public float CamHeight()
    {
        //In VR, camera height is built-in, don't need to add anything extra.
        float headHeight = (UnityEngine.XR.XRSettings.enabled ? 0.0f : CAM_HEIGHT);
        return Mathf.Lerp(headHeight, volumeHeight, volumeSmooth);
    }

    public bool IsVolumeTransition()
    {
        return (volumeInterp != 0.0f && volumeInterp != 1.0f);
    }

    public void HandleLooking()
    {
        //Apply looking
        float mouseSmooth = Mathf.Pow(2.0f, -Time.deltaTime / InputManager.CAM_SMOOTHING);
        float angX = InputManager.GetAxis(InputManager.AxisBind.LookHorizontal);
        float angY = InputManager.GetAxis(InputManager.AxisBind.LookVertical);
        if (Time.deltaTime == 0.0f)
        {
            smoothAngX = 0.0f;
            smoothAngY = 0.0f;
        }
        else
        {
            smoothAngX = smoothAngX * mouseSmooth + angX * (1.0f - mouseSmooth);
            smoothAngY = smoothAngY * mouseSmooth + angY * (1.0f - mouseSmooth);
        }

        //Update rotations
        if (UnityEngine.XR.XRSettings.enabled)
        {
            if (VRInputManager.GetKeyDown(VRInputManager.VRButton.Rotate))
            {
                VRInputManager.HandOrientation(VRInputManager.Hand.Left, out vrLastOrientation);
                vrM1Unfiltered = m1;
                vrLastOrientation = vrCompassShift * vrLastOrientation;
            }
            if (VRInputManager.GetKey(VRInputManager.VRButton.Rotate))
            {
                if (volumeMode)
                {
                    //TODO: Handle volume mode
                }
                else
                {
                    if (VRInputManager.HandOrientation(VRInputManager.Hand.Left, out Quaternion newOrientation))
                    {
                        newOrientation = vrCompassShift * newOrientation;
#if USE_5D
                        //TODO: Handle 5D
#else
                        vrM1Unfiltered = vrM1Unfiltered * vrLastOrientation * Quaternion.Inverse(newOrientation);
                        m1 = Quaternion.Slerp(vrM1Unfiltered, m1, Mathf.Pow(2.0f, -8.0f * Time.deltaTime));
#endif
                        vrLastOrientation = newOrientation;
                    }
                    else
                    {
                        vrLastOrientation = Quaternion.identity;
                    }
                }
            }
        }
        else
        {
            if (InputManager.GetKey(InputManager.KeyBind.Look4D))
            {
                if (volumeMode)
                {
                    m1 = m1 * Quaternion.Euler(0.0f, smoothAngX, 0.0f);
                    tilt += smoothAngX;
                }
                else
                {
                    m1 = m1 * Quaternion.Euler(-smoothAngY, smoothAngX, 0.0f);
                }
#if USE_5D
            } else if (InputManager.GetKey(InputManager.KeyBind.Look5D)) {
                Quaternion q = Quaternion.Euler(-smoothAngX, -smoothAngY, 0.0f);
                m1 = m1 * new Isocline(q, q);
            } else if (InputManager.GetKey(InputManager.KeyBind.LookSpin)) {
                Quaternion q = Quaternion.Euler(0.0f, 0.0f, smoothAngX);
                m1 = m1 * new Isocline(q, q);
#endif
            }
            else
            {
                if (!isOOB)
                {
                    if (volumeMode)
                    {
                        m1 = m1 * Quaternion.Euler(-smoothAngY, 0.0f, -smoothAngX);
                        yaw += smoothAngX;
                    }
                    else
                    {

                        m1 = m1 * Quaternion.Euler(0.0f, 0.0f, -smoothAngX);
                        lookYZ += smoothAngY;
                        lookYZ = Mathf.Clamp(lookYZ, -89.0f, 89.0f);
                        yaw += smoothAngX;
                    }
                }
            }
        }

        //if (volumeMode)
        //{
        //    var forward = new Vector4(0, 0, 1, 0);
        //    var tform = Transform4D.PlaneRotation(tilt, 2, 3) * Transform4D.PlaneRotation(yaw, 0, 2);
        //    forward = tform * forward;

        //    camPosition4D = targetPos; // - smoothGravityDirection * Mathf.Sin(lookYZ * Mathf.PI / 180) * cameraDistance;
        //    camPosition4D -= forward * Mathf.Cos(lookYZ * Mathf.PI / 180) * cameraDistance;
        //}
        //else
        //{
        //    var forward = new Vector4(0, 0, 1, 0);
        //    var tform = Transform4D.PlaneRotation(yaw, 0, 2);
        //    forward = tform * forward;

        //    camPosition4D = targetPos - smoothGravityDirection * Mathf.Sin(lookYZ * Mathf.PI / 180) * cameraDistance;
        //    camPosition4D -= forward * Mathf.Cos(lookYZ * Mathf.PI / 180) * cameraDistance;
        //}

        //m1 = Quaternion.identity;

        //Vector4 dir = new Vector4(1, 0, 0, 0);

        //var mat = Transform4D.PlaneRotation(-lookYZ, 1, 2);
        //dir = mat * dir;
        //// mat = Transform4D.PlaneRotation(yaw, 0, 2);
        //// dir = mat * dir;
        //// var q = Quaternion.Euler(0, 0, lookYZ);
        //// dir = q * dir;
        //// q = Quaternion.Euler(0, yaw, 0);
        //// dir = q * dir;
        //dir *= cameraDistance;

        //camPosition4D = targetPos - dir;
        // m1 = Quaternion.LookRotation(dir, new Vector3(0, 1, 0));
    }

    public Vector4 HandleMoving()
    {
        //Get movement force and jump
        Vector4 accel = Vector4.zero;
        accel.x = InputManager.GetAxis(InputManager.AxisBind.MoveLeftRight);
        if (InputManager.GetKey(InputManager.KeyBind.Left))
        {
            accel.x = -1.0f;
        }
        if (InputManager.GetKey(InputManager.KeyBind.Right))
        {
            accel.x = 1.0f;
        }
        accel.z = InputManager.GetAxis(InputManager.AxisBind.MoveForwardBack);
        if (InputManager.GetKey(InputManager.KeyBind.Backward))
        {
            accel.z = -1.0f;
        }
        if (InputManager.GetKey(InputManager.KeyBind.Forward))
        {
            accel.z = 1.0f;
        }
        accel.w = InputManager.GetAxis(InputManager.AxisBind.MoveAnaKata);
        if (InputManager.GetKey(InputManager.KeyBind.Kata))
        {
            accel.w = -1.0f;
        }
        if (InputManager.GetKey(InputManager.KeyBind.Ana))
        {
            accel.w = 1.0f;
        }
        accel.y = volumeSmooth * accel.w;
        accel.w = (volumeSmooth - 1.0f) * accel.w;
#if USE_5D
        accel.v = InputManager.GetAxis(InputManager.AxisBind.MoveSursumDeorsum);
        if (InputManager.GetKey(InputManager.KeyBind.Sursum)) {
            accel.v = -1.0f;
        }
        if (InputManager.GetKey(InputManager.KeyBind.Deorsum)) {
            accel.v = 1.0f;
        }
#endif

        return accel;
    }

    Vector4 GetOrientationVec(float time)
    {
        if (time < orientationChangeTime)
            return oldOrientationVec;
        if (time > orientationChangeTime + 0.3f)
            return newOrientationVec;
        var completion = Mathf.Clamp01((time - orientationChangeTime) / 0.3f);
        float angle = Transform4D.Angle(oldOrientationVec, newOrientationVec);
        return Transform4D.RotateTowards(oldOrientationVec, newOrientationVec, angle * completion);
    }

    public void UpdateMB(TimeState t)
    {
        //Update gravity matrix
        gravityMatrix = Transform4D.OrthoIterate(Transform4D.FromToRotation(gravityMatrix.GetColumn(1), currentOrientationVec) * gravityMatrix);

        //Check if shadows should be enabled/disabled
        if (!lockViews && InputManager.GetKeyDown(InputManager.KeyBind.ShadowToggle))
        {
            shadowMode = (shadowMode + 1) % 3;
            UpdateCameraMask();
        }
        if (!lockViews && InputManager.GetKeyDown(InputManager.KeyBind.SliceToggle))
        {
            sliceEnabled = !sliceEnabled;
            UpdateCameraMask();
        }

        //Check for volume mode change
        if (!lockViews && InputManager.GetKeyDown(InputManager.KeyBind.VolumeView))
        {
            if (!volumeMode) { volumeStartYZ = lookYZ; }
            volumeMode = !volumeMode;
        }

        //Interpolate volume change
        volumeInterp = Mathf.Clamp01(volumeInterp + (volumeMode ? Time.deltaTime : -Time.deltaTime) / VOLUME_TIME);
        volumeSmooth = Mathf.SmoothStep(0.0f, 1.0f, volumeInterp);
        Shader.SetGlobalFloat(minCheckerID, volumeSmooth * 0.125f);
        if (volumeMode)
        {
            lookYZ = Mathf.Lerp(volumeStartYZ, 0.0f, volumeSmooth);
        }

        //Handle camera and player inputs or seek
        if (!locked)
        {
            HandleLooking();
        }

        //Update compasses
        if (compass)
        {
            compass.SetRotations(m0Quaternion, m1);
        }

        //Disable overriding up-down look in VR
        if (UnityEngine.XR.XRSettings.enabled)
        {
            lookYZ = 0.0f;
        }

        //Create the camera matrix
        camMatrix = CreateCamMatrix(m1, lookYZ);

        // Camera orbit - bruh
        if (!isOOB)
        {
            targetPos = targetMarble.GetComponent<Marble4D>().lastRenderedPosition;

            var forward = new Vector4(0, 0, 1, 0);
            var forwardCam = camMatrix * forward * cameraDistance;
            camPosition4D = targetPos - forwardCam;
        }
        else
        {
            var forward = targetPos - camPosition4D;
            camMatrix.SetColumn(2, forward);
            // Look at the marble
        }

        //Update the m0 quaternion
        m0Quaternion = Quaternion.Slerp(Quaternion.Euler(-lookYZ, 0.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 90.0f), volumeSmooth);

        currentOrientationVec = GetOrientationVec(t.currentAttemptTime);
    }

    protected virtual void Update()
    {

    }

    public void SetGravityDirection(Vector4 dir, TimeState t, bool instant = false)
    {
        if (dir == gravityDirection) return;
        oldOrientationVec = currentOrientationVec;
        newOrientationVec = dir;
        orientationChangeTime = instant ? -1e8f : t.currentAttemptTime;
        this.gravityDirection = dir;
    }

    protected virtual void UpdateZoom()
    {
        targetZoom *= Mathf.Pow(ZOOM_RATE, -InputManager.GetAxis(InputManager.AxisBind.Zoom));
        targetZoom = Mathf.Clamp(targetZoom, ZOOM_MIN, ZOOM_MAX);
        float zoomSmooth = Mathf.Pow(2.0f, -Time.deltaTime / ZOOM_SMOOTH);
        zoom = zoom * zoomSmooth + targetZoom * (1.0f - zoomSmooth);
    }

    public override Vector4 camPosition4D
    {
        get
        {
            Vector4 result = position4D;
            result += currentOrientationVec * CamHeight();
            return result;
        }
        set
        {
            Vector4 result = value;
            result -= currentOrientationVec * CamHeight();
            position4D = result;
        }
    }

    public Matrix4x4 CreateCamMatrix(Quaternion m1Rot, float yz)
    {
        //Up-Forward
        Matrix4x4 mainRot = Transform4D.Slerp(Transform4D.PlaneRotation(yz, 1, 2), Transform4D.PlaneRotation(90.0f, 1, 3), volumeSmooth);

        nonVCameraMatrix = gravityMatrix * Transform4D.SkipY(m1Rot);

        //Combine with secondary rotation
        return gravityMatrix * Transform4D.SkipY(m1Rot) * mainRot;
    }

    public void UpdateCameraMask()
    {
        //NOTE: Using the static variable from CameraControl4D intentionally so it affects both
        if (!sliceEnabled && shadowMode == 0) { shadowMode = 1; }
        if (CameraControl4D.PROJECTION_MODE != 0 && shadowMode == 1) { shadowMode = 2; }
        UpdateCameraMask(shadowMode, sliceEnabled);
    }
}
