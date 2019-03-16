using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCleaner : MonoBehaviour {

    public GameObject Hand;
    public float dissolveDivision = 10000;
    public Transform PointPalm,PointForwardofHand,PointBackofHand;
    
    
    
   
    [Tooltip("Just under the faucet")]
    public Transform WaterCollisionBound;
    public float DistancetoWaterCollider = 1;


    private bool UnderWater;
    private Material material;
    private float angle;
    private float dissolveAmount;


    private void Start()
    {
        material = Hand.GetComponent<Renderer>().material;

        //Sha = Mat.shader;
        material.SetFloat("_Dissolve", 0f);
    }

    private void Update()
    {
        
        //silneişşr
        if(Input.GetKeyDown(KeyCode.R))
            material.SetFloat("_Dissolve", 0f);

        Vector3 vec = PointForwardofHand.position - PointPalm.position;
        Debug.Log(vec);
        angle = Vector3.Angle(vec, Vector3.up);


        dissolveAmount = material.GetFloat("_Dissolve");

        UnderWater = Vector3.Distance(new Vector3(WaterCollisionBound.position.x, 0, WaterCollisionBound.position.z), new Vector3(PointPalm.position.x, 0, PointPalm.position.z)) < DistancetoWaterCollider ? true : false;

        if (!UnderWater)
            return;

        if (dissolveAmount < 1)
        {
            if (angle < 80 && angle > 7.5f)
            {
                material.SetFloat("_Dissolve", dissolveAmount + ((90 - angle) / dissolveDivision));
                Debug.Log("On taraf temizleniyor");
            }
            if (angle > 100)
            {
                Debug.Log("Arka taraf temizleniyor");
                material.SetFloat("_Dissolve", dissolveAmount + ((angle - 90) / dissolveDivision));
            }

        }
          

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(PointForwardofHand.position,.01f);
        Gizmos.DrawLine(PointPalm.position, PointForwardofHand.position);

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(PointBackofHand.position, .01f);
        Gizmos.DrawLine(PointPalm.position, PointBackofHand.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(WaterCollisionBound.position,.05f);

    }
    
}
