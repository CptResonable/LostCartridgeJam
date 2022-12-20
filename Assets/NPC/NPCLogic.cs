using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCLogic : MonoBehaviour {
    protected NPC character;
    protected Character target;

    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected LayerMask environmentLayerMask;

    protected virtual void Awake() {
        target = GameManager.i.player;
        character = GetComponent<NPC>();
    }

    public virtual void UpdateInput(CharacterInput input) {
    }
}
