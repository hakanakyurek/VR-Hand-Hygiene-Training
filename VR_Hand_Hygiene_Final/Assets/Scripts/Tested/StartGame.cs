using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class StartGame : MonoBehaviour {

    public Obi.ObiEmitter waterEmitter;
    public Obi.ObiSolver waterSolver;
    private bool soapOpen;
    // Use this for initialization

    void Awake()
    {
        waterSolver.gameObject.SetActive(false);
        waterEmitter.gameObject.SetActive(false);
        waterEmitter.selfCollisions = true;

    }

    void Start () {

        soapOpen = false;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown("space"))
        {
            this.StartTheGame();
        }
    }

    void StartTheGame()
    {

        waterSolver.gameObject.SetActive(true);
        waterEmitter.gameObject.SetActive(true);
        soapOpen = true;
    }
}
