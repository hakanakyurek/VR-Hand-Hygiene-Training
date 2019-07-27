using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace ManusVR
{
    /// <summary>
    /// Checks if this finger is colliding with a given object.
    /// </summary>
    /// <remarks>
    ///  **Required Components:**
    ///   * 'Collider' - A Unity collider that checks collisions
    ///   
    ///  **Script Usage:**
    ///   * Place 'VRTK_ManusVRColliderInfo' on one of the colliders in the collider container.
    /// </remarks>
    /// </summary>
    public class MVR_FingerCollisionInfo : MonoBehaviour
    {
        [Tooltip("Specify which type of finger belongs to this collider")]
        public MVR_HandData.FingerTypes FingerIndex;
        [SerializeField]
        public SDK_BaseController.ButtonTypes fingerIndex;

        /// <summary>
        /// The amount of colliders that are in the triggers of this finger.
        /// </summary>
        public int NumberOfCollidersInFinger => collidersInTrigger.Count;

        /// <summary>
        /// Event that gets called when a collider enters the finger.
        /// </summary>
        public Action<Collider, SDK_BaseController.ButtonTypes> OnFingerEntersCollider;

        /// <summary>
        /// Event that gets called when a collider exits the finger.
        /// </summary>
        public Action<Collider, SDK_BaseController.ButtonTypes> OnFingerExitsCollider;

        protected List<Collider> collidersInTrigger = new List<Collider>();
        protected List<MVR_PhalangeCollisionInfo> phalanges = new List<MVR_PhalangeCollisionInfo>();

        private bool debugColliderInfo = false;

        protected virtual void OnEnable()
        {
            collidersInTrigger.Clear();

            phalanges = new List<MVR_PhalangeCollisionInfo>(GetComponentsInChildren<MVR_PhalangeCollisionInfo>());

            ManageListeners(true);
        }

        protected virtual void OnDisable()
        {
            ManageListeners(false);

            phalanges.Clear();
        }

        protected virtual void ManageListeners(bool subscribe)
        {
            if (subscribe)
            {
                foreach (var phalange in phalanges)
                {
                    phalange.OnPhalangeEntersCollider += OnPhalangeEntersCollider;
                    phalange.OnPhalangeExitsCollider += OnPhalangeExitsCollider;
                }
            }
            else
            {
                foreach (var phalange in phalanges)
                {
                    phalange.OnPhalangeEntersCollider -= OnPhalangeEntersCollider;
                    phalange.OnPhalangeExitsCollider -= OnPhalangeExitsCollider;
                }
            }
        }

        protected virtual void OnPhalangeEntersCollider(Collider collider)
        {
            VRTK_SharedMethods.AddListValue(collidersInTrigger, collider, true);

            OnFingerEntersCollider?.Invoke(collider, fingerIndex);
        }

        protected virtual void OnPhalangeExitsCollider(Collider collider)
        {
            collidersInTrigger.Remove(collider);

            OnFingerExitsCollider?.Invoke(collider, fingerIndex);
        }

        protected virtual void OnValidate()
        {
            fingerIndex = MVR_HandData.SetFingerButton(FingerIndex, fingerIndex);
        }

        private void Log(string log)
        {
            if (debugColliderInfo)
            {
                Debug.Log(log);
            }
        }
    }

}

