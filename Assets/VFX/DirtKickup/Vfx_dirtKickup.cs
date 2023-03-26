using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vfx_dirtKickup : VFX {
    private MeshRenderer meshRenderer;
    public void Initiate(Transform origin, bool overrideColor, Color colorOverride, float size) {
        base.Initiate(origin);
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        //ColorReader.i.Move(transform.position, -transform.forward);
        StartCoroutine(DelayCorutine(overrideColor, colorOverride, size));
    }

    IEnumerator DelayCorutine(bool overrideColor, Color colorOverride, float size) {
        yield return new WaitForEndOfFrame();
        Color color = Color.white;
        if (overrideColor)
            color = colorOverride;
        //Color color = ColorReader.i.ReadColor();

        meshRenderer.material.SetFloat("_InitTime", Time.time);
        meshRenderer.material.SetFloat("_Lifetime", lifetime);
        meshRenderer.material.SetFloat("_Size", size);
        meshRenderer.material.SetVector("_NoiseOffset", transform.position.normalized * Time.time * 1000);
        meshRenderer.material.SetColor("_Color0", color);
    }
}
