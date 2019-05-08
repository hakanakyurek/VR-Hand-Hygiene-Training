using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change_shader_values : MonoBehaviour
{

    private MeshRenderer rend;
    private Material mat_01, mat_02;
    public float speed = 3;
    private float val;
    private bool up = true;
    public bool change_soap=true, change_stain=true;
	void Start ()
    {
        rend = GetComponent<MeshRenderer>();

        mat_01 = rend.materials[0];
        mat_02 = rend.materials[1];


    }

    void Update () {
        if (change_soap)
        {
            mat_01.SetFloat("_SoapValue", val);
            mat_02.SetFloat("_SoapValue", val);
        }

        if (change_stain)
        {
            mat_01.SetFloat("_Dissolve", val);
            mat_02.SetFloat("_Dissolve", val);
        }
       

        if (val >= 1)
        {
            up = false;
            val = 1;
        }

        if (val <= 0)
        {
            up = true;
            val = 0;
        }

        if (up)
            val += Time.deltaTime * speed;
        else
        {
            val -= Time.deltaTime * speed;

        }
    }
}
