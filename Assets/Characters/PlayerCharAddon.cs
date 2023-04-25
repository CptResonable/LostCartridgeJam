using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharAddon : MonoBehaviour{
    private Character character;
    [SerializeField] private DamageEffectController damageEffectController;
    [SerializeField] private Camera deathCamera;

    private void Awake() {
        character = GetComponent<Character>();
        damageEffectController.Init(character);
        character.health.diedEvent += Health_diedEvent;
    }

    private void Health_diedEvent() {
        deathCamera.gameObject.SetActive(true);
    }
}
