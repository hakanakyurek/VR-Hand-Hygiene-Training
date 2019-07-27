using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class SoapHand : MonoBehaviour {

    public GameObject startGame;
    public GameObject Hand;
    public Obi.ObiEmitter emitter;
    public Obi.ObiSolver solver;
    public ParticleSystem particles;
    public float SoapIncrease;

    private float SoapAmmount;
    private Material material;
    private bool openSoap;
    // Use this for initialization
    void Awake () {
        particles.Stop();
        SoapAmmount = 1;

    }
	// Update is called once per frame
	void Update () {
        material = Hand.GetComponent<Renderer>().material;
        //openSoap = startGame.GetComponent<StartGame>().soapOpen;
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag==("SoapTrigger") && openSoap)
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
        if (other.gameObject.tag==("SoapTrigger"))
        {
            Debug.Log(gameObject.name + "Exited");
            emitter.gameObject.SetActive(false);
            solver.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag==("SoapTrigger"))
        {
            if (SoapAmmount >= 1)
                return;
            SoapAmmount -= Time.deltaTime/1000;
            material.SetFloat("_SoapValue", SoapAmmount);
        }
    }
}
