using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character {
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
}
