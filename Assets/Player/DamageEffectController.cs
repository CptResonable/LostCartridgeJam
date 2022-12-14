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

    private Player player;
    public void Init(Player player) {
        this.player = player;

        volumeProfile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out vingette);

        player.updateEvent += Player_updateEvent;

        player.health.damageTakenEvent += Health_damageTakenEvent;
    }

    private void Health_damageTakenEvent(float f) {
        damageVignatte += damageTakenVignetteScale * f; 
    }

    private void Player_updateEvent() {
        float hpVignette = Mathf.Lerp(maxDamageVingette, 0, player.health.HP / player.health.maxHP);
        damageVignatte = Mathf.Lerp(damageVignatte, 0, damageTakenVignetteResetSpeed * Time.deltaTime);

        vingette.intensity.value = hpVignette + damageVignatte;
    }
}
