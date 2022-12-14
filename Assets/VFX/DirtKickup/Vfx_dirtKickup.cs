using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vfx_dirtKickup : VFX {
    private MeshRenderer meshRenderer;
    public override void Initiate(Transform origin) {
        base.Initiate(origin);
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        //ColorReader.i.Move(transform.position, -transform.forward);
        StartCoroutine(DelayCorutine());
    }

    IEnumerator DelayCorutine() {
        yield return new WaitForEndOfFrame();
        //Color color = ColorReader.i.ReadColor();
        Color color = Color.white;
        meshRenderer.material.SetFloat("_InitTime", Time.time);
        meshRenderer.material.SetFloat("_Lifetime", lifetime);
        meshRenderer.material.SetVector("_NoiseOffset", transform.position.normalized * Time.time * 1000);
        meshRenderer.material.SetColor("_Color0", color);
    }
}
