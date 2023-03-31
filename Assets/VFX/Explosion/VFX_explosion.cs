using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_explosion : VFX {
    private MeshRenderer meshRenderer;

    private void Awake() {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("_InitTime", Time.time);
        meshRenderer.material.SetFloat("_Lifetime", 1);
    }

    //protected override void Update() {
    //    base.Update();

    //    //if (Input.GetKeyDown(KeyCode.Alpha1)) {
    //    //    Initiate();
    //    //}
    //}

    public override void Initiate(Transform origin) {
        base.Initiate(origin);

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("_InitTime", Time.time);
        meshRenderer.material.SetFloat("_Lifetime", lifetime);
        // meshRenderer.material.SetVector("_NoiseOffset", transform.position.normalized * Time.time * 1000);

        // SoundManager.i.PlaySoundStatic(Sounds.i.explosion_gas, transform.position);
    }
}
