using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class ChangeObjective : MonoBehaviour {
    
    public List<Objective> objectiveList;
    private bool[] objectiveCheckList;
    public TextMeshProUGUI textField;
    public VideoPlayer video_player;


    private int currentObjectiveIndex;
    

	// Use this for initialization
	void Start () {

        this.currentObjectiveIndex = -1;
        objectiveCheckList = new bool[objectiveList.Count];

    }
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown("space"))
        {
            if (currentObjectiveIndex < objectiveList.Count - 1)
                ChangeTheObjective();
        }

	}

    private void ChangeTheObjective()
    {
        this.currentObjectiveIndex++;
        textField.SetText((currentObjectiveIndex+1).ToString()+"-"+objectiveList[currentObjectiveIndex].GetObjectiveText());
        this.objectiveCheckList[currentObjectiveIndex] = true;
        video_player.clip = objectiveList[currentObjectiveIndex].GetClip();
        video_player.Play();
    }
}
[System.Serializable]
public class Objective
{
    [TextArea(2,5), SerializeField] private string obj_text;
    private VideoClip clp;

    public string GetObjectiveText()
    {
        return obj_text;
    }
    public VideoClip GetClip()
    {
        return clp;
    }

}
