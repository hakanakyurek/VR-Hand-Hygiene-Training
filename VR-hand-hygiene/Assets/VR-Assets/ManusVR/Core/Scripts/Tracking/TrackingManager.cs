// Copyright (c) 2018 ManusVR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ManusVR;
using UnityEngine;
using Valve.VR;

namespace Assets.ManusVR.Scripts
{
    public class TrackingManager : MonoBehaviour
    {
        public enum EIndex
        {
            None = -1,
            Hmd = (int) OpenVR.k_unTrackedDeviceIndex_Hmd,
            Limit = (int) OpenVR.k_unMaxTrackedDeviceCount
        }

        public enum EUsableTracking
        {
            Controller,
            GenericTracker
        }

        private enum ERole
        {
            HMD,
            LeftHand,
            RightHand
        }

        private class TrackedDevice
        {
            public int index;
            public bool isValid;
        }

        [Header("Tracking settings")]
        public EUsableTracking trackingToUse = EUsableTracking.GenericTracker;
        private ETrackedDeviceClass _trackingToUse = ETrackedDeviceClass.GenericTracker;
        [Tooltip("Ignore the offset that is included with the GenericTracker or Controller option and use your own.")]
        public bool UseCustomTrackingOffset = true;

        public Vector3 LCustomPositionOffset = Vector3.zero;
        public Vector3 RCustomPositionOffset = Vector3.zero;

        public Vector3 LCustomRotationOffset = Vector3.zero;
        public Vector3 RCustomRotationOffset = Vector3.zero;

        [Header("Transform settings")]
        public Transform HMD;
        public Transform leftTracker;
        public Transform rightTracker;
        public static TrackingManager Instance;
        private Transform[] trackerTransforms;

        private TrackedDevice[] devices;

        private bool shouldTrackDevices = true;

        SteamVR_Events.Action newPosesAction;

        [SerializeField] private TrackingValues _trackingValues;
        public KeyCode switchArmsButton = KeyCode.None;

        // Use this for initialization
        void Start()
        {
            _trackingToUse = trackingToUse == EUsableTracking.Controller ? ETrackedDeviceClass.Controller : ETrackedDeviceClass.GenericTracker;

            trackerTransforms = new Transform[3];
            trackerTransforms[(int) ERole.HMD] = HMD;
            trackerTransforms[(int) ERole.LeftHand] = leftTracker;
            trackerTransforms[(int) ERole.RightHand] = rightTracker;

            int num = System.Enum.GetNames(typeof(ERole)).Length;
            devices = new TrackedDevice[num];

            for (int i = 0; i < num; i++)
            {
                devices[i] = new TrackedDevice();
                devices[i].index = new int();
                devices[i].index = (int) EIndex.None;
                devices[i].isValid = new bool();
                devices[i].isValid = false;

                GetIndex(i);
            }

            bool _useTrackers = trackingToUse == TrackingManager.EUsableTracking.GenericTracker;

            if (_trackingValues.AreArmsSwitched)
                SwitchArms(false);
        }

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
        }

        void OnEnable()
        {
            var render = SteamVR_Render.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            newPosesAction.enabled = true;
        }

