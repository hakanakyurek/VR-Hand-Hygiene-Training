using System.Collections.Generic;
using VRTK;

namespace ManusVR
{

    /// <summary>
    /// static class that deal with changing fingertypes to the right button type
    /// </summary>
    public static class MVR_HandData
    {
        /// <summary>
        /// Custom Fingertypes that we change into SDK_BaseController.ButtonTypes
        /// </summary>
        public enum FingerTypes
        {
            Thumb,
            IndexFinger,
            MiddleFinger,
            RingFinger,
            Pinky,
            //MANUS_TODO: remove palm
            Palm
        }

        /// <summary>
        /// Changes fingertypes list to the SDK button type
        /// </summary>
        /// <param name="convertFingers">FingerTypes list</param>
        /// <param name="targetFingers">SDK_BaseController.ButtonTypes list</param>
        /// <returns>List of SDK_Basecontroller.ButtonTypes element</returns>
        public static List<SDK_BaseController.ButtonTypes> SetFingerButton(List<FingerTypes> convertFingers, List<SDK_BaseController.ButtonTypes> targetFingers)
        {
            targetFingers.Clear();
            for (int i = 0; i < convertFingers.Count; i++)
            {
                targetFingers.Add(new SDK_BaseController.ButtonTypes());
                switch (convertFingers[i])
                {
                    case FingerTypes.Thumb:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.Touchpad;
                        break;
                    case FingerTypes.IndexFinger:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.Trigger;
                        break;
                    case FingerTypes.MiddleFinger:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.MiddleFinger;
                        break;
                    case FingerTypes.RingFinger:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.RingFinger;
                        break;
                    case FingerTypes.Pinky:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.PinkyFinger;
                        break;
                    case FingerTypes.Palm:
                        targetFingers[i] = SDK_BaseController.ButtonTypes.StartMenu;
                        break;
                }
            }
            return targetFingers;
        }
        /// <summary>
        /// Changes FingerTypes element to SDK_BaseController.ButtonTypes element
        /// </summary>
        /// <param name="convertFinger">FingerTypes element</param>
        /// <param name="targetFinger">SDK_BaseController element</param>
        /// <returns>SDK_BaseController.ButtonTypes element</returns>
        public static SDK_BaseController.ButtonTypes SetFingerButton(FingerTypes convertFinger, SDK_BaseController.ButtonTypes targetFinger)
        {
            switch (convertFinger)
            {
                case FingerTypes.Thumb:
                    targetFinger = SDK_BaseController.ButtonTypes.Touchpad;
                    break;
                case FingerTypes.IndexFinger:
                    targetFinger = SDK_BaseController.ButtonTypes.Trigger;
                    break;
                case FingerTypes.MiddleFinger:
                    targetFinger = SDK_BaseController.ButtonTypes.MiddleFinger;
                    break;
                case FingerTypes.RingFinger:
                    targetFinger = SDK_BaseController.ButtonTypes.RingFinger;
                    break;
                case FingerTypes.Pinky:
                    targetFinger = SDK_BaseController.ButtonTypes.PinkyFinger;
                    break;
                case FingerTypes.Palm:
                    targetFinger = SDK_BaseController.ButtonTypes.StartMenu;
                    break;
            }
            return targetFinger;
        }
    }

}
