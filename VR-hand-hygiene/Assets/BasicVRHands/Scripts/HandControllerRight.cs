using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;
public class HandControllerRight : MonoBehaviour {

    private Animator animator;
	public Hand hand; 
	void Start () {
	
		hand = GameObject.Find("Hand1").GetComponent<Hand>();	
        animator = GetComponent<Animator>();
		transform.SetPositionAndRotation(gameObject.transform.position, Quaternion.Euler(0.0f ,0.0f ,-70.0f));
	
	}
	
	void Update () {
        /*
		if(hand.objectTag == "Small")
			animator.SetBool("isGrabbing", hand.controller.GetPress(SteamVR_Controller.ButtonMask.Trigger));
		else if(hand.objectTag == "Big")
			animator.SetBool("isGrabbingBig", hand.controller.GetPress(SteamVR_Controller.ButtonMask.Trigger));
		else if(hand.objectTag == "Middle")
			animator.SetBool("isGrabbingMid", hand.controller.GetPress(SteamVR_Controller.ButtonMask.Trigger));
		else
			animator.SetBool("isGrabbingMid", hand.controller.GetPress(SteamVR_Controller.ButtonMask.Trigger));
	    */
	}
}
