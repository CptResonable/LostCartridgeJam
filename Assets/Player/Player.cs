using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Head head;
    public FPCamera fpCamera;
    public PlayerController playerController;
    public Hand hand;
    public Rigidbody rb;
    public HandMovement handMovement;
    public Arm arm;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        fpCamera.Initialize(this);
        playerController.Initialize(this);
        arm.Initialize(this);
    }

    private void Update() {
        updateEvent?.Invoke();
        fpCamera.SetRotation();
    }

    private void FixedUpdate() {
        fixedUpdateEvent?.Invoke();
        handMovement.DoUpdate();
    }

    private void LateUpdate() {
        lateUpdateEvent?.Invoke();
    }
}
