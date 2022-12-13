using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInput : CharacterInput {
    private NPCLogic npcLogic;

    public void Init(NPC npc) {
        base.Init();

        mouseMovement = new MouseMovement(npc);

        foreach (KeyAction action in actions) {
            action.InitNPC();
        }

        npcLogic = npc.GetComponent<NPCLogic>();

        npc.updateEvent += NPC_updateEvent;
    }

    private void NPC_updateEvent() {
        moveInput = Vector3.zero;

        npcLogic.UpdateInput(this);
        //if (Input.GetKey(KeyCode.W))
        //    moveInput.z += 1;
        //if (Input.GetKey(KeyCode.S))
        //    moveInput.z -= 1;
        //if (Input.GetKey(KeyCode.A))
        //    moveInput.x -= 1;
        //if (Input.GetKey(KeyCode.D))
        //    moveInput.x += 1;

        //moveInput.Normalize();
    }
}
