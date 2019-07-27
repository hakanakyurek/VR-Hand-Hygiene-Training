using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Controllables;

namespace ManusVR
{
    /// <summary>
    /// This script ignores physical contact between the given colliders and non interactable objects.
    /// </summary>
    public class MVR_CollisionToggler : MonoBehaviour
    {
        [Header("Custom Settings")]
        [Tooltip("The gameobject that holds all of the colliders that should be ignored. If left blank it will use itself as the container.")]
        public List<GameObject> customColliderContainers = new List<GameObject>();

        protected List<Collider> ownColliders = new List<Collider>();

        protected bool debugCollision = false;

        // Use this for initialization
        protected virtual void OnEnable()
        {
            if (customColliderContainers.Count == 0)
            {
                customColliderContainers.Add(gameObject);
            }

            // Get all of the colliders that should not be ignored
            List<Collider> interactableColliders = GetAllInteractableColiders();

            // All of the colliders that are childs of this object.
            ownColliders.Clear();
            foreach (var container in customColliderContainers)
            {
                ownColliders.AddRange(container.GetComponentsInChildren<Collider>());
            }

            // Find all of the colliders that are currently in the scene
            List<Collider> allColliders = new List<Collider>(FindObjectsOfType<Collider>());

            // Interactable colliders should not be ignore
            foreach (var interactableCollider in interactableColliders)
            {
                allColliders.Remove(interactableCollider);
            }

            foreach (var collider in allColliders)
            {
                foreach (var ownCollider in ownColliders)
                {
                    Log("Ignoring collision between " + collider.gameObject.name + " and " + ownCollider.gameObject.name);
                    Physics.IgnoreCollision(collider, ownCollider);
                }
            }
        }

        /// <summary>
        /// Ignore or enable collision with the colliders on the hand and the given object
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="ignore"></param>
        public virtual void IgnoreCollisionWithCollidersOnHand(GameObject gameObject, bool ignore)
        {
            Collider[] targetColliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var collider in targetColliders)
            {
                foreach (var ownCollider in ownColliders)
                {
                    if (!ownCollider.isTrigger)
                    {
                        Log("Ignoring: " + ignore + " between: " + ownCollider.name + " - " + gameObject.name);
                        Physics.IgnoreCollision(collider, ownCollider, ignore);
                    }
                }
            }
        }

        public virtual void EnableHandCollision(bool enable)
        {
            foreach (var collider in ownColliders)
            {
                if (!collider.isTrigger)
                {
                    collider.enabled = enable;
                }
            }
        }

        protected virtual List<Collider> GetAllInteractableColiders()
        {
            List<Collider> interactableColliders = new List<Collider>();
            interactableColliders.AddRange(GetCollidersOfComponentType<VRTK_InteractableObject>());
            interactableColliders.AddRange(GetCollidersOfComponentType<VRTK_BaseControllable>());
            return interactableColliders;
        }

        protected List<Collider> GetCollidersOfComponentType<T>() where T : Component
        {
            List<Collider> colliders = new List<Collider>();
            foreach (T interactable in Component.FindObjectsOfType<T>())
            {
                foreach (var collider in interactable.GetComponentsInChildren<Collider>())
                {
                    VRTK_SharedMethods.AddListValue(colliders, collider, true);
                }
            }
            return colliders;
        }

        protected virtual void Log(string log)
        {
            if (debugCollision)
            {
                Debug.Log(log);
            }
        }
    }

}

