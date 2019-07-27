using ManusVR.Core.Apollo;
using ManusVR.Core.Hands;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace ManusVR
{

    /// <summary>
    /// Provides a custom controller hand model with psuedo finger functionality.
    /// </summary>
    /// <remarks>
    /// **Prefab Usage:**
    ///  * Place the VRTK_Rightbasichand` prefab as a child of either the left or right script alias.
    ///  * If the prefab is being used in the left hand then check the `Mirror Model` parameter.
    ///  * By default, the avatar hand controller will detect which controller is connected and represent it accordingly.
    ///  * Optionally, use SDKTransformModify scripts to adjust the hand orientation based on different controller types.
    /// </remarks>
    public class VRTK_MVRAvatarHandController : VRTK_AvatarHandController
    {
        [Header("Thumb rotation settings")]
        [Tooltip("Add the thumb bone for the thumb rotation.")]
        public Transform thumbTransform;
        [Tooltip("Adds a pre-rotation to the thumb.")]
        public Vector3 preRotation;
        [Tooltip("Adds a post-rotation to the thumb.")]
        public Vector3 postRotation;
        [Header("Finger rotation settings")]
        [Tooltip("Freeze the movement of a finger when it touches the touched object that is provided by ManusVRTouch.")]
        public bool freezeFingersOnTouch = true;

        protected Vector2[] fingers = new Vector2[5];
        protected VRTK_MVRTouch manusTouch;
        protected VRTK_MVRGrab manusGrab;

        protected Quaternion lastThumbRotation = Quaternion.identity;

        protected VRTK_ControllerReference ControllerReference => VRTK_ControllerReference.GetControllerReference(controllerGameObject);

        protected GameObject controllerGameObject;

        private bool debugHand = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            manusTouch = (VRTK_MVRTouch)interactTouch;
            manusGrab = (VRTK_MVRGrab)interactGrab;

            SteamVR_ControllerManager manager = FindObjectOfType<SteamVR_ControllerManager>();
            controllerGameObject = manusTouch.gameObject;
        }

        protected override void Update()
        {
            base.Update();

            UpdateControllerAxis();
        }

        /// <summary>
        /// Check if the given arrayIndex is touching a object.
        /// </summary>
        /// <param name="arrayIndex">The arrayIndex that is used by the animation system to determine the finger position.</param>
        /// <returns></returns>
        protected virtual bool IsFingerTouchingObject(int arrayIndex)
        {
            // A list of fingers that are currently touching the object.
            Dictionary<SDK_BaseController.ButtonTypes, Vector2> touchingFingers = manusTouch.TouchingFingers;

            switch (arrayIndex)
            {
                case 0:
                    return touchingFingers.ContainsKey(SDK_BaseController.ButtonTypes.Touchpad);
                case 1:
                    return touchingFingers.ContainsKey(SDK_BaseController.ButtonTypes.Trigger);
                case 2:
                    return touchingFingers.ContainsKey(SDK_BaseController.ButtonTypes.MiddleFinger);
                case 3:
                    return touchingFingers.ContainsKey(SDK_BaseController.ButtonTypes.RingFinger);
                case 4:
                    return touchingFingers.ContainsKey(SDK_BaseController.ButtonTypes.PinkyFinger);
                default:
                    return true;
            }
        }

        protected override void SetFingerPosition(int arrayIndex, float axis)
        {
            //Code taken from base class VRTK_AvatarHandController
            //Changed compared to base class: Changed edit to animation layer instead of once twice for the 2 finger phalanges
            //Edited the + to 1 and 6 for the different animation layers
            float mcpAxis = fingers[arrayIndex].x;
            float pipAxis = fingers[arrayIndex].y;

            int mcpAnimationlayer = arrayIndex * 2 + 1;
            int pipAnimationlayer = arrayIndex * 2 + 2;

            if (ShouldFreezeFingerMovement(arrayIndex, mcpAxis, pipAxis, mcpAnimationlayer, pipAnimationlayer))
            {
                Log("Freezing arrayindex: " + arrayIndex);
                return;
            }

            animator.SetLayerWeight(mcpAnimationlayer, mcpAxis);
            animator.SetLayerWeight(pipAnimationlayer, pipAxis);

            if (overrideAxisValues[arrayIndex] == OverrideState.WasOverring)
                SetOverrideValue(arrayIndex, ref overrideAxisValues, OverrideState.NoOverride);
            //end of code from base class
        }

        protected virtual bool ShouldFreezeFingerMovement(int arrayIndex, float mcpAxis, float pipAxis, int mcpAnimationlayer, int pipAnimationlayer)
        {
            // Freeze the movement of the finger if it is touching the object.
            if (manusGrab.GetGrabbedObject() != null && freezeFingersOnTouch && IsFingerTouchingObject(arrayIndex))
            {
                // Only allow outwards movement when the finger is touching a object.
                if (mcpAxis > animator.GetLayerWeight(mcpAnimationlayer) ||
                    pipAxis > animator.GetLayerWeight(pipAnimationlayer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// saves the finger data from the controller reference in to a private array
        /// </summary>
        protected virtual void UpdateControllerAxis()
        {
            fingers[0] = VRTK_SDK_Bridge.GetControllerAxis(thumbAxisButton, ControllerReference);
            fingers[1] = VRTK_SDK_Bridge.GetControllerAxis(indexAxisButton, ControllerReference);
            fingers[2] = VRTK_SDK_Bridge.GetControllerAxis(middleAxisButton, ControllerReference);
            fingers[3] = VRTK_SDK_Bridge.GetControllerAxis(ringAxisButton, ControllerReference);
            fingers[4] = VRTK_SDK_Bridge.GetControllerAxis(pinkyAxisButton, ControllerReference);
        }

        protected virtual void LateUpdate()
        {
            RotateThumbMCPJoint();
        }

        /// <summary>
        /// Adds Thumb rotation quaternion to the Thumbtransform/
        /// </summary>
        protected virtual void RotateThumbMCPJoint()
        {
            if (!thumbTransform || !HandDataManager.IsPlayerNumberValid(SDK_ManusVRController.PlayerNumber))
            {
                return;
            }

            Quaternion thumbRotation = Quaternion.identity;

            int thumbFinger = (int)ApolloHandData.FingerName.Thumb;
            int CmcJoint = (int)ApolloHandData.JointNameThumb.CMC;

            // Freeze the thumb rotation when it is touching an object.
            if (freezeFingersOnTouch && IsFingerTouchingObject(0))
            {
                thumbRotation = lastThumbRotation;
            }
            else
            {
                switch (ControllerReference.hand)
                {
                    case SDK_BaseController.ControllerHand.Left:
                        if (HandDataManager.CanGetHandData(SDK_ManusVRController.PlayerNumber, device_type_t.GLOVE_LEFT))
                            thumbRotation = HandDataManager.GetHandData(SDK_ManusVRController.PlayerNumber, device_type_t.GLOVE_LEFT)
                                .fingers[thumbFinger].joints[CmcJoint];
                        break;
                    case SDK_BaseController.ControllerHand.Right:
                        if (HandDataManager.CanGetHandData(SDK_ManusVRController.PlayerNumber, device_type_t.GLOVE_RIGHT))
                            thumbRotation = HandDataManager.GetHandData(SDK_ManusVRController.PlayerNumber, device_type_t.GLOVE_RIGHT)
                                .fingers[thumbFinger].joints[CmcJoint];
                        break;
                    default:
                        thumbRotation = lastThumbRotation;
                        break;
                }
            }

            thumbTransform.localRotation = Quaternion.Euler(preRotation) * thumbRotation * Quaternion.Euler(postRotation);

            // Save the last thumb rotation.
            lastThumbRotation = thumbRotation;
        }

        protected virtual void Log(string log)
        {
            if (debugHand)
            {
                Debug.Log(log);
            }
        }
    }

}