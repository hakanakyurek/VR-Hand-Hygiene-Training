using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace ManusVR
{

    /// <summary>
    /// Determines if the ManusVRTouch can initiate a grab with the touched Interactable Object.
    /// </summary>
    /// <remarks>
    /// **Required Components:**
    ///  * `VRTK_ManusVRTouch` - The touch component to determine when a valid touch has taken place to denote a grab can occur. This must be applied on the same GameObject as this script if one is not provided via the `Interact Touch` parameter.
    ///
    /// **Optional Components:**
    ///  * `VRTK_ControllerEvents` - The events component to listen for the button presses on. This must be applied on the same GameObject as this script if one is not provided via the `Controller Events` parameter.
    ///
    /// **Script Usage:**
    ///  * Place the `VRTK_InteractGrab` script on either:
    ///    * The GameObject with the Interact Touch and Controller Events scripts.
    ///    * Any other scene GameObject and provide a valid `VRTK_ControllerEvents` component to the `Controller Events` parameter and a valid `VRTK_InteractTouch` component to the `Interact Touch` parameter of this script.
    /// </remarks>
    public class VRTK_MVRGrab : VRTK_InteractGrab
    {
        [Header("ManusVR Custom Settings")]
        [Tooltip("The Collision Detector handles the collision.  If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
        public MVR_CollisionDetector collisionDetector;
        [Tooltip("The Collision Manager manages the collision.  If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
        public MVR_CollisionToggler collisionManager;

        /// <summary>
        /// Is the interactgrab actually grabbing an object.
        /// </summary>
        public bool IsGrabbing => grabbedObject != null;

        /// <summary>
        /// Get the current grabbed object.
        /// </summary>
        public GameObject GrabbedObject => grabbedObject;

        protected VRTK_MVRTouch manusTouch;
        protected List<GameObject> ungrabbableObjects = new List<GameObject>();

        private bool debugGrab = false;

        /// <summary>
        /// Check if the given object is in the list of ungrabbable objects or not.
        /// Objects are placed in this list once they are grabbed to prevent them from being re-grabbed immediately after releasing them.
        /// They are removed from the list once the hand is no longer colliding with the object.
        /// </summary>
        /// <param name="gameObject">The object that should be looked for in the list of ungrabbable objects.</param>
        /// <returns>True if the object is in the list of ungrabbable objects and should be ignored, false if it is not in the list.</returns>
        public virtual bool IsUngrabbableObject(GameObject gameObject)
        {
            if (gameObject != null && !ungrabbableObjects.Contains(gameObject))
            {
                return true;
            }

            return false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            manusTouch = (VRTK_MVRTouch)interactTouch;
            collisionDetector = collisionDetector != null ? collisionDetector : GetComponentInChildren<MVR_CollisionDetector>();
            collisionManager = collisionManager != null ? collisionManager : GetComponentInChildren<MVR_CollisionToggler>();

            ManageListeners(true);

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PreventCollisionWithGrabbedObject(0.25f));
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ManageListeners(false);

            StopAllCoroutines();
        }

        protected virtual void ManageListeners(bool enable)
        {
            if (!collisionDetector)
            {
                return;
            }

            if (enable)
            {
                ControllerGrabInteractableObject += HandGrabbedObject;
                ControllerUngrabInteractableObject += HandUngrabbedObject;
                collisionDetector.HandStopCollision += HandStoppedCollidingWithObject;
            }
            else
            {
                ControllerGrabInteractableObject -= HandGrabbedObject;
                ControllerUngrabInteractableObject -= HandUngrabbedObject;
                collisionDetector.HandStopCollision -= HandStoppedCollidingWithObject;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (grabbedObject == null)
            {
                // Try to grab an object when no object is grabbed
                GameObject grabbableObject = GetGrabbableObject();

                // Attempt to grab the object if it isn't currently ungrabbable.
                if (!ungrabbableObjects.Contains(grabbableObject))
                {
                    AttemptGrab();
                }
            }
            else if (ObjectIsReleasable())
            {
                // Try to release the object
                AttemptReleaseObject();
            }
        }

        protected virtual void HandUngrabbedObject(object sender, ObjectInteractEventArgs e)
        {
            MakeObjectTouchable(e.target, true);
        }

        protected virtual void HandGrabbedObject(object sender, ObjectInteractEventArgs e)
        {
            MakeObjectTouchable(e.target, false);
        }

        protected virtual void HandStoppedCollidingWithObject(GameObject gameObject)
        {
            MakeObjectTouchable(gameObject, true);
        }

        protected virtual void MakeObjectTouchable(GameObject gameObject, bool enableTouch)
        {
            if (enableTouch)
            {
                // Only enable collision when the object is not colliding anymore
                if (!collisionDetector.IsCollidingWithGameobject(gameObject))
                {
                    ungrabbableObjects.Remove(gameObject);
                    if (collisionManager)
                    {
                        // Enable collision beteen the gameobject and the hand.
                        collisionManager.IgnoreCollisionWithCollidersOnHand(gameObject, false);
                    }
                }
            }
            else
            {
                if (collisionManager)
                {
                    // Disable collision between the gameobject and the hand.
                    collisionManager.IgnoreCollisionWithCollidersOnHand(gameObject, true);
                }

                if (!ungrabbableObjects.Contains(gameObject))
                {
                    // Object should be ungrabbable until the hand no longer colliding with the object.
                    ungrabbableObjects.Add(gameObject);
                }
            }
        }

        protected virtual bool ObjectIsReleasable()
        {
            var fingers = manusTouch.TouchingFingers;
            fingers.Remove(SDK_BaseController.ButtonTypes.Touchpad);
            return fingers.Count < 1;
        }

        protected override void PerformGrabAttempt(GameObject objectToGrab)
        {
            // Code taken from base class -----------------------
            if (grabbedObject != null)
                return;

            IncrementGrabState();
            // Changed compared to base class: store return value
            bool grabbed = IsValidGrabAttempt(objectToGrab);
            undroppableGrabbedObject = GetUndroppableObject();
            // End of code taken from base class ----------------

            if (!grabbed)
            {
                return;
            }

            GameObject objectGrabbed = GetGrabbedObject();

            Log("Grabbed the following item: " + objectGrabbed);
        }

        protected virtual IEnumerator PreventCollisionWithGrabbedObject(float interval)
        {
            //todo remove this method since it is only a workaround to prevent collision between the grabbed object and the hand.
            while (true)
            {
                yield return new WaitForSeconds(interval);

                if (collisionManager && grabbedObject != null)
                {
                    collisionManager.IgnoreCollisionWithCollidersOnHand(grabbedObject, true);
                }
            }
        }

        protected virtual void Log(string log)
        {
            if (debugGrab)
            {
                Debug.Log(log);
            }
        }
    }
}
