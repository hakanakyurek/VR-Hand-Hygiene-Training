using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;
public class HandController : MonoBehaviour {

    private Animator animator;
	public Hand hand;
	public string tag;
	void Start () {
	
	hand = GameObject.Find("Hand2").GetComponent<Hand>();	
        animator = GetComponent<Animator>();
	
	}
	
	void Update () {
        
	//animator.SetBool("isGrabbing", hand.controller.GetPress(SteamVR_Controller.ButtonMask.Trigger));
	
	}
}
