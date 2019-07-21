using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

namespace ManusVR
{
    /// <summary>
    /// Fires events when the hand starts to have collision with other interactable objects.
    /// </summary>
    public class MVR_CollisionDetector : MonoBehaviour
    {

        [Tooltip("Assign every root object that contains all of the trigger colliders on the hand.")]
        public List<GameObject> customTriggerColliderRoots = new List<GameObject>();

        /// <summary>
        /// Event that gets called when the hand just started to have collision.
        /// </summary>
        public Action<GameObject> HandStartCollision;
        /// <summary>
        /// Event the gets callen when the hand completely stopped having collision.
        /// </summary>
        public Action<GameObject> HandStopCollision;

        /// <summary>
        /// Event that gets called when one of the fingers just started to have collision.
        /// </summary>
        public Action<GameObject, SDK_BaseController.ButtonTypes> FingerStartCollision;
        /// <summary>
        /// Event that gets called when one of the fingers just stopped having collision.
        /// </summary>
        public Action<GameObject, SDK_BaseController.ButtonTypes> FingerStopCollision;
        /// <summary>
        /// The amount of objects that are colliding with this hand.
        /// </summary>
        public int AmountOfCollidingObjects => fingersCollidingWithCollider.Count;

        /// <summary>
        /// Is the hand colliding with an object.
        /// </summary>
        public bool IsHandColliding => AmountOfCollidingObjects > 0;

        protected List<MVR_FingerCollisionInfo> colliderInfos = new List<MVR_FingerCollisionInfo>();

        protected Dictionary<Collider, List<SDK_BaseController.ButtonTypes>> fingersCollidingWithCollider =
            new Dictionary<Collider, List<SDK_BaseController.ButtonTypes>>();

        protected List<GameObject> interactableObjects = new List<GameObject>();

        private bool debugDetector = false;

        protected virtual void OnEnable()
        {
            if (customTriggerColliderRoots.Count == 0)
            {
                colliderInfos.AddRange(GetComponentsInChildren<MVR_FingerCollisionInfo>());
                Log("No trigger collider root is set for the collision detector. It will use itself as the root gameobject.");
            }
            else
            {
                foreach (var container in customTriggerColliderRoots)
                {
                    colliderInfos.AddRange(container.GetComponentsInChildren<MVR_FingerCollisionInfo>());
                }
            }

            ManageListeners(true);
        }

        protected virtual void OnDisable()
        {
            ManageListeners(false);

            colliderInfos.Clear();
        }

        /// <summary>
        /// Is the given finger colliding with the given collider.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="finger"></param>
        /// <returns></returns>
        public virtual bool IsFingerCollidingWithCollider(Collider collider, SDK_BaseController.ButtonTypes finger)
        {
            return fingersCollidingWithCollider.ContainsKey(collider) && fingersCollidingWithCollider[collider].Contains(finger);
        }

        /// <summary>
        /// Return the fingers that are colliding with the given collider.
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public virtual List<SDK_BaseController.ButtonTypes> FingersCollidingWithCollider(Collider collider)
        {
            if (fingersCollidingWithCollider.ContainsKey(collider))
            {
                return fingersCollidingWithCollider[collider];
            }
            return new List<SDK_BaseController.ButtonTypes>();
        }

        /// <summary>
        ///Return the fingers that are colliding with the given interactable gameobject.
        /// </summary>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        public virtual List<SDK_BaseController.ButtonTypes> FingersCollidingWithGameobject(GameObject gameobject)
        {
            if (!interactableObjects.Contains(gameobject)) return new List<SDK_BaseController.ButtonTypes>();

            HashSet<SDK_BaseController.ButtonTypes> fingers = new HashSet<SDK_BaseController.ButtonTypes>();
            foreach (var coll in gameobject.GetComponentsInChildren<Collider>())
            {
                if (!fingersCollidingWithCollider.ContainsKey(coll)) continue;

                foreach (var finger in fingersCollidingWithCollider[coll])
                {
                    fingers.Add(finger);
                }
            }

            return fingers.ToList();
        }

