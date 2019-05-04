// Copyright (c) 2018 ManusVR
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ManusVR.Scripts.Factory;
using UnityEngine;

namespace Assets.ManusVR.Scripts
{
    public interface ICollidingCounter
    {
        int AmountOfCollidingObjects();
    }

    public abstract class Hand : MonoBehaviour, ICollidingCounter
    {

        public Transform[][] FingerTransforms
        {
            get { return _fingerTransforms; }
        }

        private Transform[][] _fingerTransforms = null;
        public device_type_t DeviceType = device_type_t.GLOVE_RIGHT;
        public HandManager HandManager;

        public Dictionary<FingerIndex, Finger> FingerControllers { get { return _fingerControllers; } }
        private readonly Dictionary<FingerIndex, Finger> _fingerControllers = new Dictionary<FingerIndex, Finger>();

        public Rigidbody HandRigidbody { get { return Wrist.Rigidbody; } }

        public Transform WristTransform
        {
            get
            {
                if (_wristTransform == null)
                    FindWrist(transform.root.gameObject.GetComponentInChildren<ManusRigger>());
                return _wristTransform;
            }
        }

        private Transform _wristTransform = null;
        private Quaternion _lastThumbRotation;

        public Wrist Wrist { get; private set; }

        public Transform Thumb
        {
            get
            {
                if (_fingerTransforms == null)
                    FindFingers(transform.root.gameObject.GetComponentInChildren<ManusRigger>());
                return _fingerTransforms[0][1];
            }
        }

        public HandData HandData { get; set; }
        public HandType HandType { get; set; }


        /// <summary>
        ///     Finds a deep child in a transform
        /// </summary>
        /// <param name="aParent">Transform to be searched</param>
        /// <param name="aName">Name of the (grand)child to be found</param>
        /// <returns></returns>
        private static Transform FindDeepChild(Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = FindDeepChild(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        public virtual void Awake()
        {
            Application.runInBackground = true;
            
        }

        /// <summary>
        ///     Constructor which loads the HandModel
        /// </summary>
        public virtual void Start()
        {
            ManusRigger rigger = transform.root.gameObject.GetComponentInChildren<ManusRigger>();
            FindWrist(rigger);
            FindFingers(rigger);
            Wrist = AddWristController(DeviceType);

            foreach (FingerIndex finger in Enum.GetValues(typeof(FingerIndex)))
            {
                _fingerControllers.Add(finger, CreateFinger(finger));
            }
        }

        protected virtual Wrist AddWristController(device_type_t deviceType)
        {
            return HandFactory.GetWristController(WristTransform.gameObject, HandType, deviceType, this);
        }

        private void FindWrist(ManusRigger rigger)
        {
            //Get the cached transform from the manus rigger.
            if (rigger)
                _wristTransform = rigger.GetWristTransform(DeviceType);

            //Get the transform manually if the rigger wasn't assigned or the WristTransform was still not found.
            if (!rigger || !_wristTransform)
                _wristTransform = FindDeepChild(HandManager.RootTransform,
                DeviceType == device_type_t.GLOVE_RIGHT ? "hand_r" : "hand_l");
        }



        private void FindFingers(ManusRigger rigger)
        {
            string[] fingers =
            {
                "thumb_0",
                "index_0",
                "middle_0",
                "ring_0",
                "pinky_0"
            };

            // Associate the game transforms with the skeletal model.
            _fingerTransforms = new Transform[5][];
            for (var i = 0; i < 5; i++)
            {
                _fingerTransforms[i] = new Transform[5];
                for (var j = 1; j < 4; j++)
                {
                    //Get the cached transform from the manus rigger.
                    if (rigger)
                        _fingerTransforms[i][j] =
                            rigger.GetFingerTransform(DeviceType, (FingerIndex) i, (PhalangeType) j);
                    
                    //Manually find the transform if rigger is null or the transform is still not assigned/
                    if (!rigger || !_fingerTransforms[i][j])
                    {
                        var postfix = DeviceType == device_type_t.GLOVE_LEFT ? "_l" : "_r";
                        var finger = fingers[i] + j + postfix;
                        _fingerTransforms[i][j] = FindDeepChild(HandManager.RootTransform, finger);
                    }
                }
            }
        }

        public virtual void FixedUpdate()
        {
            int debugFrameCounter = Time.frameCount;
            if (HandManager.ApplyWristRotation)
                RotateWrist();
            if (HandManager.ApplyThumbRotation)
                RotateThumb();

            MoveWrist();

            //Only update fingers if data has been received
            if (!HandData.HasReceivedData(DeviceType == device_type_t.GLOVE_LEFT
                ? GloveLaterality.GLOVE_LEFT
                : GloveLaterality.GLOVE_RIGHT))
                return;
            foreach (var finger in FingerControllers)
            {
                for (int i = 1; i <= 3; i++)
                    finger.Value.RotatePhalange(i, HandData.GetFingerRotation(finger.Key, DeviceType, i));
            }
        }

        /// <summary>
        /// Tell the wrist to rotate to a given rotation
        /// </summary>
        protected virtual void RotateWrist()
        {
            Wrist.RotateWrist(HandData.CalibratedWristRotation(DeviceType));
        }

        protected virtual void RotateThumb()
        {
            // note that this has become a local rotation with Apollo
            Thumb.localRotation = ThumbRotation();
        }



        protected virtual Finger CreateFinger(FingerIndex finger)
        {
            return HandFactory.GetFinger(_fingerTransforms[(int)finger][1].gameObject, finger,
                HandType, this, DeviceType,
                _fingerTransforms[(int)finger][1].gameObject, _fingerTransforms[(int)finger][2].gameObject,
                _fingerTransforms[(int)finger][3].gameObject);
        }

        protected virtual void MoveWrist()
        {

        }

        /// <summary>
        /// Enables or disables the rotation of the wrist and thumb
        /// </summary>
        /// <param name="enabled"></param>
        public virtual void EnableRotation(bool enabled)
        {
            Wrist.enabled = enabled;
            _fingerControllers[FingerIndex.thumb].enabled = enabled;
        }

        public virtual Quaternion ThumbRotation()
        {
            // var thumbRotation = HandData.ValidOutput(DeviceType)
            //     ? HandManager.HandData.GetThumbRotation(DeviceType)
            //     : _lastThumbRotation;
            // _lastThumbRotation = thumbRotation;
            // return thumbRotation;
            return HandManager.HandData.GetThumbRotation(DeviceType);
        }

        public virtual int AmountOfCollidingObjects()
        {
            return 0;
        }
    }
}
