using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public bool isPlayer;
    public CharacterInput characterInput;
    public FPCamera fpCamera;
    public UpperBody upperBody;
    public Locomotion locomotion;
    public WeaponController weaponController;
    public AnimatorController animatorController;
    public Arms arms;
    public Health health;
    public Body body;

    public Rigidbody rb;
    public Animator animator;

    public Transform tRig;
    public GameObject goAliveModel;
    public GameObject goDeadModel;
    public CapsuleCollider capColider;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    protected void Awake() {
        rb = GetComponent<Rigidbody>();
        capColider = GetComponent<CapsuleCollider>();

        animatorController.Init(this);
        fpCamera.Initialize(this);
        locomotion.Initialize(this);
        weaponController.Init(this);
        upperBody.Init(this);
        arms.Init(this);
        health.Init(this);
        body.Init(this);

        health.diedEvent += Health_diedEvent;
    }

    protected void Update() {
        if (!health.isAlive)
            return;

        updateEvent?.Invoke();
    }

    protected void FixedUpdate() {
        if (!health.isAlive)
            return;

        fixedUpdateEvent?.Invoke();
    }

    protected void LateUpdate() {
        if (!health.isAlive)
            return;

        lateUpdateEvent?.Invoke();
    }

    private void Health_diedEvent() {
        capColider.enabled = false;
        rb.isKinematic = true;
        goAliveModel.SetActive(false);
        body.Ragdollify();
        goDeadModel.SetActive(true);
    }
}
