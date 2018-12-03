using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
public class HandCollider : MonoBehaviour {

    public Transform parent;

	void Start () {

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform notend = parent.GetChild(i);
            for (int j= 0;j < notend.childCount; j++)
            {
                Transform a = notend.GetChild(j);
                BoxCollider col = a.gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(.02f,.02f,.02f);
               
                ObiCollider obiCollider = a.gameObject.AddComponent<ObiCollider>();
                obiCollider.SourceCollider = col;
            }
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
