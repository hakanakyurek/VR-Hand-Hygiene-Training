using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

namespace ManusVR
{
    /// <summary>
    /// Determines if a GameObject can initiate a touch with an Interactable Object. 
    /// Extra functionality is added to fine tune the way that you can pickup objects with your fingers
    /// </summary>
    /// <remarks>
    /// **Required Components:**
    ///  * `Rigidbody` - A Unity kinematic Rigidbody to determine when collisions happen between the Interact Touch GameObject and other valid colliders.
    ///
    /// **Script Usage:**
    ///  * Place the `VRTK_ManusVRTouch` script on the controller script alias GameObject of the controller to track (e.g. Right Controller Script Alias).
    /// </remarks>
    /// </summary>
    public class VRTK_MVRTouch : VRTK_InteractTouch
    {
        [Tooltip("Choose which fingers are at least required to pickup the object. A minimum of two fingers is always required")]
        public List<MVR_HandData.FingerTypes> RequiredFingers = new List<MVR_HandData.FingerTypes> { MVR_HandData.FingerTypes.Thumb };
        [Header("ManusVR Finger Touch Settings")]
        [Range(0, 1f)]
        [Tooltip("The minimum value of the metacarpophalangeal joint (bottom joint) before a touch is detected. (0 = open, 1 = closed)")]
        public float MinMcpBend = 0.15f;
        [Range(0, 1f)]
        [Tooltip("The maximum value of the metacarpophalangeal joint (bottom joint) before a touch is detected. (0 = open, 1 = closed)")]
        public float MaxMcpBend = 0.9f;
        [Range(0, 1f)]
        [Tooltip("The minimum value of the proximal interphalangeal (upper joint) before a touch is detected. (0 = open, 1 = closed)")]
        public float MinPipBend = 0.05f;
        [Range(0, 1f)]
        [Tooltip("The maximum value of the proximal interphalangeal (upper joint) before a touch is detected. (0 = open, 1 = closed)")]
        public float MaxPipBend = 0.9f;

        [Header("ManusVR Finger Release Settings")]
        [Tooltip("If checked, a multiplier instead of a constant number will be used to decide if the fingers should release the object.")]
        public bool useMultiplierForReleaseThreshold = false;
        [Range(-1, 1f)]
        [Tooltip("Release the object if the bend value on pickup is smaller than the current bend value minus the threshold multiplier.")]
        public float releaseThresholdMultiplier = 0.90f;
        [Range(0, 0.1f)]
        [Tooltip("Release the object if the bend value on pickup is smaller than the current bend value minus the threshold delta.")]
        public float releaseThresholdDelta = 0.03f;
        [Tooltip("If checked, the system will temporarily disable a finger that just released an object, to make sure that the finger can't immediately regrab the object.")]
        public bool temporarilyDisableFingersOnRelease = false;
        [Tooltip("The amount of time a finger should be disabled after releasing an object.")]
        public float postReleaseFingerDisableTime = 0.5f;

