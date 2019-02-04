using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoapHand : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("SoapTrigger"))
        {
            Debug.Log("entered");
        }
    }
}
