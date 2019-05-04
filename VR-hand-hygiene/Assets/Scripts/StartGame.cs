using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class StartGame : MonoBehaviour {

    public Obi.ObiEmitter waterEmitter;
    public Obi.ObiEmitter soapEmitter;
    public Obi.ObiSolver waterSolver;
    public bool soapOpen;
    // Use this for initialization

    void Awake()
    {
        this.StartTheGame(false);
    }

    void Start () {


    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown("space"))
        {
            this.StartTheGame(true);
        }
    }

    void StartTheGame(bool state)
    {
        waterEmitter.gameObject.SetActive(state);
        waterSolver.gameObject.SetActive(state);
        soapOpen = state;
    }
}
