using ManusVR.Core.Apollo;
using ManusVR.Core.Hands;

namespace VRTK
{
    using UnityEngine;
    using Valve.VR;

    /// <summary>
    /// The ManusVRController provides functionality to use input from ManusVR gloves
    /// </summary>
    [SDK_Description(typeof(SDK_ManusVRSystem))]
    public class SDK_ManusVRController : SDK_SteamVRController
    {
        /// <summary>
        /// The amount of milliseconds the rumble motor will be enabled for every time HapticPulse is called during haptic feedback.
        /// A duration of 100 milliseconds works well to prevent interruptions in the rumble with the default interval of VRTK_InteractHaptics,
        /// and it minimises how much it extends the rumble duration.
        /// In situations with severe packet loss a higher duration may work better.
        /// </summary>
        public ushort hapticPulseDuration = 100;
        /// <summary>
        /// If Apollo's pinch pose detection should be enabled. This can be used with Apollo version 2019.2 and up.
        /// </summary>
        public bool usePinchFilter = false;

        /// <summary>
        /// The SteamVR tracked device index that was found for the left hand.
        /// </summary>
        protected uint cachedLeftIndex = uint.MaxValue;

        /// <summary>
        /// The SteamVR tracked device index that was found for the right hand.
        /// </summary>
        protected uint cachedRightIndex = uint.MaxValue;

        /// <summary>
        /// The player number that hand data will be used of.
        /// </summary>
        protected static int playerNumber = HandDataManager.invalidPlayerNumber;
        public static int PlayerNumber => playerNumber;

        protected bool debugController = false;

        protected bool activatedLeftTrackedObject = false;

        protected bool activatedRightTrackedObject = false;

        ~SDK_ManusVRController()
        {
            if (HandDataManager.IsPlayerNumberValid(playerNumber))
            {
                Log("Releasing the playerNumber.");
                HandDataManager.ReleasePlayerNumber(playerNumber);
            }
        }

        public static void OverridePlayerNumber(int pNumber)
        {
            playerNumber = pNumber;
        }

        public override void OnAfterSetupLoad(VRTK_SDKSetup setup)
        {
            base.OnAfterSetupLoad(setup);
            // Initialise the HandDataManager so that hand data will be available.
            HandDataManager.Initialise(HandDataManager.DataStreamType.Apollo);
        }

        /// <summary>
        /// The GetCurrentControllerType method returns the current used ControllerType based on the SDK and headset being used.
        /// </summary>
        /// <param name="controllerReference">The reference to the controller to get type of.</param>
        /// <returns>The ControllerType based on the SDK and headset being used.</returns>
        public override ControllerType GetCurrentControllerType(VRTK_ControllerReference controllerReference = null)
        {
            return ControllerType.Custom;
        }

        /// <summary>
        /// The GetControllerElementPath returns the path to the game object that the given controller element for the given hand resides in.
        /// </summary>
        /// <param name="element">The controller element to look up.</param>
        /// <param name="hand">The controller hand to look up.</param>
        /// <param name="fullPath">Whether to get the initial path or the full path to the element.</param>
        /// <returns>A string containing the path to the game object that the controller element resides in.</returns>
        public override string GetControllerElementPath(ControllerElements element, ControllerHand hand, bool fullPath = false)
        {
            return "";
        }

