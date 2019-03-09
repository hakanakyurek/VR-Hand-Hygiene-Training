using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCleaner : MonoBehaviour {

    public GameObject Hand;
    public Collider Collider;
    public float Divider = 10000;
    public Transform PointPalm,PointTo,PointReverse;
    public float Angle;
    [SerializeField]
    private Material Mat;
    [SerializeField]
    private Shader Sha;
    [SerializeField]
    private float dissolveAmount;
    private bool UnderWater;
    public Transform sink;
    public float dist = 1;
    private void Start()
    {
        Mat = Hand.GetComponent<Renderer>().material;

        Sha = Mat.shader;
        Mat.SetFloat("_Dissolve", 0f);
    }

    private void Update()
    {
        
        //silneişşr
        if(Input.GetKeyDown(KeyCode.R))
            Mat.SetFloat("_Dissolve", 0f);

        Vector3 vec = PointTo.position - PointPalm.position;
        Debug.Log(vec);
        Angle = Vector3.Angle(vec, Vector3.up);


        dissolveAmount = Mat.GetFloat("_Dissolve");

        UnderWater = Vector3.Distance(new Vector3(sink.position.x, 0, sink.position.z), new Vector3(PointPalm.position.x, 0, PointPalm.position.z)) < dist ? true : false;

        if (!UnderWater)
            return;

        if (dissolveAmount < 1)
        {
            if (Angle < 80 && Angle > 7.5f)
            {
                Mat.SetFloat("_Dissolve", dissolveAmount + ((90 - Angle) / Divider));
                Debug.Log("On taraf temizleniyor");
            }
            if (Angle > 100)
            {
                Debug.Log("Arka taraf temizleniyor");
                Mat.SetFloat("_Dissolve", dissolveAmount + ((Angle - 90) / Divider));
            }

        }
          

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(PointTo.position,.01f);
        Gizmos.DrawLine(PointPalm.position, PointTo.position);

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(PointReverse.position, .01f);
        Gizmos.DrawLine(PointPalm.position, PointReverse.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(sink.position,.05f);

    }
    
}
