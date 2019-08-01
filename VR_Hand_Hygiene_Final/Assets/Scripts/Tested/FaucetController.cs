using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaucetController : MonoBehaviour
{
    
    public StainController LeftController, RightController;
    public GameObject RightHandPoint,LeftHandPoint;
    public StartGame GameManager;
    void Start()
    {
        
    }

    void Update()
    {
        if (!GameManager.waterOpen)
            return;
        bool uwr=Vector3.Distance(transform.position,RightHandPoint.transform.position) <.05F, uwl=Vector3.Distance(transform.position, LeftHandPoint.transform.position) < .05F;
        
        if (uwr)
            RightController.WaterOn();
        if (uwl)
            LeftController.WaterOn();
    }

  
}
