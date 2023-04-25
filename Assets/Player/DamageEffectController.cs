using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DamageEffectController {
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField] private float maxDamageVingette;
    [SerializeField] private float damageTakenVignetteScale;
    [SerializeField] private float damageTakenVignetteResetSpeed;
    private UnityEngine.Rendering.Universal.Vignette vingette;

    private float damageVignatte;

    private Character character;
    public void Init(Character character) {
        this.character = character;

        volumeProfile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out vingette);

        character.updateEvent += Player_updateEvent;

        character.health.damageTakenEvent += Health_damageTakenEvent;
    }

    private void Health_damageTakenEvent(float f) {
        damageVignatte += damageTakenVignetteScale * f; 
    }

    private void Player_updateEvent() {
        float hpVignette = Mathf.Lerp(maxDamageVingette, 0, character.health.HP / character.health.maxHP);
        damageVignatte = Mathf.Lerp(damageVignatte, 0, damageTakenVignetteResetSpeed * Time.deltaTime);

        vingette.intensity.value = hpVignette + damageVignatte;
    }
}
