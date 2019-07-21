using UnityEditor;
using UnityEngine;

namespace ManusVR.VRTK
{
    /// <summary>
    /// Creates a custom inspector for VRTK_ManusVRGrab with custom configuration
    /// </summary>
    [CustomEditor(typeof(VRTK_MVRGrab)), CanEditMultipleObjects]
    public class VRTK_MVRGrabEditor : Editor
    {
        protected SerializedProperty throwMultiplier;
        protected SerializedProperty createRigidBodyWhenNotTouching;
        protected SerializedProperty controllerAttachPoint;
        protected SerializedProperty controllerEvents;
        protected SerializedProperty interactTouch;
        protected SerializedProperty collisionDetector;
        protected SerializedProperty collisionManager;
        protected MonoScript scriptReference;

        /// <summary>
        /// Initialisation of the serialized properties
        /// </summary>
        protected void OnEnable()
        {
            throwMultiplier = serializedObject.FindProperty("throwMultiplier");
            createRigidBodyWhenNotTouching = serializedObject.FindProperty("createRigidBodyWhenNotTouching");
            controllerAttachPoint = serializedObject.FindProperty("controllerAttachPoint");
            controllerEvents = serializedObject.FindProperty("controllerEvents");
            interactTouch = serializedObject.FindProperty("interactTouch");
            collisionDetector = serializedObject.FindProperty("collisionDetector");
            collisionManager = serializedObject.FindProperty("collisionManager");
            scriptReference = MonoScript.FromMonoBehaviour((VRTK_MVRGrab)target);
        }
        /// <summary>
        /// Draws the serialized properties on the Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //We show the script referce and disable the GUI temporarily to lock it in place
            GUI.enabled = false;
            scriptReference = EditorGUILayout.ObjectField("Script", scriptReference, typeof(MonoScript), false) as MonoScript;
            GUI.enabled = true;
            //We enable the settings in the inspector
            EditorGUILayout.PropertyField(throwMultiplier);
            EditorGUILayout.PropertyField(createRigidBodyWhenNotTouching);
            EditorGUILayout.PropertyField(controllerAttachPoint);
            EditorGUILayout.PropertyField(controllerEvents);
            EditorGUILayout.PropertyField(interactTouch);
            EditorGUILayout.PropertyField(collisionDetector);
            EditorGUILayout.PropertyField(collisionManager);
            //Enables ability to apply custom properties
            serializedObject.ApplyModifiedProperties();
        }
    }
}