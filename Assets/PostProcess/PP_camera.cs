using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PP_camera : MonoBehaviour {

    [SerializeField] RenderTexture rtS;
    [SerializeField] RenderTexture rtD;
    private void Update() {
        //GetComponent<Camera>().activeTexture = rtS;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        rtS = source;
        rtD = destination;
        Debug.Log("RENDER!!");  
    }
}
