using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMaterialKeeper : MonoBehaviour {
    public EffectMaterial effectMaterial;

    public void PlayBulletHitEffects(Vector3 hitPoint, Vector3 hitNormal) {
        GameObject goDirtKickup = EZ_Pooling.EZ_PoolManager.Spawn(effectMaterial.VFX_bulletHit.transform, hitPoint + hitNormal * 0.1f, Quaternion.LookRotation(hitNormal)).gameObject;
        Vfx_dirtKickup dirtKickup = goDirtKickup.GetComponent<Vfx_dirtKickup>();

        dirtKickup.Initiate(transform, true, effectMaterial.color_bulletHit, effectMaterial.size);

        //AudioManager.i.PlaySoundStatic(effectMaterial.SFX_bulletHit, hitPoint);
    }

    public void PlayFootstepEffects() {

    }
}
