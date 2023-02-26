using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character {
    [SerializeField] private DashTrigger dashTrigger;
    [SerializeField] private bool dropAmmo;
    [SerializeField] private GameObject ammoBoxPrefab;

    protected void Awake() {
        NPCInput npcInput = GetComponent<NPCInput>();
        npcInput.Init(this);
        characterInput = npcInput;

        base.Awake();

        health.diedEvent += Health_diedEvent;
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
        //locomotion.Dash(0.15f, dashVector);
    }

    private void Health_diedEvent() {
        if (dropAmmo)
            Instantiate(ammoBoxPrefab, transform.position, Quaternion.identity);
    }
}
