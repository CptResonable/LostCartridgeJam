using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Head head;
    public PlayerController playerController;
    public Hand hand;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    private void Awake() {
        playerController.Initialize(this);
        head.Initialize(this);
        hand.Initialize(this);
    }

    private void Update() {
        updateEvent?.Invoke();
    }

    private void FixedUpdate() {
        fixedUpdateEvent?.Invoke();
    }

    private void LateUpdate() {
        lateUpdateEvent?.Invoke();
    }
}
