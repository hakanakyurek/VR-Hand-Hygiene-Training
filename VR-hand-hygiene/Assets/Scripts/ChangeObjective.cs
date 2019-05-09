using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class ChangeObjective : MonoBehaviour {
    
    public Objective[] objectiveList;
    public bool[] objectiveCheckList;
    public TextMeshProUGUI textField;
    public VideoPlayer video_player;


    private int currentObjectiveIndex;
    

	// Use this for initialization
	void Start () {

        this.currentObjectiveIndex = -1;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown("space"))
        {
            if (currentObjectiveIndex < objectiveList.Length - 1)
                ChangeTheObjective();
        }

	}

    private void ChangeTheObjective()
    {
        this.currentObjectiveIndex++;
        textField.SetText(objectiveList[currentObjectiveIndex].obj_text);
        this.objectiveCheckList[currentObjectiveIndex] = true;
        video_player.clip = objectiveList[currentObjectiveIndex].clp;
        video_player.Play();
    }
}
[System.Serializable]
public class Objective
{

    public string obj_text;
    public VideoClip clp;
}