        void OnDisable()
        {
            newPosesAction.enabled = false;

            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i] != null)
                    devices[i].isValid = false;
            }
                
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(switchArmsButton))
            {
                SwitchArms();
            }
        }

        public void SwitchArms(bool updateSettings = true)
        {
            if (updateSettings)
                _trackingValues.AreArmsSwitched = !_trackingValues.AreArmsSwitched;
            Transform left = trackerTransforms[(int)ERole.LeftHand];
            trackerTransforms[(int)ERole.LeftHand] = trackerTransforms[(int)ERole.RightHand];
            trackerTransforms[(int)ERole.RightHand] = left;
            _switched = !_switched;
        }

        private bool _switched = false;

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (!shouldTrackDevices)
                return;

            for (int deviceNum = 0; deviceNum < devices.Length; deviceNum++)
            {
                // if no role is set or the tracked object is the head
                //if (myRole == ETrackedControllerRole.Invalid && !isHead)
                if (devices[deviceNum].index == (int)EIndex.None)
                    continue;

                int intIndex = (int)devices[deviceNum].index;
                devices[deviceNum].isValid = false;

                if (poses.Length <= intIndex)
                    continue;
                try
                {
                    if (!poses[intIndex].bDeviceIsConnected)
                        continue;
                }
                catch (System.IndexOutOfRangeException)
                {
                    // retry to get the glove index
                    GetIndex(deviceNum);
                    continue;
                }

                if (!poses[intIndex].bPoseIsValid)
                    continue;
                devices[deviceNum].isValid = true;

                var pose = new SteamVR_Utils.RigidTransform(poses[intIndex].mDeviceToAbsoluteTracking);

                // make sure the offset is localized
                trackerTransforms[deviceNum].position = transform.TransformPoint(pose.pos);
                trackerTransforms[deviceNum].rotation = pose.rot * transform.rotation;

                
                if (deviceNum == 1 || deviceNum == 2)
                {
                    int switchedNum = deviceNum;
                    if (_switched && deviceNum == 1)
                        switchedNum = 2;
                    if (_switched && deviceNum == 2)
                        switchedNum = 1;

                    // Set the offsets
                    if (UseCustomTrackingOffset)
                    {
                        if (switchedNum == 1)
                            AddOffsetToDevice(leftTracker, LCustomPositionOffset, LCustomRotationOffset);
                        else if (switchedNum == 2)
                            AddOffsetToDevice(rightTracker, RCustomPositionOffset, RCustomRotationOffset);
                    }

                    else if (_trackingToUse == ETrackedDeviceClass.Controller)
                    {
                        if (switchedNum == 1)
                            AddOffsetToDevice(leftTracker, new Vector3(0, 0.05f, 0f), new Vector3(0, 90, 0));
                        else if (switchedNum == 2)
                            AddOffsetToDevice(rightTracker, new Vector3(0, 0.05f, 0f), new Vector3(0, 90, -90));
                    }

                    else if (_trackingToUse == ETrackedDeviceClass.GenericTracker)
                    {
                        if (switchedNum == 1)
                            AddOffsetToDevice(leftTracker, new Vector3(0, -0.04f, -0.05f), new Vector3(0, -90, 90));
                        else if (switchedNum == 2)
                            AddOffsetToDevice(rightTracker, new Vector3(0, -0.04f, -0.05f), new Vector3(0, -90, 0));
                    }
                }
            }            
        }

        void AddOffsetToDevice(Transform device, Vector3 position, Vector3 rotation)
        {
            device.position += device.TransformVector(position);   
            device.rotation *= Quaternion.Euler(rotation);
        }
    
        void GetIndex(int deviceNum)
        {
            ERole role = (ERole) deviceNum;

            if (role == ERole.HMD)
            {
                devices[deviceNum].index = (int) EIndex.Hmd;
                return;
            }

            int DeviceCount = 0;

            for (uint i = 0; i < (uint) EIndex.Limit; i++)
            {
                ETrackedPropertyError error = new ETrackedPropertyError();
                ETrackedDeviceClass type;
                if (OpenVR.System != null)
                    type = (ETrackedDeviceClass) OpenVR.System.GetInt32TrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_DeviceClass_Int32, ref error);
                else
                {
                    continue;
                }

                if (_trackingToUse == ETrackedDeviceClass.Controller && type == ETrackedDeviceClass.Controller
                    || _trackingToUse == ETrackedDeviceClass.GenericTracker && type == ETrackedDeviceClass.GenericTracker)
                {
                    if (role == ERole.LeftHand && DeviceCount == 0 || role == ERole.RightHand && DeviceCount == 1)
                    {
                        devices[deviceNum].index = (int) i;
                        return;
                    }

                    DeviceCount++;
                }
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            shouldTrackDevices = !enabled;
            newPosesAction.enabled = enabled;
        }

        public void ManualInput(TrackingDataNode trackingData, bool playRotationData)
        {
            trackerTransforms[(int)ERole.LeftHand].localPosition = trackingData.LeftTrackerPosition;
            trackerTransforms[(int)ERole.RightHand].localPosition = trackingData.RightTrackerPosition;
            trackerTransforms[(int)ERole.HMD].localPosition = trackingData.HMDPosition;

            if (playRotationData)
            {
                trackerTransforms[(int)ERole.LeftHand].localRotation = trackingData.LeftTrackerRotation;
                trackerTransforms[(int)ERole.RightHand].localRotation = trackingData.RightTrackerRotation;
                trackerTransforms[(int)ERole.HMD].localRotation = trackingData.HMDRotation;
            }
        }
    }
}