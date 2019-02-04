using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class SoapHand : MonoBehaviour {

    public Obi.ObiEmitter emitter;
    public Obi.ObiSolver solver;
    // Use this for initialization
    void Awake () {
        transform.GetComponent<ParticleSystem>().Stop();
        emitter.gameObject.SetActive(false);
        solver.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("SoapTrigger"))
        {
            Debug.Log("entered");

            emitter.gameObject.SetActive(true);
            solver.gameObject.SetActive(true);
            emitter.KillAll();

            transform.GetComponent<ParticleSystem>().Play();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SoapTrigger"))
        {
            Debug.Log("Exited");
            emitter.gameObject.SetActive(false);
            solver.gameObject.SetActive(false);
        }
    }
}
