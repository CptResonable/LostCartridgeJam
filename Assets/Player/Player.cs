using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {

    protected void Awake() {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.Init(this);
        characterInput = playerInput;

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
