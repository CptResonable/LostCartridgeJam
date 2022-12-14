using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
    [SerializeField] private DamageEffectController damageEffectController;

    protected void Awake() {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.Init(this);
        characterInput = playerInput;
        damageEffectController.Init(this);

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
}
