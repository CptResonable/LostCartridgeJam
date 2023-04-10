using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCLogic : MonoBehaviour {
    protected Character character;
    protected Character target;
    protected CharacterInput input;

    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected LayerMask environmentLayerMask;

    protected event Delegates.EmptyDelegate updateInputEvent;

    protected virtual void Awake() {
        target = GameManager.i.player;
        character = GetComponent<Character>();
        input = GetComponent<CharacterInput>();
    }

    public virtual void UpdateInput(CharacterInput input) {
        updateInputEvent?.Invoke();
    }
}