        /// <summary>
        /// Check if the hand is colliding with the given gameobject
        /// </summary>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        public virtual bool IsCollidingWithGameobject(GameObject gameobject)
        {
            foreach (var collider in fingersCollidingWithCollider.Keys)
            {
                if (GetInteractableObjectFromCollider(collider) == gameobject)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void FingerEntersCollider(Collider collider, SDK_BaseController.ButtonTypes finger)
        {
            // Get the interactable object that is attached to the collider
            GameObject interactableObject = GetInteractableObjectFromCollider(collider);

            if (!fingersCollidingWithCollider.ContainsKey(collider))
            {
                // Create a new list with fingers that are colliding with the given collider.
                fingersCollidingWithCollider.Add(collider, new List<SDK_BaseController.ButtonTypes>());
            }

            // Tell others that the finger started colliding with another object
            if (FingerStartCollision != null && fingersCollidingWithCollider[collider].Contains(finger))
            {
                FingerStartCollision(interactableObject, finger);
            }

            // Add the finger to the list with colliding fingers.
            fingersCollidingWithCollider[collider].Add(finger);

            if (!interactableObjects.Contains(interactableObject))
            {
                interactableObjects.Add(interactableObject);

                // Tell others that the hand is colliding with another object
                HandStartCollision?.Invoke(interactableObject);
                Log("Added interactable " + interactableObject.name + " to list");
            }
        }

        protected virtual void FingerExitsCollider(Collider collider, SDK_BaseController.ButtonTypes finger)
        {
            // Get the interactable object that is attached to the collider
            GameObject interactableObject = GetInteractableObjectFromCollider(collider);

            // Remove the finger from the list of colliding fingers
            fingersCollidingWithCollider[collider].Remove(finger);

            if (fingersCollidingWithCollider[collider].Count <= 0)
            {
                // If no finger is colliding the remove the collider from the list
                fingersCollidingWithCollider.Remove(collider);

                // Tell others that the hand is not colliding with another object
                HandStopCollision?.Invoke(interactableObject);
            }

            // Tell others that the finger stopped colliding with another object
            FingerStopCollision?.Invoke(interactableObject, finger);

            if (!interactableObject.GetComponentsInChildren<Collider>()
                .Any(coll => fingersCollidingWithCollider.ContainsKey(coll)))
            {
                interactableObjects.Remove(interactableObject);
                Log("Removed interactable " + interactableObject.name + " from list");
            }
        }

        protected virtual GameObject GetInteractableObjectFromCollider(Collider collider)
        {
            VRTK_InteractableObject interactableObject = collider.GetComponentInParent<VRTK_InteractableObject>();
            return interactableObject?.gameObject;
        }

        protected virtual void ManageListeners(bool enable)
        {
            if (enable)
            {
                foreach (var info in colliderInfos)
                {
                    info.OnFingerEntersCollider += FingerEntersCollider;
                    info.OnFingerExitsCollider += FingerExitsCollider;
                }

                HandStartCollision += DebugStartHandCollision;
                HandStopCollision += DebugStopHandCollision;

                FingerStartCollision += DebugStartFingerCollision;
                FingerStopCollision += DebugStopFingerCollision;

            }
            else
            {
                foreach (var info in colliderInfos)
                {
                    info.OnFingerEntersCollider -= FingerEntersCollider;
                    info.OnFingerExitsCollider -= FingerExitsCollider;
                }

                HandStartCollision -= DebugStartHandCollision;
                HandStopCollision -= DebugStopHandCollision;

                FingerStartCollision -= DebugStartFingerCollision;
                FingerStopCollision -= DebugStopFingerCollision;
            }
        }

        protected virtual void DebugStartHandCollision(GameObject gameobject)
        {
            Log("Hand started to have collision with " + gameobject.name);
        }

        protected virtual void DebugStopHandCollision(GameObject gameobject)
        {
            Log("Hand stopped having collision with " + gameobject.name);
        }

        protected virtual void DebugStartFingerCollision(GameObject collider, SDK_BaseController.ButtonTypes finger)
        {
            Log(finger + " started colliding  with " + collider);
        }

        protected virtual void DebugStopFingerCollision(GameObject collider, SDK_BaseController.ButtonTypes finger)
        {
            Log(finger + " stopped colliding  with " + collider);
        }

        protected virtual void Log(string log)
        {
            if (debugDetector)
            {
                Debug.Log(log);
            }
        }
    }

}
