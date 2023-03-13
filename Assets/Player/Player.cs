using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
    [SerializeField] private DamageEffectController damageEffectController;
    [SerializeField] private Camera deathCamera;

    protected void Awake() {
        //PlayerInput playerInput = GetComponent<PlayerInput>();
        //playerInput.Init(this);
        //characterInput = playerInput;
        damageEffectController.Init(this);
        health.diedEvent += Health_diedEvent;

        base.Awake();
    }

    protected void Update() {
        base.Update();
    }

    protected void FixedUpdate() {
        base.FixedUpdate();
    }

    protected void LateUpdate() {
        base.LateUpdate();
    }

    private void Health_diedEvent() {
        deathCamera.gameObject.SetActive(true);
    }
}
