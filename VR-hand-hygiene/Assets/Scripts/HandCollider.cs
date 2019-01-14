using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
public class HandCollider : MonoBehaviour {

    public Transform parent;
    public ObiSolver solver;
    public Material mat;
    public string dessolveReference;


    public float resetTime=.5f;
    private bool isOn = true;
    private void Awake()
    {
        solver.OnCollision += Solver_OnCollision;
        InvokeRepeating("EnableDisable", 1, resetTime);
    }
    void Start () {
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform notend = parent.GetChild(i);
            for (int j= 0;j < notend.childCount; j++)
            {
                Transform a = notend.GetChild(j);
                BoxCollider col = a.gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(.02f,.02f,.02f);
               
                ObiCollider obiCollider = a.gameObject.AddComponent<ObiCollider>();
                obiCollider.SourceCollider = col;
            }
        }

	}
	
	// Update is called once per frame
	void Update () {

       


    }

    void EnableDisable()
    {
        if (isOn)
        {
            solver.OnCollision -= Solver_OnCollision;
            isOn = false;
        }
         else
        {
            solver.OnCollision += Solver_OnCollision;
            isOn = true;
        }

    }
    void Solver_OnCollision(object sender,Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        // Performans sıkıntısında saniye başına detection denenebilir. O da olmazsa raycast denenebilir.
        // Logic going to implemented.
        Debug.Log("Collusion Detected!");
        Debug.Log(e);
    }
}
