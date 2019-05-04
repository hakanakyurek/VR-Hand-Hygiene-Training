using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.ManusVR.Scripts
{
    [Serializable]
    public class HandRig
    {
        public Transform WristTransform;

        public List<FingerRig> Fingers
        {
            get { return new List<FingerRig>() {Thumb, Index, Middle, Ring, Pinky}; }
        }

        public FingerRig Thumb = new FingerRig(), Index = new FingerRig(), Middle = new FingerRig(), Ring = new FingerRig(), Pinky = new FingerRig();

        public HandRig()
        {
            
        }

        public FingerRig GetFingerRig(FingerIndex finger)
        {
            switch (finger)
            {
                case FingerIndex.thumb:
                    return Thumb;
                case FingerIndex.index:
                    return Index;
                case FingerIndex.middle:
                    return Middle;
                case FingerIndex.ring:
                    return Ring;
                case FingerIndex.pink:
                    return Pinky;
                default:
                    throw new ArgumentOutOfRangeException("finger", finger, null);
            }
        }
    }

    [Serializable]
    public class FingerRig
    {
        public List<Transform> Transforms
        {
            get { return new List<Transform>() {Proximal, Intermedial, Distal}; }
        }

        public Transform Proximal, Intermedial, Distal;
    }
}
