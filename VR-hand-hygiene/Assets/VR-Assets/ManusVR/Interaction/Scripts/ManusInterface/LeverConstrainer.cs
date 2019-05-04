using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    X,
    Y
}
public class LeverConstrainer : MonoBehaviour
{

    public Axis Axis = Axis.Y;

    private Vector3 initialEuler;
    private Rigidbody rb;

	// Use this for initialization
	IEnumerator Start ()
	{
	    rb = GetComponent<Rigidbody>();
        yield return new WaitForSeconds(0.1f);
	    initialEuler = transform.rotation.eulerAngles;

        StartCoroutine(CheckLeverRotation());
	}
	
	// Update is called once per frame
	IEnumerator CheckLeverRotation()
	{
        while (true)
	    {
	        switch (Axis)
	        {
	            case Axis.X:
	                if (Mathf.Abs(transform.rotation.eulerAngles.y - initialEuler.y) > 0.5f)
	                {
	                    rb.angularVelocity = Vector3.zero;
	                    var euler = transform.rotation.eulerAngles;
	                    euler.y = initialEuler.y;
	                    transform.rotation = Quaternion.Euler(euler);
	                }
                    break;
	            case Axis.Y:
	                if (Mathf.Abs(transform.rotation.eulerAngles.x - initialEuler.x) > 0.5f)
	                {
	                    rb.angularVelocity = Vector3.zero;
	                    var euler = transform.rotation.eulerAngles;
	                    euler.x = initialEuler.x;
	                    transform.rotation = Quaternion.Euler(euler);
	                }
                    break;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	        yield return null;
	    }
	}
}
