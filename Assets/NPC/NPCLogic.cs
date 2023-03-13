using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCLogic : MonoBehaviour {
    protected Character character;
    protected Character target;

    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected LayerMask environmentLayerMask;

    protected virtual void Awake() {
        target = GameManager.i.player;
        character = GetComponent<Character>();
    }

    public virtual void UpdateInput(CharacterInput input) {
    }
}
