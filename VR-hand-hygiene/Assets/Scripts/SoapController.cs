using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoapController : MonoBehaviour {

    public Obi.ObiEmitter emitter;
    public Obi.ObiSolver solver;
    private void Awake()
    {
        emitter.gameObject.SetActive(false);
        solver.gameObject.SetActive(false);
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
