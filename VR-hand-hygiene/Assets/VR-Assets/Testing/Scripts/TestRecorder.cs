// Copyright (c) 2018 ManusVR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Assets.ManusVR.Scripts;
using Assets.Testing.Scripts;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Testing.Scripts
{
    /// <summary>
    ///     Record input of the hands and arms with this script
    /// </summary>
    public class TestRecorder : MonoBehaviour
    {
        private const int MaxRecordingFrames = 10000;
        private const int VersionNumber = 2;
        private HandData _handData;

        private ManusRecording _manusRecording;
        public bool IsRecording { get; private set; }
        private TrackingManager _trackingManager;


        public KeyCode RecordButton = KeyCode.T;
        public string OptionalFilename;
        [Range(0, 10)]
        public float DelayBeforeStart = 0f;

        public string OptionalWritePath;

        public float TimeStartRecording = 0;

        // Use this for initialization
        private void Start()
        {
            IsRecording = false;
            _trackingManager = GetComponent<TrackingManager>();
            if (_trackingManager == null)
                _trackingManager = Component.FindObjectOfType<TrackingManager>();
            _handData = GetComponent<HandData>();
            if (_handData == null)
                _handData = Component.FindObjectOfType<HandData>();

            if (!_trackingManager || !_handData)
                Debug.LogError("Not all references are set on the recorder");
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(RecordButton))
            {
                if (IsRecording)
                    StopRecording();
                else
                    StartRecording();
            }
        }

        public void StopRecording()
        {
            if (!IsRecording)
                return;
            IsRecording = false;
        }

        public void StartRecording()
        {
            if (IsRecording) return;
            _manusRecording = new ManusRecording();

            //Save all context data in the recorder
            _manusRecording.HandYawOffsets.Add(_handData.TrackingValues.HandYawOffset[device_type_t.GLOVE_LEFT]);
            _manusRecording.HandYawOffsets.Add(_handData.TrackingValues.HandYawOffset[device_type_t.GLOVE_RIGHT]);
            _manusRecording.VersionNumber = VersionNumber;


            StartCoroutine(RecordingLoop());
        }

        /// <summary>
        ///     Start recording the input
        /// </summary>
        /// <returns></returns>
        private IEnumerator RecordingLoop()
        {
            Debug.Log("Delay started waiting for " + DelayBeforeStart);
            yield return new WaitForSeconds(DelayBeforeStart);
            IsRecording = true;
            Debug.Log("Recorder has started recording");

            List<RecordingDataPacket> packets = new List<RecordingDataPacket>();

            int frames = 0;

            //Cache at what frame the recording started
            int frameIndexStart = Time.frameCount;

            //Loop while recorder is set to record and the maximum preset amount of frames is not exceeded
            while (IsRecording && frames <= MaxRecordingFrames)
            {
                //Package all relevant data into a single packet
                RecordingDataPacket packet = new RecordingDataPacket
                {
                    //Save current frame index
                    FrameIndex = Time.frameCount - frameIndexStart,

                    //Save joint- and raw data from the HandData instance
                    JointData = new JointDataNode
                    {
                        JointDataLeft = _handData.GetHandData(GloveLaterality.GLOVE_LEFT),
                        JointDataRight = _handData.GetHandData(GloveLaterality.GLOVE_RIGHT),
                        RawDataLeft = _handData.GetRawHandData(GloveLaterality.GLOVE_LEFT),
                        RawDataRight = _handData.GetRawHandData(GloveLaterality.GLOVE_RIGHT)
                    },

                    //Save all tracker and HMD data from the TrackingManager
                    TrackingData = new TrackingDataNode
                    {
                        LeftTrackerPosition = _trackingManager.leftTracker.localPosition,
                        RightTrackerPosition = _trackingManager.rightTracker.localPosition,
                        LeftTrackerRotation = _trackingManager.leftTracker.localRotation,
                        RightTrackerRotation = _trackingManager.rightTracker.localRotation,
                        HMDPosition = _trackingManager.HMD.localPosition,
                        HMDRotation = _trackingManager.HMD.localRotation
                    }
                };

                //Since Unity doesn't support serialization of arrays, the flex data array in raw data is not serialized. 
                //We manually serialize this here in a struct and save it seperately in the recorded packet.
                packet.JointData.FlexDataLeft = new JointDataNode.ByteArrayWrapper { Array = packet.JointData.RawDataLeft.flexRaw };
                packet.JointData.FlexDataRight = new JointDataNode.ByteArrayWrapper { Array = packet.JointData.RawDataRight.flexRaw };

                packets.Add(packet);
                yield return new WaitForFixedUpdate();
                frames++;
            }
            if (frames > MaxRecordingFrames)
            {
                Debug.Log("IsRecording reached the limit of " + MaxRecordingFrames + " frames");
                IsRecording = false;
            }
            else if (!IsRecording)
            {
                _manusRecording.Data = packets;
                SaveGameData(_manusRecording);
            }
        }

        /// <summary>
        ///     Save the recorded data to a json file
        /// </summary>
        /// <param name="data"></param>
        private string SaveGameData(ManusRecording data)
        {
            var dataAsJson = JsonUtility.ToJson(data);

            string fileName;
            if (OptionalFilename.Length == 0)
                fileName = CurrentDateTime();
            else
            {
                fileName = OptionalFilename;
                fileName += ".json";
            }

            string filePath;
            if (OptionalWritePath.Length == 0)
                filePath = Application.dataPath + "/Testing/Recordings/";
            else
            {
                filePath = OptionalWritePath;
            }

            filePath += fileName;
            try
            {
                File.WriteAllText(filePath, dataAsJson);
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.Log(e);
                return "";
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            Debug.Log("Succesfully saved recording to: " + filePath);
            return filePath;
        }

        private string CurrentDateTime()
        {
            return DateTime.Now.Date.Year + "-" +
                   DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".json";
        }
    }
}