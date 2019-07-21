using ManusVR.Core.Apollo;
using ManusVR.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace ManusVR.Teleportation
{
    public class MVR_TeleportDetector : MonoBehaviour
    {
        [Tooltip("Which hand should be used to start and aim the teleport procedure.")]
        public GloveLaterality detectorGloveLaterality = GloveLaterality.GLOVE_LEFT;

        public Action onActivatePointer;
        public Action onDeactivatePointer;
        public Action onInitiateTeleport;

        [Tooltip("The positional offset at which the detector object should be positioned from the tracker.")]
        public Vector3 transformFollowPositionOffset;
        [Tooltip("The rotational offset at which the detector object should be positioned from the tracker.")]
        public Vector3 transformFollowRotationOffset;

        [Tooltip("The amount of seconds the user must touch the detector object to start the teleport procedure!")]
        public float secondsToActivate = 1;

        protected List<MVR_PhalangeCollisionInfo> phalanges = new List<MVR_PhalangeCollisionInfo>();
        protected Vector2[] fingerData = new Vector2[5];

        protected bool canTryTeleport = true;
        protected bool handOpened = false;

        protected GameObject controllerGameObject;
        protected Renderer renderer;

        protected Coroutine touchCoroutine;
        protected Coroutine decideDestinationCoroutine;

        protected bool ControllerPointingDownwards => controllerGameObject?.transform.up.y < -0.7f && controllerGameObject?.transform.up.y > -1.0f;
        protected VRTK_ControllerReference ControllerReference => VRTK_ControllerReference.GetControllerReference(controllerGameObject);

        protected virtual void OnEnable()
        {
            renderer = GetComponent<Renderer>();

            StartCoroutine(Initialize());
        }

        //Wait until SteamVR references are cached
        protected virtual IEnumerator Initialize()
        {
            yield return null;
            SteamVR_ControllerManager manager = FindObjectOfType<SteamVR_ControllerManager>();
            controllerGameObject = detectorGloveLaterality == GloveLaterality.GLOVE_LEFT ? manager.left : manager.right;

            var transformFollow = gameObject.AddComponent<TransformFollow>();
            transformFollow.transformToMove = transform;
            transformFollow.transformToFollow = controllerGameObject.transform;
            transformFollow.positionOffset = transformFollowPositionOffset;
            transformFollow.rotationOffset = transformFollowRotationOffset;
        }

        protected virtual void Update()
        {
            var prevHandOpened = handOpened;

            UpdateControllerAxes();
            UpdateHandOpened();
            CheckSingleHandTeleport();

            if (prevHandOpened != handOpened)
            {
                DetectorObjectTouchStop();
            }
        }

        /// <summary>
        /// saves the finger data from the controller reference in to a private array
        /// </summary>
        protected virtual void UpdateControllerAxes()
        {
            fingerData[0] = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Touchpad, ControllerReference);
            fingerData[1] = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Trigger, ControllerReference);
            fingerData[2] = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.MiddleFinger, ControllerReference);
            fingerData[3] = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.RingFinger, ControllerReference);
            fingerData[4] = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.PinkyFinger, ControllerReference);
        }

        protected virtual void UpdateHandOpened()
        {
            handOpened = false;
            for (var i = 1; i < 5; i++)
            {
                if (!(fingerData[i].x < 0.4f) && !(fingerData[i].y < 0.4f)) continue;

                handOpened = true;
                break;
            }
        }

        protected virtual void CheckSingleHandTeleport()
        {
            if (!canTryTeleport || !ControllerPointingDownwards || !handOpened) return;

            if (decideDestinationCoroutine == null)
                decideDestinationCoroutine = StartCoroutine(DecideDestination(true));
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var touchingPhalange = other.GetComponent<MVR_PhalangeCollisionInfo>();
            if (touchingPhalange == null) return;

            phalanges.Add(touchingPhalange);
            if (phalanges.Count == 1)
            {
                DetectorObjectTouchStart();
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var touchingPhalange = other.GetComponent<MVR_PhalangeCollisionInfo>();
            if (touchingPhalange == null) return;

            phalanges.Remove(touchingPhalange);

            if (phalanges.Count == 0)
            {
                DetectorObjectTouchStop();
            }
        }

        protected virtual void DetectorObjectTouchStart()
        {
            if (touchCoroutine == null)
            {
                touchCoroutine = StartCoroutine(DetectorObjectTouchProcedure());
            }
        }

        protected virtual void DetectorObjectTouchStop()
        {
            renderer.enabled = handOpened;
            GetComponent<Collider>().enabled = handOpened;
            phalanges = new List<MVR_PhalangeCollisionInfo>();

            if (touchCoroutine == null) return;

            StopCoroutine(touchCoroutine);
            touchCoroutine = null;
            SetAlpha(1);
        }

        protected virtual void DetectorObjectTouchComplete()
        {
            if (decideDestinationCoroutine == null)
                decideDestinationCoroutine = StartCoroutine(DecideDestination(false));
        }

        protected virtual IEnumerator DetectorObjectTouchProcedure()
        {
            var startTime = Time.time;
            var lerp = 0f;

            while (lerp < 1f)
            {
                SetAlpha(1 - lerp);

                lerp = Mathf.InverseLerp(startTime, startTime + secondsToActivate, Time.time);
                yield return null;
            }
            DetectorObjectTouchComplete();
        }

        protected virtual IEnumerator DecideDestination(bool singleHanded)
        {
            canTryTeleport = false;
            onActivatePointer?.Invoke();
            while (handOpened)
            {
                if (singleHanded)
                {
                    if (!ControllerPointingDownwards)
                    {
                        StopPointingProcedure();
                        yield break;
                    }
                }
                else
                {
                    if (phalanges.Count == 0)
                    {
                        StopPointingProcedure();
                        yield break;
                    }
                }

                yield return null;
            }
            InitiateTeleport();
            StopPointingProcedure();
        }

        protected virtual void StopPointingProcedure()
        {
            decideDestinationCoroutine = null;
            onDeactivatePointer?.Invoke();
            canTryTeleport = true;
        }

        protected virtual void InitiateTeleport()
        {
            onInitiateTeleport?.Invoke();
            canTryTeleport = true;
        }

        protected virtual void SetAlpha(float alpha)
        {
            var color = renderer.material.GetColor("_Color");
            color.a = alpha;
            renderer.material.SetColor("_Color", color);
        }
    }
}
