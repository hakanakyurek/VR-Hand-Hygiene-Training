// Copyright (c) 2018 ManusVR
using System.Collections;
using Assets.ManusVR.Scripts;
using UnityEngine;

namespace Assets.Testing.Scripts
{
    /// <summary>
    /// Play the recorded data with this script
    /// </summary>
    [RequireComponent(typeof(HandData))]
    public class TestPlayer : MonoBehaviour
    {
        public TextAsset RecordedSource;
        public bool PlayOnStart = true;
        public bool PlayGloveData = true;
        public bool PlayArmPositionData = true;
        public bool PlayArmRotationData = true;
        private HandData _handData;
        private TrackingManager _trackingManager;

        // Use this for initialization
        IEnumerator Start()
        {
            _handData = GetComponent<HandData>();
            if (_handData == null)
                _handData = Component.FindObjectOfType<HandData>();
            _trackingManager = GetComponent<TrackingManager>();
            if (_trackingManager == null)
                _trackingManager = Component.FindObjectOfType<TrackingManager>();

            if (RecordedSource == null)
            {
                Debug.Log("TestPlayer needs a source");
                yield break;
            }
            // Wait a frame because some scripts still have to do stuff before they get disabled when using the inputdata
            yield return new WaitForEndOfFrame();

            if (!PlayOnStart) yield break;
            StartCoroutine(PlayRecording(RecordedSource.text));
        }

        public IEnumerator PlayRecording(string jsonSource)
        {
            StopRecording();
            var manusRecording = LoadGameData(jsonSource);

            //Set all context data in HandData and TrackingManager from the recording 
            _handData.TrackingValues.HandYawOffset[device_type_t.GLOVE_LEFT] = manusRecording.HandYawOffsets[0];
            _handData.TrackingValues.HandYawOffset[device_type_t.GLOVE_RIGHT] = manusRecording.HandYawOffsets[1];

            //Temporarily disable Apollo and SteamVR input
            _handData.SetApolloInputEnabled(false);
            _trackingManager.SetInputEnabled(false);

            WaitForFixedUpdate wfu = new WaitForFixedUpdate();

            //Loop through all data packets from the recording and pass the data to the HandData and TrackingManager as input
            int frameIndexStart = Time.frameCount;
            for (int i = 0; i < manusRecording.Data.Count; i++)
            {
                RecordingDataPacket packet = manusRecording.Data[i];
                if(packet.FrameIndex < Time.frameCount - frameIndexStart)
                    continue;

                _handData.ManualJointInput(packet.JointData);
                _trackingManager.ManualInput(packet.TrackingData, PlayArmRotationData);
                yield return wfu;
            }

            //Reenable Apollo and SteamVR input
            _handData.SetApolloInputEnabled(true);
            _trackingManager.SetInputEnabled(true);
        }

        public void StopRecording()
        {
            _handData.SetApolloInputEnabled(true);
        }

        private ManusRecording LoadGameData(string jsonSource)
        {
            return JsonUtility.FromJson<ManusRecording>(jsonSource);
        }
    }
}
