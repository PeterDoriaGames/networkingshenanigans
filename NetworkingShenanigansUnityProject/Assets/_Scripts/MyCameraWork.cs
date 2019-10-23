using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraWork : MonoBehaviour
{
    #region PRIVATE FIELDS

    [Tooltip("The distance in the local x-z plane to the target")]
    [SerializeField]
    private float _Distance = 7.0f;

    [Tooltip("The height of we want the camera to be above the target")]
    [SerializeField]
    private float _Height = 3.0f;

    [Tooltip("The Smooth time lag for the height of the camera")]
    [SerializeField]
    private float _HeightSmoothLag = 0.3f;

    [Tooltip("Allow the camera to be offsetted vertically from the target, for example, giving more view of the sceneray and less ground")]
    [SerializeField]
    private Vector3 _CenterOffset = Vector3.zero;

    [Tooltip("Set this as false if a component of a prefab being instantiated by Photon Network, and manually call OnStartFollowing() when and if needed")]
    [SerializeField]
    private bool _FollowOnStart = false;

    // cached transform of the target
    private Transform _CameraTransform;
    // maintain a flag internally to reconnect if target is lost or camera is switched
    private bool _IsFollowing;
    // Represents the current velocity, this value is modified by SmoothDamp() every time you call it
    private float _HeightVel;
    // Represents the position we are trying to reach using SmoothDamp()
    private float targetHeight = 100000.0f;

    #endregion


    #region UNITY CALLBACKS

    private void OnEnable()
    {
        // Start following the target if wanted.
        if (_FollowOnStart)
        {
            StartFollowing();
        }
    }


    private void LateUpdate()
    {
        // the transform target may not destroy on level load, 
        // so we need to cover corner cases where the Main Camera is different everytime
        // we load a new scene, and reconnect when that happens
        if (_CameraTransform == null && _IsFollowing)
        {
            StartFollowing();
        }
        if (_IsFollowing)
        {
            Apply();
        }
    }

    #endregion


    #region PUBLIC METHODS

    /// <summary>
    /// Raises the start following event. 
    /// Use this wwhen you don't know at the time of editing what to follow, typically instances managed by photon network
    /// </summary>
    public void StartFollowing()
    {
        _CameraTransform = Camera.main.transform;
        _IsFollowing = true;
        // we don't smooth anything, we go straight to the right camera shot
        Cut();
    }

    #endregion


    #region PRIVATE METHODS

    /// <summary>
    /// Follow the target smoothly
    /// </summary>
    void Apply()
    {
        Vector3 targetCenter = transform.position + _CenterOffset;
        //Calculate the current & target rotation angles
        float originalTargetAngle = transform.eulerAngles.y;
        float currentAngle = _CameraTransform.eulerAngles.y;
        // Adjust real target angle when the camera is locked
        float targetAngle = originalTargetAngle;
        currentAngle = targetAngle;
        targetHeight = targetCenter.y + _Height;

        //Damp the height
        float currentHeight = _CameraTransform.position.y;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref _HeightVel, _HeightSmoothLag);
        // Convert the angle int a rotation by which we then reposition the camera
        Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);
        _CameraTransform.position = targetCenter;
        _CameraTransform.position += currentRotation * Vector3.back * _Distance;
        // Set the height of the camera
        _CameraTransform.position = new Vector3(_CameraTransform.position.x, currentHeight, _CameraTransform.position.z);
        // Always look at the target
        SetUpRotation(targetCenter);

    }

    /// <summary>
    /// Directly position the camera to a specified Target and center.
    /// </summary>
    void Cut()
    {
        float oldHeightSmooth = _HeightSmoothLag;
        _HeightSmoothLag = 0.001f;
        Apply();
        _HeightSmoothLag = oldHeightSmooth;
    }

    /// <summary>
    /// Sets up the rotation of the camera to always be behind the target
    /// </summary>
    /// <param name="centerPos"> Center Pos </param>
    void SetUpRotation (Vector3 centerPos)
    {
        Vector3 cameraPos = _CameraTransform.position;
        Vector3 offsetToCenter = centerPos - cameraPos;
        // Generate base rotation only around y-axis
        Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));
        Vector3 relativeOffset = Vector3.forward * _Distance + Vector3.down * _Height;
        _CameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);
    }

    #endregion
}
