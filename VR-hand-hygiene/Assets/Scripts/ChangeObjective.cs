using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeObjective : MonoBehaviour {
    
    public string[] objectiveList;
    public bool[] objectiveCheckList;
    public TextMeshProUGUI textField;

    private int currentObjectiveIndex;
    

	// Use this for initialization
	void Start () {

        this.currentObjectiveIndex = -1;
        ChangeTheObjective();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown("space"))
        {
            if (this.currentObjectiveIndex < this.objectiveList.Length - 1)
                this.ChangeTheObjective();
        }

	}

    private void ChangeTheObjective()
    {
        this.currentObjectiveIndex++;
        textField.SetText(this.objectiveList[this.currentObjectiveIndex]);
        this.objectiveCheckList[currentObjectiveIndex] = true;
    }
}
