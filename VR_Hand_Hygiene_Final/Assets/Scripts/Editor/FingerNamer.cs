using UnityEngine;
using UnityEditor;
using System.Collections;

class FingerNamer : EditorWindow
{
    private Transform hand;
    private string hand_type;
    [MenuItem("Window/HandManager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FingerNamer));
    }

    void OnGUI()
    {

        hand = (Transform)EditorGUILayout.ObjectField(hand, typeof(Transform),true);



        hand_type = GUILayout.TextField(hand_type);


        if(GUILayout.Button("Rename Fingers",EditorStyles.toolbarButton))
        {
            if (hand != null)
            {
                rename(hand, hand_type);
            }
        }


    }

    public void rename(Transform thand,string ttype)
    {
        thand.name = "hand_" + ttype;
        for (int i = 0; i < thand.childCount; i++)
        {
            Transform finger = thand.GetChild(i);
            string type = choose_type(finger);
            finger.name = type + "_00"  + "_" + ttype;
            int depth =0;
            while (true)
            {
                depth++;

                finger.GetChild(0).name = type + "_0" + (depth ).ToString() + "_" + ttype;
                finger = finger.GetChild(0);
                if (finger == null)
                    break;
                if (finger.childCount == 0)
                    break;
                if (finger.GetChild(0) == null)
                    break;


            }


        }
    }

    private string choose_type(Transform f)
    {
        if (f.name.Contains("index"))
            return "index";
        else if (f.name.Contains("thumb"))
            return "thumb";
        else if (f.name.Contains("pinky"))
            return "pinky";
        else if (f.name.Contains("ring"))
            return "ring";
        else if (f.name.Contains("middle"))
            return "middle";
        return "";

    }
}