        /// <summary>
        /// The HapticPulse/2 method is used to initiate a simple haptic pulse on the tracked object of the given controller reference.
        /// </summary>
        /// <param name="controllerReference">The reference to the tracked object to initiate the haptic pulse on.</param>
        /// <param name="strength">The intensity of the rumble of the controller motor. `0` to `1`.</param>
        public override void HapticPulse(VRTK_ControllerReference controllerReference, float strength = 0.5F)
        {
            // Get the laterality of the controller.
            // Note: no support for specific device IDs yet. It uses the first device found of the given laterality.
            ControllerHand lateralityVRTK = controllerReference.hand;
            GloveLaterality lateralityApollo = GloveLaterality.UNKNOWN;

            switch (lateralityVRTK)
            {
                case ControllerHand.None:
                    Debug.LogError("Attempted to use rumble, but the device's laterality is set to \"None\".");
                    return;

                case ControllerHand.Left:
                    lateralityApollo = GloveLaterality.GLOVE_LEFT;
                    break;

                case ControllerHand.Right:
                    lateralityApollo = GloveLaterality.GLOVE_RIGHT;
                    break;

                default:
                    Debug.LogError("Attempted to use rumble, but the device's laterality is set to an unrecognised value. Its value was " + lateralityVRTK + ".");
                    return;
            }

            // Use Apollo to tell the glove to rumble.
            const float minStrength = 0.4f;
            if (strength < minStrength)
            {
                // Any less than minStrength will not get the motor started, so use at least minStrength.
                strength = minStrength;
            }

            ushort strengthConverted = (ushort)(strength * (float)ushort.MaxValue);

            Apollo.rumble(lateralityApollo, hapticPulseDuration, strengthConverted);
        }

