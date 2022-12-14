using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character {
    [SerializeField] private DashTrigger dashTrigger;

    protected void Awake() {
        NPCInput npcInput = GetComponent<NPCInput>();
        npcInput.Init(this);
        characterInput = npcInput;

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

    public void DashAttack(Vector3 dashVector) {
        dashTrigger.DashAttack(0.15f);
        playerController.Dash(0.15f, dashVector);
    }
}
