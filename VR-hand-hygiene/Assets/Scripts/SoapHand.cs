using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class SoapHand : MonoBehaviour {

    public Obi.ObiEmitter emitter;
    public Obi.ObiSolver solver;
    public ParticleSystem particles;
    // Use this for initialization
    void Awake () {
        particles.Stop();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("SoapTrigger"))
        {
            Debug.Log(gameObject.name + "Entered");

            emitter.gameObject.SetActive(true);
            solver.gameObject.SetActive(true);
            emitter.KillAll();

            particles.Play();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SoapTrigger"))
        {
            Debug.Log(gameObject.name + "Exited");
            emitter.gameObject.SetActive(false);
            solver.gameObject.SetActive(false);
        }
    }
}