        [Header("ManusVR Custom Settings")]
        [Tooltip("The Interact Grab that handles grabbing. If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
        public VRTK_MVRGrab interactGrab;
        [Tooltip("The Collision Detector handles the collision.  If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
        public MVR_CollisionDetector collisionDetector;

        /// <summary>
        /// The fingers (button types) that are attached to the hand
        /// </summary>
        public List<SDK_BaseController.ButtonTypes> FingersOnHand => colliderInfos.Keys.ToList();

        /// <summary>
        /// All of the finger collision info scripts that are on the hand.
        /// </summary>
        public List<MVR_FingerCollisionInfo> FingerCollisionInfosOnHand => colliderInfos.Values.ToList();

        /// <summary>
        /// All of the fingers that are currently touching the touched object
        /// </summary>
        public Dictionary<SDK_BaseController.ButtonTypes, Vector2> TouchingFingers => touchingFingers;

        protected List<SDK_BaseController.ButtonTypes> requiredFingers = new List<SDK_BaseController.ButtonTypes>();

        protected Dictionary<SDK_BaseController.ButtonTypes, Vector2> touchingFingers =
            new Dictionary<SDK_BaseController.ButtonTypes, Vector2>();

        protected Dictionary<SDK_BaseController.ButtonTypes, MVR_FingerCollisionInfo> colliderInfos =
            new Dictionary<SDK_BaseController.ButtonTypes, MVR_FingerCollisionInfo>();

        protected List<SDK_BaseController.ButtonTypes> disabledFingers =
            new List<SDK_BaseController.ButtonTypes>();

        private bool debugTouch = false;

        protected override void Awake()
        {
            base.Awake();

            interactGrab = (interactGrab != null ? interactGrab : GetComponentInChildren<VRTK_MVRGrab>());
            collisionDetector = collisionDetector != null ? collisionDetector : GetComponentInChildren<MVR_CollisionDetector>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            colliderInfos.Clear();
            touchingFingers.Clear();

            collisionDetector = collisionDetector != null ? collisionDetector : GetComponent<MVR_CollisionDetector>();

            MVR_FingerCollisionInfo[] collisionInfos = customColliderContainer == null ?
                controllerCollisionDetector.GetComponentsInChildren<MVR_FingerCollisionInfo>() :
                customColliderContainer.GetComponentsInChildren<MVR_FingerCollisionInfo>();

            foreach (var info in collisionInfos)
            {
                VRTK_SharedMethods.AddDictionaryValue(colliderInfos, info.fingerIndex, info, false);
            }

            requiredFingers = MVR_HandData.SetFingerButton(RequiredFingers, requiredFingers);
        }

        /// <summary>
        /// Check if the given finger (button type) is touching the collider
        /// </summary>
        /// <param name="finger"></param>
        /// <param name="collider"></param>
        /// <returns></returns>
        public virtual bool IsFingerTouchingCollider(SDK_BaseController.ButtonTypes finger, Collider collider)
        {
            return collisionDetector != null && collisionDetector.IsFingerCollidingWithCollider(collider, finger);
        }

        /// <summary>
        /// Return the fingers that are touching with the given collider
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public virtual List<SDK_BaseController.ButtonTypes> FingersTouchingCollider(Collider collider)
        {
            return collisionDetector != null ? collisionDetector.FingersCollidingWithCollider(collider).Except(disabledFingers).ToList() : new List<SDK_BaseController.ButtonTypes>();

        }

        /// <summary>
        /// Return the fingers that are touching with the given gameobject
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public virtual List<SDK_BaseController.ButtonTypes> FingersTouchingGameObject(GameObject gameobject)
        {
            return collisionDetector != null ? collisionDetector.FingersCollidingWithGameobject(gameobject).Except(disabledFingers).ToList() : new List<SDK_BaseController.ButtonTypes>();
        }

        /// <summary>
        /// The amount of fingers that are touching a collider.
        /// </summary>
        /// <returns></returns>
        public virtual int NumberOfFingersTouching()
        {
            int amount = 0;
            foreach (var info in colliderInfos)
            {
                amount += info.Value.NumberOfCollidersInFinger;
            }
            return amount;
        }

        /// <summary>
        /// Is the given finger colliding with a gameobject.
        /// </summary>
        /// <param name="finger"></param>
        /// <returns></returns>
        public virtual bool IsFingerColliding(SDK_BaseController.ButtonTypes finger)
        {
            if (!colliderInfos.ContainsKey(finger))
            {
                return false;
            }

            return colliderInfos[finger].NumberOfCollidersInFinger > 0;
        }

        /// <summary>
        /// Check if the given list contains the required fingers
        /// </summary>
        /// <param name="fingersToCheck"></param>
        /// <returns></returns>
        public virtual bool ContainsRequiredFingers(List<SDK_BaseController.ButtonTypes> fingersToCheck)
        {
            foreach (var requiredFinger in requiredFingers)
            {
                if (!fingersToCheck.Contains(requiredFinger))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Is the given finger bent enough 
        /// </summary>
        /// <param name="finger">The finger to check for bending</param>
        /// <returns></returns>
        public virtual bool IsFingerBentEnough(SDK_BaseController.ButtonTypes finger)
        {
            Vector2 bends = VRTK_SDK_Bridge.GetControllerAxis(finger, controllerReference);
            // Check if the bend values of the finger are in range
            return bends.x >= MinMcpBend && bends.x <= MaxMcpBend && bends.y >= MinPipBend && bends.y <= MaxPipBend;
        }

        /// <summary>
        /// Check if the given fingers are bent enough, it will skip the thumb (touchpad) since it is not reliable enough.
        /// </summary>
        /// <param name="fingers">The fingers to check</param>
        /// <returns></returns>
        public virtual bool AreFingersBentEnough(List<SDK_BaseController.ButtonTypes> fingers)
        {
            foreach (var finger in fingers)
            {
                // Skip the thumb since it is not reliable enough for checking the bend
                if (finger == SDK_BaseController.ButtonTypes.Touchpad)
                    continue;

                if (IsFingerBentEnough(finger))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (interactGrab.IsGrabbing)
            {
                UpdateGrabbingFingers();
            }
        }

        protected virtual void UpdateFingersNotYetTouchingCollider(Collider collider)
        {
            foreach (var info in colliderInfos)
            {
                if (disabledFingers.Contains(info.Key) || TouchingFingers.ContainsKey(info.Key))
                    continue;

                if (IsFingerTouchingCollider(info.Key, collider) && IsFingerBentEnough(info.Key))
                {
                    Vector2 bends = VRTK_SDK_Bridge.GetControllerAxis(info.Key, controllerReference);
                    VRTK_SharedMethods.AddDictionaryValue(touchingFingers, info.Key, bends, false);
                }
            }
        }

        protected virtual void UpdateGrabbingFingers()
        {
            foreach (var info in colliderInfos)
            {
                if (!TouchingFingers.ContainsKey(info.Key) || disabledFingers.Contains(info.Key))
                    continue;

                Vector2 bends = VRTK_SDK_Bridge.GetControllerAxis(info.Key, controllerReference);
                Vector2 bendsOnTouch = touchingFingers[info.Key];

                if (bends.x <= Mathf.Clamp(useMultiplierForReleaseThreshold ? bendsOnTouch.x * releaseThresholdMultiplier : bendsOnTouch.x - releaseThresholdDelta, 0, 1)
                 && bends.y <= Mathf.Clamp(useMultiplierForReleaseThreshold ? bendsOnTouch.y * releaseThresholdMultiplier : bendsOnTouch.y - releaseThresholdDelta, 0, 1))
                {
                    if (temporarilyDisableFingersOnRelease)
                        StartCoroutine(IgnoreFingerForSeconds(info.Key, postReleaseFingerDisableTime));
                    touchingFingers.Remove(info.Key);
                }
            }
        }

        protected override void OnTriggerExit(Collider collider)
        {
            base.OnTriggerExit(collider);
            if (touchedObject == null)
            {
                // Clear the list of touching fingers
                touchingFingers.Clear();
            }
        }

        protected override void OnTriggerStay(Collider collider)
        {
            // Code taken from base class -----------------------
            GameObject colliderInteractableObject = TriggerStart(collider);

            if (touchedObject == null || collider.transform.IsChildOf(touchedObject.transform))
            {
                triggerIsColliding = true;
            }

            // Changed compared to base class: Update all of the touching fingers
            if (touchedObject != null && touchedObjectColliders.Contains(collider))
            {
                UpdateFingersNotYetTouchingCollider(collider);
            }

            // Changed compared to base class: Check if the required fingers are touching the object
            List<SDK_BaseController.ButtonTypes> touchingFingers = FingersTouchingGameObject(colliderInteractableObject);
            if (touchingFingers == null
                || touchingFingers.Count < 2
                || !ContainsRequiredFingers(touchingFingers)
                || !AreFingersBentEnough(touchingFingers)
                || (interactGrab != null
                && !interactGrab.IsUngrabbableObject(collider.gameObject)))
            {
                Log("Touch was refused since it did not follow all of the touching criteria.");
                return;
            }

            // End of changes

            if (touchedObject == null && colliderInteractableObject != null && IsObjectInteractable(collider.gameObject))
            {
                touchedObject = colliderInteractableObject;
                VRTK_InteractableObject touchedObjectScript = touchedObject.GetComponent<VRTK_InteractableObject>();

                // End of code taken from base class ----------------

                UpdateFingersNotYetTouchingCollider(collider);

                // Code taken from base class -----------------------
                //If this controller is not allowed to touch this interactable object then clean up touch and return before initiating a touch.
                if (touchedObjectScript != null && !touchedObjectScript.IsValidInteractableController(gameObject, touchedObjectScript.allowedTouchControllers))
                {
                    CleanupEndTouch();
                    return;
                }
                OnControllerStartTouchInteractableObject(SetControllerInteractEvent(touchedObject));
                StoreTouchedObjectColliders(collider);

                ToggleControllerVisibility(false);
                touchedObjectScript.StartTouching(this);

                OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedObject));
                // End of code taken from base class ----------------
            }
        }

        protected virtual IEnumerator IgnoreFingerForSeconds(SDK_BaseController.ButtonTypes finger, float seconds)
        {
            disabledFingers.Add(finger);
            yield return new WaitForSeconds(seconds);
            disabledFingers.Remove(finger);
        }

        protected void Log(string log)
        {
            if (debugTouch)
            {
                Debug.Log(log);
            }
        }
    }

}