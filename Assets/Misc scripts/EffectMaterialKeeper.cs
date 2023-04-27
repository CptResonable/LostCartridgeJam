using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMaterialKeeper : MonoBehaviour {
    public EffectMaterial effectMaterial;

    public void PlayBulletHitEffects(Vector3 hitPoint, Vector3 hitNormal) {

        if (effectMaterial.useColorReader)
            ColorReader.i.Move(hitPoint, -hitNormal);

        StartCoroutine(PlayBulletHitEffectsCorutine(hitPoint, hitNormal));
        // AudioManager.i.PlaySoundStatic(effectMaterial.SFX_bulletHit, hitPoint);
    }

    private IEnumerator PlayBulletHitEffectsCorutine(Vector3 hitPoint, Vector3 hitNormal) {
        yield return new WaitForEndOfFrame();

        Color color;

        if (effectMaterial.useColorReader)
            color = ColorReader.i.ReadColor();
        else
            color = effectMaterial.color_bulletHit;

        GameObject goDirtKickup = EZ_Pooling.EZ_PoolManager.Spawn(effectMaterial.VFX_bulletHit.transform, hitPoint, Quaternion.LookRotation(hitNormal)).gameObject;
        Vfx_dirtKickup dirtKickup = goDirtKickup.GetComponent<Vfx_dirtKickup>();

        dirtKickup.Initiate(transform, true, color, effectMaterial.size);
    }

    public void PlayFootstepEffects() {

    }
}