        /// <summary>
        /// The GetButtonAxis method retrieves the current X/Y axis values for the given button type on the given controller reference.
        /// </summary>
        /// <param name="buttonType">The type of button to check for the axis on.</param>
        /// <param name="controllerReference">The reference to the controller to check the button axis on.</param>
        /// <returns>A Vector2 of the X/Y values of the button axis. If no axis values exist for the given button, then a Vector2.Zero is returned.</returns>
        public override Vector2 GetButtonAxis(ButtonTypes buttonType, VRTK_ControllerReference controllerReference)
        {
            device_type_t deviceType = GetDeviceType(controllerReference);

            bool hasValidPlayerNumber = HandDataManager.IsPlayerNumberValid(playerNumber) || HandDataManager.GetPlayerNumber(out playerNumber);
            bool canGetData = HandDataManager.CanGetHandData(playerNumber, deviceType);

            if (!hasValidPlayerNumber || !canGetData)
            {
                return Vector2.zero;
            }

            int thumb = (int)ApolloHandData.FingerName.Thumb;
            int index = (int)ApolloHandData.FingerName.Index;
            int middle = (int)ApolloHandData.FingerName.Middle;
            int ring = (int)ApolloHandData.FingerName.Ring;
            int pinky = (int)ApolloHandData.FingerName.Pinky;

            int proximal = (int)ApolloHandData.FlexSensorSegment.Proximal;
            int medial = (int)ApolloHandData.FlexSensorSegment.Medial;

            ApolloHandData handData = HandDataManager.GetHandData(playerNumber, deviceType);

            if (handData != null)
            {
                switch (buttonType)
                {
                    case ButtonTypes.Touchpad:
                        return new Vector2((float)handData.fingers[thumb].flexSensorRaw[proximal], (float)handData.fingers[thumb].flexSensorRaw[medial]);
                    case ButtonTypes.Trigger:
                        return new Vector2((float)handData.fingers[index].flexSensorRaw[proximal], (float)handData.fingers[index].flexSensorRaw[medial]);
                    case ButtonTypes.MiddleFinger:
                        return new Vector2((float)handData.fingers[middle].flexSensorRaw[proximal], (float)handData.fingers[middle].flexSensorRaw[medial]);
                    case ButtonTypes.RingFinger:
                        return new Vector2((float)handData.fingers[ring].flexSensorRaw[proximal], (float)handData.fingers[ring].flexSensorRaw[medial]);
                    case ButtonTypes.PinkyFinger:
                        return new Vector2((float)handData.fingers[pinky].flexSensorRaw[proximal], (float)handData.fingers[pinky].flexSensorRaw[medial]);

                        // Note: other button types are currently not supported. The default value will always be returned for them.
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// The GetButtonSenseAxis method retrieves the current sense axis value for the given button type on the given controller reference.
        /// </summary>
        /// <param name="buttonType">The type of button to check for the sense axis on.</param>
        /// <param name="controllerReference">The reference to the controller to check the sense axis on.</param>
        /// <returns>The current sense axis value.</returns>
        public override float GetButtonSenseAxis(ButtonTypes buttonType, VRTK_ControllerReference controllerReference)
        {
            device_type_t deviceType = GetDeviceType(controllerReference);

            bool hasValidPlayerNumber = HandDataManager.IsPlayerNumberValid(playerNumber) || HandDataManager.GetPlayerNumber(out playerNumber);
            bool canGetData = HandDataManager.CanGetHandData(playerNumber, deviceType);

            if (!hasValidPlayerNumber || !canGetData)
            {
                return 0.0f;
            }

            int thumb = (int)ApolloHandData.FingerName.Thumb;
            int index = (int)ApolloHandData.FingerName.Index;
            int middle = (int)ApolloHandData.FingerName.Middle;
            int ring = (int)ApolloHandData.FingerName.Ring;
            int pinky = (int)ApolloHandData.FingerName.Pinky;

            int proximal = (int)ApolloHandData.FlexSensorSegment.Proximal;

            ApolloHandData handData = HandDataManager.GetHandData(playerNumber, deviceType);

            if (handData != null)
            {
                switch (buttonType)
                {
                    case ButtonTypes.Touchpad:
                        return (float)handData.fingers[thumb].flexSensorRaw[proximal];
                    case ButtonTypes.Trigger:
                        return (float)handData.fingers[index].flexSensorRaw[proximal];
                    case ButtonTypes.MiddleFinger:
                        return (float)handData.fingers[middle].flexSensorRaw[proximal];
                    case ButtonTypes.RingFinger:
                        return (float)handData.fingers[ring].flexSensorRaw[proximal];
                    case ButtonTypes.PinkyFinger:
                        return (float)handData.fingers[pinky].flexSensorRaw[proximal];

                        // Note: other button types are currently not supported. The default value will always be returned for them.
                }
            }

            return 0.0f;
        }

        /// <summary>
        /// The GetControllerButtonState method is used to determine if the given controller button for the given press type on the given controller reference is currently taking place.
        /// </summary>
        /// <param name="buttonType">The type of button to check for the state of.</param>
        /// <param name="pressType">The button state to check for.</param>
        /// <param name="controllerReference">The reference to the controller to check the button state on.</param>
        /// <returns>Returns true if the given button is in the state of the given press type on the given controller reference.</returns>
        public override bool GetControllerButtonState(ButtonTypes buttonType, ButtonPressTypes pressType, VRTK_ControllerReference controllerReference)
        {
            uint index = VRTK_ControllerReference.GetRealIndex(controllerReference);
            if (index >= OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                return false;
            }

            switch (buttonType)
            {
                case ButtonTypes.Trigger:
                    Vector2 buttonAxis = GetButtonAxis(ButtonTypes.Trigger, controllerReference);
                    switch (pressType)
                    {
                        case ButtonPressTypes.PressDown:
                            return buttonAxis.y >= 0.5f;
                        case ButtonPressTypes.PressUp:
                            return buttonAxis.y < 0.5f;
                        case ButtonPressTypes.Press:
                            return buttonAxis.y > 0.2f;
                    }
                    break;
            }
            return false;
        }

        protected device_type_t GetDeviceType(VRTK_ControllerReference controllerReference)
        {
            device_type_t deviceType = device_type_t.GLOVE_RIGHT;
            switch (controllerReference.hand)
            {
                case ControllerHand.Left:
                    deviceType = device_type_t.GLOVE_LEFT;
                    break;
                case ControllerHand.Right:
                    deviceType = device_type_t.GLOVE_RIGHT;
                    break;
            }
            return deviceType;
        }

        protected virtual void Log(string log)
        {
            if (debugController)
                Debug.Log(log);
        }
    }
}