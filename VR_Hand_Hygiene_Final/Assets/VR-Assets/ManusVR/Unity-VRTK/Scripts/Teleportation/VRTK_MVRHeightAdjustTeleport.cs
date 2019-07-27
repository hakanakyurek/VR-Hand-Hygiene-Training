using ManusVR.Core.Apollo;
using System.Collections;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

namespace ManusVR.Teleportation
{
    /// <summary>
    /// Updates the `x/z` position of the SDK Camera Rig with an optional screen fade.
    /// </summary>
    /// <remarks>
    ///   > The `y` position is not altered by the Basic Teleport so it only allows for movement across a 2D plane.
    ///
    /// **Script Usage:**
    ///  * Place the `VRTK_BasicTeleport` script on any active scene GameObject.
    ///
    /// **Script Dependencies:**
    ///  * An optional Destination Marker (such as a Pointer) to set the destination of the teleport location.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/004_CameraRig_BasicTeleport` uses the `VRTK_Pointer` script on the Controllers to initiate a laser pointer by pressing the `Touchpad` on the controller and when the laser pointer is deactivated (release the `Touchpad`) then the user is teleported to the location of the laser pointer tip as this is where the pointer destination marker position is set to.
    /// </example>
    [AddComponentMenu("ManusVR/Unity-VRTK/Scripts/VRTK_MVRHeightAdjustTeleport")]
    public class VRTK_MVRHeightAdjustTeleport : VRTK_HeightAdjustTeleport
    {
        [Tooltip("Reference to the pointer object on the left hand.")]
        public VRTK_MVRPointer leftVRTKPointer;
        [Tooltip("Reference to the pointer object on the right hand.")]
        public VRTK_MVRPointer rightVRTKPointer;

        [Tooltip("Which hand should be used to initiate and aim the teleportation interaction?")]
        public GloveLaterality handToUse = GloveLaterality.GLOVE_LEFT;
        [Tooltip("The prefab that detects teleportation")]
        public GameObject teleportDetectorPrefab;

        protected MVR_TeleportDetector teleportDetector;
        protected VRTK_MVRPointer pointerToUse;
        protected Coroutine detectorTouchCompleteCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();

            var teleportObject = Instantiate(teleportDetectorPrefab);
            teleportDetector = teleportObject.GetComponent<MVR_TeleportDetector>();
            if (teleportDetector == null)
            {
                teleportDetector = teleportObject.AddComponent<MVR_TeleportDetector>();
            }
            if (teleportDetector == null)
            {
                enabled = false;
                return;
            }

            teleportDetector.detectorGloveLaterality = handToUse;

            pointerToUse = handToUse == GloveLaterality.GLOVE_LEFT
                ? leftVRTKPointer
                : rightVRTKPointer;

            teleportDetector.onActivatePointer += () => pointerToUse?.Toggle(true);
            teleportDetector.onDeactivatePointer += () => pointerToUse?.Toggle(false);
            teleportDetector.onInitiateTeleport += OnInitiateTeleport;
        }

        protected virtual void OnInitiateTeleport()
        {
            if (detectorTouchCompleteCoroutine == null)
            {
                detectorTouchCompleteCoroutine = StartCoroutine(DetectorTouchComplete());
            }
        }

        protected virtual IEnumerator DetectorTouchComplete()
        {
            teleportDetector.gameObject.SetActive(false);

            var hands = FindObjectsOfType<VRTK_MVRPhysicsAvatarHandController>();

            //Cache previous hand move distance
            var maxDistanceDeltaWhenKinematic = hands[0].MaxDistanceDeltaWhenKinematic;

            //Set the hand move distance to an absurdly high amount to make the hands teleport with the player
            hands.ForEach(hand => hand.MaxDistanceDeltaWhenKinematic = 1000);

            pointerToUse.ExecuteDestinationDecided();

            yield return new WaitForSeconds(0.01f);

            //Set the hand move distance back to the cached value
            hands.ForEach(hand => hand.MaxDistanceDeltaWhenKinematic = maxDistanceDeltaWhenKinematic);

            teleportDetector.gameObject.SetActive(true);

            detectorTouchCompleteCoroutine = null;
        }
    }
}