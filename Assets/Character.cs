using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public bool isPlayer;
    public CharacterInput characterInput;
    public FPCamera fpCamera;
    public UpperBody upperBody;
    public PlayerController playerController;
    public HandMovement handMovement;
    public WeaponController weaponController;
    public Arm arm;
    public Health health;
    public Body body;

    public Rigidbody rb;
    public Animator animator;

    public GameObject goAliveModel;
    public GameObject goDeadModel;
    public CapsuleCollider capColider;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;
    public event Delegates.EmptyDelegate animatorUpdatedEvent;

    private Vector3 localVelocity;

    protected void Awake() {
        rb = GetComponent<Rigidbody>();
        capColider = GetComponent<CapsuleCollider>();

        fpCamera.Initialize(this);
        playerController.Initialize(this);
        weaponController.Init(this);
        upperBody.Init(this);
        arm.Init(this);
        health.Init(this);
        body.Init(this);

        health.diedEvent += Health_diedEvent;
    }

    protected void Update() {
        if (!health.isAlive)
            return;

        UpdateAnimator();

        updateEvent?.Invoke();
    }

    protected void FixedUpdate() {
        if (!health.isAlive)
            return;

        fixedUpdateEvent?.Invoke();
        handMovement.DoUpdate();
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

    private void UpdateAnimator() {
        Vector3 preAnimPelvisPos = body.tPelvis.position;
        localVelocity = Vector3.Lerp(localVelocity, transform.InverseTransformVector(rb.velocity), Time.deltaTime * 4);
        animator.SetFloat("velForward", localVelocity.z / 6);
        animator.SetFloat("velSide", localVelocity.x / 6);
        animator.Update(Time.deltaTime);
        animatorUpdatedEvent?.Invoke();
        body.tPelvis.position = Vector3.Lerp(body.tPelvis.position, new Vector3(preAnimPelvisPos.x, body.tPelvis.position.y, preAnimPelvisPos.z), 0.5f);
    }
}
