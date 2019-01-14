using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FPSCounter : MonoBehaviour {

    public Text fpsText;
    private float frameCount, dt, fps, updateRate=4.0f;
	void Start () {
	
	}
	
	void Update () {
        
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1 / updateRate;

        }
            
        fpsText.text = ((int)fps).ToString()+" fps";
    }
}
