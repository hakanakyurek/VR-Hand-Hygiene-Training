using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StainController : MonoBehaviour
{

    [SerializeField] private SkinnedMeshRenderer rend;
    [SerializeField] private bool waterOn=false;

    public float speed = .5F;
    public bool change_soap=true, change_stain=true;



    private Material mat;
    private float val;
    void Start ()
    {
        mat = rend.materials[0];
    }

    void Update () {


        if (change_soap)
            mat.SetFloat("_SoapValue", val);

        if (change_stain)
            mat.SetFloat("_Dissolve", val);

        if (val >= 1)
            val = 1;
        if (val <= 0)
            val = 0;
        if (waterOn)
            val += Time.deltaTime * speed;
    }
}

