using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace ManusVR
{
    public class MVR_PhalangeCollisionInfo : MonoBehaviour
    {

        /// <summary>
        /// The amount of colliders that are currently in the trigger of the phalange.
        /// </summary>
        public int NumberOfCollidersInPhalange => collidersInTrigger.Count;

        /// <summary>
        /// Event that gets called when a collider enters the trigger of the phalange.
        /// </summary>
        public Action<Collider> OnPhalangeEntersCollider;

        /// <summary>
        /// Event that gets callen when a collider exits the trigger of the phalange.
        /// </summary>
        public Action<Collider> OnPhalangeExitsCollider;

        protected List<Collider> collidersInTrigger = new List<Collider>();



        protected virtual void OnEnable()
        {
            collidersInTrigger.Clear();
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (!collider.isTrigger && !collider.GetComponent<MVR_PhalangeCollisionInfo>() &&
                collider.GetComponentInParent<VRTK_InteractableObject>())
            {
                VRTK_SharedMethods.AddListValue(collidersInTrigger, collider, true);

                OnPhalangeEntersCollider?.Invoke(collider);
            }
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            if (collidersInTrigger.Contains(collider))
            {
                collidersInTrigger.Remove(collider);

                OnPhalangeExitsCollider?.Invoke(collider);
            }
        }

    }

}

