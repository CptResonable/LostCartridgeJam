using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vfx_muzzleFlash : VFX {
    private MeshRenderer meshRenderer;
    public override void Initiate() {
        base.Initiate();

        Debug.Log("INIT!");
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("_InitTime", Time.time);
        meshRenderer.material.SetFloat("_Lifetime", lifetime);
        meshRenderer.material.SetVector("_NoiseOffset", transform.position.normalized * Time.time * 1000);
    }
}
