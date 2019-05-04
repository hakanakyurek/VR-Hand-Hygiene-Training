// Copyright (c) 2018 ManusVR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ManusVR;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Assets.ManusVR.Scripts
{
    public enum FingerIndex
    {
        thumb = 0,
        index = 1,
        middle = 2,
        ring = 3,
        pink = 4
    }

    /// <summary>
    /// List of values/states to check for each hand
    /// Go from big to small numbers
    /// </summary>
    public enum CloseValue
    {
        Fist = 65,
        Small = 30,
        Tiny = 15,
        Open = 5
    }

    /// <summary>
    /// State of each hand
    /// </summary>
    public struct HandValue
    {
        public CloseValue CloseValue;
        public bool IsClosed;
        public bool IsOpen;
        public bool HandOpened;
        public bool HandClosed;
        public ToggleEvent OnValueChanged;
    }

    [System.Serializable]
    public class ToggleEvent : UnityEvent<CloseValue>
    {
    }

    public class HandData : MonoBehaviour
    {
        // Saving the leftHand retrieved from the hand
        private ApolloJointData _leftHand;
        private ApolloJointData _rightHand;
        private ApolloRawData _leftHandRaw;
        private ApolloRawData _rightHandRaw;

        [SerializeField]
        private KeyCode _rotateLeftHandL = KeyCode.S;
        [SerializeField]
        private KeyCode _rotateLeftHandR = KeyCode.A;
        [SerializeField]
        private KeyCode _rotateRightHandL = KeyCode.W;
        [SerializeField]
        private KeyCode _rotateRightHandR = KeyCode.Q;
        [SerializeField][Tooltip("Automatically align the wrists with the Vive trackers")]
        private KeyCode _automaticCalibration = KeyCode.Space;

        public Vector3 PreRotWristLeft = new Vector3(180f, 0f, 0f);
        public Vector3 PreRotWristRight = new Vector3(0f, 0f, 0f);

        public Vector3 PostRotWristLeft;
        public Vector3 PostRotWristRight;

        public Vector3 PreRotThumbLeft;
        public Vector3 PreRotThumbRight;

        [Header("Experimental")][Tooltip("This will enable debugging the connection with Apollo")]
        public bool DebugApollo = false;
        [Tooltip("Use the pinch calibration in Apollo (Only works with Apollo 2019.2)")]
        public bool PinchFilter = false;

        [SerializeField] private TrackingValues _trackingValues;



        public Action OnReceivedData;

        public TrackingValues TrackingValues
        {
            get { return _trackingValues; }
        }



        public bool HasReceivedData(GloveLaterality laterality)
        {
            switch (laterality)
            {
                case GloveLaterality.GLOVE_LEFT:
                    return _leftHand.IsValid;
                case GloveLaterality.GLOVE_RIGHT:
                    return _rightHand.IsValid;
                default:
                    throw new ArgumentOutOfRangeException("laterality", laterality, null);
            }
        }

        public ApolloJointData GetHandData(GloveLaterality laterality)
        {
            switch (laterality)
            {
                case GloveLaterality.GLOVE_LEFT:
                    return _leftHand;
                case GloveLaterality.GLOVE_RIGHT:
                    return _rightHand;
                default:
                    throw new ArgumentOutOfRangeException("laterality", laterality, null);
            }
        }

        public ApolloRawData GetRawHandData(GloveLaterality laterality)
        {
            switch (laterality)
            {
                case GloveLaterality.GLOVE_LEFT:
                    return _leftHandRaw;
                case GloveLaterality.GLOVE_RIGHT:
                    return _rightHandRaw;
                default:
                    throw new ArgumentOutOfRangeException("laterality", laterality, null);
            }
        }

        /// <summary>
        /// Get the close value of the hand
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public CloseValue GetCloseValue(device_type_t deviceType)
        {
            return _handValues[(int) deviceType].CloseValue;
        }

        public UnityEvent<CloseValue> GetOnValueChanged(device_type_t deviceType)
        {
            return _handValues[(int) deviceType].OnValueChanged;
        }

        /// <summary>
        /// Check if the hand just opened
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public bool HandOpened(device_type_t deviceType)
        {
            return _handValues[(int) deviceType].HandOpened;
        }

        /// <summary>
        /// Check if the hand just closed
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public bool HandClosed(device_type_t deviceType)
        {
            return _handValues[(int) deviceType].HandClosed;
        }

        private HandValue[] _handValues = new HandValue[2];
        
        // Use this for initialization
        public virtual void Start()
        {
            Application.runInBackground = true;

            // Register delegates for incoming handData
            Apollo apollo = Apollo.GetInstance(DebugApollo, PinchFilter);
            apollo.RegisterDataListener(newHandData);
            apollo.RegisterDataListener(newRawData);
        }

        public void newHandData(ApolloJointData data, GloveLaterality side)
        {
            //todo clean up null check
            if ( data.fingers == null)
                return;

            switch(side)
            {
                case GloveLaterality.GLOVE_LEFT:
                    // store the jointData for later use
                    _leftHand = data;
                    break;
                case GloveLaterality.GLOVE_RIGHT:
                    // store the jointData for later use
                    _rightHand = data;
                    break;
            }
        }


        public void newRawData(ApolloRawData data, GloveLaterality side)
        {

            switch (side)
            {
                case GloveLaterality.GLOVE_LEFT:
                    // store the raw data
                    _leftHandRaw = data;
                    break;
                case GloveLaterality.GLOVE_RIGHT:
                    // store the raw data
                    _rightHandRaw = data;

                    break;
            }
        }

        private void FixedUpdate()
        {
            // process the raw data
            UpdateCloseValue(TotalAverageValue(_leftHandRaw), device_type_t.GLOVE_LEFT);
            UpdateCloseValue(TotalAverageValue(_rightHandRaw), device_type_t.GLOVE_RIGHT);
        }

        private void Update()
        {
            ManualWristRotation();

            if (Input.GetKeyDown(_automaticCalibration))
            {
                CalibrateAlignment(device_type_t.GLOVE_LEFT);
                CalibrateAlignment(device_type_t.GLOVE_RIGHT);
                Debug.Log("Calibrated the hands with the Vive trackers");
            }

        }

        private void ManualWristRotation()
        {
            const float speed = 30;
            if (Input.GetKey(_rotateRightHandL))
                _trackingValues.HandYawOffset[device_type_t.GLOVE_RIGHT] -= Time.deltaTime * speed;
            if (Input.GetKey(_rotateRightHandR))
                _trackingValues.HandYawOffset[device_type_t.GLOVE_RIGHT] += Time.deltaTime * speed;
            if (Input.GetKey(_rotateLeftHandL))
                _trackingValues.HandYawOffset[device_type_t.GLOVE_LEFT] -= Time.deltaTime * speed;
            if (Input.GetKey(_rotateLeftHandR))
                _trackingValues.HandYawOffset[device_type_t.GLOVE_LEFT] += Time.deltaTime * speed;
        }

        /// <summary>
        /// Get the thumb imu rotation of the given device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public Quaternion GetThumbRotation(device_type_t deviceType)
        {
            Vector3 preRotThumb = deviceType == device_type_t.GLOVE_LEFT ? PreRotThumbLeft : PreRotThumbRight;
            var thumbRotOffset = Quaternion.Euler(preRotThumb);

            switch (deviceType)
            {
                case device_type_t.GLOVE_LEFT:
      		    return thumbRotOffset * GetFingerRotation(FingerIndex.thumb, device_type_t.GLOVE_LEFT, 1);
                case device_type_t.GLOVE_RIGHT:
                    return thumbRotOffset * GetFingerRotation(FingerIndex.thumb, device_type_t.GLOVE_RIGHT, 1);
                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Get the wrist rotation of a given device
        /// </summary>
        /// <param name="deviceType">The device type</param>
        /// <returns></returns>
        public Quaternion GetWristRotation(device_type_t deviceType)
        {
            Vector3 postRot = deviceType == device_type_t.GLOVE_LEFT ? PostRotWristLeft : PostRotWristRight;
            var wristRotOffset = Quaternion.Euler(postRot);
            switch (deviceType)
            {

                case device_type_t.GLOVE_LEFT:
                    return transform.rotation * _leftHand.wrist * wristRotOffset;
                case device_type_t.GLOVE_RIGHT:
                    return transform.rotation * _rightHand.wrist * wristRotOffset;
                default:
                    return Quaternion.identity;
            }
        }


        /// <summary>
        /// Align the wrists with the vive trackers
        /// </summary>
        /// <param name="deviceType"></param>
        void CalibrateAlignment(device_type_t deviceType)
        {
            TrackingManager trackingManager = GetComponent<TrackingManager>();

            if (trackingManager == null)
            {
                Debug.LogWarning("No trackingmanager attached to this gameobject");
                return;
            }
            if (trackingManager.UseCustomTrackingOffset)
            {
                Debug.LogWarning("Automatic alignment will only work when use custom tracking is turned off");
                return;
            }
            
            // Get the rotation of the vive tracker
            Quaternion trackerRotation = deviceType == device_type_t.GLOVE_RIGHT ?
                trackingManager.rightTracker.rotation
                : trackingManager.leftTracker.rotation;

            // Get the pre rotation that is needed to rotate the wrist in the right direction
            Quaternion preRotation = PreRotation(deviceType);

            // Multiply the pre rotation with the pre rotation
            Quaternion wristRotation = preRotation * RawWristRotation(deviceType);

            // Align the hand frame with the frame of the vive tracker
            wristRotation *= deviceType == device_type_t.GLOVE_LEFT
                    ? Quaternion.Euler(0, 0, -180)
                    : Quaternion.Euler(0, 0, 90);

            // Calculate the offset in the y world axis between the wrist and the vive tracker
            Quaternion offset =  wristRotation * Quaternion.Inverse(preRotation) * Quaternion.Inverse(trackerRotation) * preRotation;

            _trackingValues.HandYawOffset[deviceType] = Quaternion.Inverse(offset).eulerAngles.y;

        }
        
        /// <summary>
        /// Get the wrist rotation with adding any calibration values
        /// </summary>
        /// <param name="deviceType">The device type</param>
        /// <returns></returns>
        public Quaternion RawWristRotation(device_type_t deviceType)
        {
            switch (deviceType)
            {
                case device_type_t.GLOVE_LEFT:
                    return _leftHand.wrist;
                case device_type_t.GLOVE_RIGHT:
                    return _rightHand.wrist;
                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Get the wrist rotation with added calibration values for alignment
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public Quaternion CalibratedWristRotation(device_type_t deviceType)
        {
            // Create a rotation from the yawoffset that was calculated during calibration
            Quaternion yawOffset = Quaternion.Euler(0, _trackingValues.HandYawOffset[deviceType], 0); 
            Quaternion preRotation = PreRotation(deviceType) * yawOffset;
            Quaternion postRotation = PostRotation(deviceType);

            switch (deviceType)
            {
                case device_type_t.GLOVE_LEFT:
                    return preRotation * _leftHand.wrist * postRotation;
                case device_type_t.GLOVE_RIGHT:
                    return preRotation * _rightHand.wrist * postRotation;
                default:
                    return Quaternion.identity;
            }
        }

        private Quaternion PostRotation(device_type_t deviceType)
        {
            var postVector = deviceType == device_type_t.GLOVE_LEFT ? PostRotWristLeft : PostRotWristRight;
            return Quaternion.Euler(postVector);
        }

        private Quaternion PreRotation(device_type_t deviceType)
        {
            var preVector = deviceType == device_type_t.GLOVE_LEFT ? PreRotWristLeft : PreRotWristRight;
            return Quaternion.Euler(preVector);
        }

        /// <summary>
        /// Get the rotation of the given finger
        /// </summary>
        /// <param name="fingerIndex"></param>
        /// <param name="deviceType"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        public Quaternion GetFingerRotation(FingerIndex fingerIndex, device_type_t deviceType, int pose)
        {
            ApolloJointData hand = GetJointData(deviceType);
            if (!hand.IsValid) return Quaternion.identity;

            Quaternion fingerRotation = hand.fingers[(int) fingerIndex].joints[pose].rotation;

            return fingerRotation;
        }

        private ApolloJointData GetJointData(device_type_t deviceType)
        {
            return deviceType == device_type_t.GLOVE_LEFT ? _leftHand : _rightHand;
        }

        /// <summary>
        /// Get the average value of the first joints without the thumb
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public double FirstJointAverage(device_type_t deviceType)
        {
            ApolloRawData raw = deviceType == device_type_t.GLOVE_LEFT ? _leftHandRaw : _rightHandRaw;
            
            double total = 0;
            total += raw.flex(1);
            total += raw.flex(3);
            total += raw.flex(5);
            total += raw.flex(7);
            return total / 4;
        }

        /// <summary>
        /// Get the average value of all fingers combined on the given hand
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public double TotalAverageValue(ApolloRawData raw)
        {
            int sensors = 0;
            double total = 0;
            // Loop through all of the finger values (except the thumb)
            for (int bendPosition = 0; bendPosition < 8; bendPosition++)
            {
                sensors ++;
                total += raw.flex(bendPosition);
            }

            return total / sensors;
        }

        internal void UpdateCloseValue(double averageSensorValue, device_type_t deviceType)
        {
            var values = Enum.GetValues(typeof(CloseValue));
            HandValue handValue;
            if (deviceType == device_type_t.GLOVE_LEFT)
                handValue = _handValues[0];
            else
                handValue = _handValues[1];

            CloseValue closest = CloseValue.Open;
            // Save the old value for comparisment
            CloseValue oldClose = handValue.CloseValue;

            // Get the current close value
            foreach (CloseValue item in values)
            {
                // Div by 100.0 is used because an enum can only contain ints
                if (averageSensorValue > (double) item / 100.0)
                    closest = item;
            }

            handValue.CloseValue = closest;

            // Invoke the on value changed event
            if (oldClose != handValue.CloseValue && handValue.OnValueChanged != null)
                handValue.OnValueChanged.Invoke(handValue.CloseValue);

            // Check if the hand just closed
            handValue.HandClosed = oldClose == CloseValue.Tiny && handValue.CloseValue == CloseValue.Small;
            // Check if the hand just opened
            handValue.HandOpened = (oldClose == CloseValue.Small && handValue.CloseValue == CloseValue.Open);

            if (deviceType == device_type_t.GLOVE_LEFT)
                _handValues[0] = handValue;
            else
                _handValues[1] = handValue;

         
        }

        public void SetApolloInputEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void ManualJointInput(JointDataNode input)
        {
            if(input.JointDataLeft.fingers != null && input.JointDataLeft.fingers.Length > 0)
                _leftHand = input.JointDataLeft;
            if (input.JointDataRight.fingers != null && input.JointDataRight.fingers.Length > 0)
                _rightHand = input.JointDataRight;

            input.RawDataLeft.flexRaw = input.FlexDataLeft.Array;
            input.RawDataRight.flexRaw = input.FlexDataRight.Array;

            _leftHandRaw = input.RawDataLeft;
            _rightHandRaw = input.RawDataRight;
            UpdateCloseValue(TotalAverageValue(_rightHandRaw), device_type_t.GLOVE_RIGHT);
        }
    }
}
