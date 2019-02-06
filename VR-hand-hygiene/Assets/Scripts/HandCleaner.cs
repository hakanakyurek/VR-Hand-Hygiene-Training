using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCleaner : MonoBehaviour {

    public bool waterOn = true;
    public string tag = "Sink";
    public GameObject left, right;
    private Renderer rdLeft,rdRight;
    private Material matLeft, matRight;
    public float value;
	void Start () {
        rdLeft = left.GetComponent<Renderer>();
        rdRight = right.GetComponent<Renderer>();
        matLeft = rdLeft.material;
        matRight = rdRight.material;
        value = matLeft.GetFloat("_Fuzziness");

    }
	
	// Update is called once per frame
	void Update () {

	}
    private void OnTriggerStay(Collider other)
    {
        /*
        Debug.Log("hand is in collider, tag: " + other.transform.tag);
        if(waterOn && other.tag=="LeftHand")
        {
            matLeft.SetFloat("_Fuzziness", value);
            Debug.Log("material fuzziness: " + matLeft.GetFloat("_Fuzziness"));
        }*/
    }
}
