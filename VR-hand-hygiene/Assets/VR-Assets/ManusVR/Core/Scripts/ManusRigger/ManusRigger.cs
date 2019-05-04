using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.ManusVR.Scripts;

namespace Assets
{
    public enum PhalangeType
    {
        Proximal = 1,
        Intermedial = 2,
        Distal = 3
    }

    public class ManusRigger : MonoBehaviour
    {
        [SerializeField]
        public HandRig LeftHand, RightHand;

        private Animator GetAnimator(device_type_t deviceType)
        {
            return null;
        }

        public Transform GetWristTransform(device_type_t deviceType)
        {
            return GetHand(deviceType).WristTransform;
        }

        public Transform GetFingerTransform(device_type_t deviceType, FingerIndex finger, PhalangeType phalange)
        {
            switch (phalange)
            {
                case PhalangeType.Proximal:
                    return GetHand(deviceType).GetFingerRig(finger).Proximal;
                case PhalangeType.Intermedial:
                    return GetHand(deviceType).GetFingerRig(finger).Intermedial;
                case PhalangeType.Distal:
                    return GetHand(deviceType).GetFingerRig(finger).Distal;
                default:
                    throw new ArgumentOutOfRangeException("phalange", phalange, null);
            }
        }

        public Transform GetTransformByBone(device_type_t deviceType, HumanBodyBones boneID)
        {
            Animator anim = GetAnimator(deviceType);
            if (anim == null)
                return null;
            
            return anim.GetBoneTransform(boneID);
        }

        private HandRig GetHand(device_type_t deviceType)
        {
            return deviceType == device_type_t.GLOVE_LEFT ? LeftHand : RightHand;
        }

        void OnValidate()
        {
            List<Transform> transforms = new List<Transform>();
            transforms.AddRange(LeftHand.Fingers.SelectMany(f => f.Transforms));
            transforms.AddRange(RightHand.Fingers.SelectMany(f => f.Transforms));

            var hashset = new HashSet<Transform>();
            foreach (var t in transforms)
            {
                if (!hashset.Add(t))
                {
                    Debug.LogError("ManusRigger: Transform " + t.name + " is assigned more than once!");
                }
            }
        }
        void Reset()
        {
            LeftHand = new HandRig();
            RightHand = new HandRig();

            string hand = "hand";
            string left = "_l";
            string right = "_r";
            string prox = "_01";
            string inter = "_02";
            string dist = "_03";

            LeftHand.WristTransform = transform.FindDeepChild(hand + left);
            RightHand.WristTransform = transform.FindDeepChild(hand + right);

            for (int i = 0; i < 5; i++)
            {
                string finger = "";
                switch (i)
                {
                    case 0:
                        finger = "thumb";
                        break;
                    case 1:
                        finger = "index";
                        break;
                    case 2:
                        finger = "middle";
                        break;
                    case 3:
                        finger = "ring";
                        break;
                    case 4:
                        finger = "pinky";
                        break;
                }

                LeftHand.GetFingerRig((FingerIndex)i).Proximal = transform.FindDeepChild(finger + prox + left);
                LeftHand.GetFingerRig((FingerIndex)i).Intermedial = transform.FindDeepChild(finger + inter + left);
                LeftHand.GetFingerRig((FingerIndex)i).Distal = transform.FindDeepChild(finger + dist + left);

                RightHand.GetFingerRig((FingerIndex)i).Proximal = transform.FindDeepChild(finger + prox + right);
                RightHand.GetFingerRig((FingerIndex)i).Intermedial = transform.FindDeepChild(finger + inter + right);
                RightHand.GetFingerRig((FingerIndex)i).Distal = transform.FindDeepChild(finger + dist + right);
            }
            
        }
    }
}
