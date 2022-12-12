using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public CharacterInput characterInput;
    public FPCamera fpCamera;
    public UpperBody upperBody;
    public PlayerController playerController;
    public HandMovement handMovement;
    public WeaponController weaponController;
    public Arm arm;
    public Body body;

    public Rigidbody rb;
    public Animator animator;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    private Vector3 localVelocity;

    protected void Awake() {
        rb = GetComponent<Rigidbody>();

        fpCamera.Initialize(this);
        playerController.Initialize(this);
        weaponController.Init(this);
        upperBody.Init(this);
        arm.Initialize(this);
    }

    protected void Update() {

        UpdateAnimator();

        updateEvent?.Invoke();
        fpCamera.SetRotation();
    }

    protected void FixedUpdate() {
        fixedUpdateEvent?.Invoke();
        handMovement.DoUpdate();
    }

    protected void LateUpdate() {
        lateUpdateEvent?.Invoke();
    }

    private void UpdateAnimator() {
        Vector3 preAnimPelvisPos = body.tPelvis.position;
        localVelocity = Vector3.Lerp(localVelocity, transform.InverseTransformVector(rb.velocity), Time.deltaTime * 4);
        animator.SetFloat("velForward", localVelocity.z / 6);
        animator.SetFloat("velSide", localVelocity.x / 6);
        animator.Update(Time.deltaTime);
        body.tPelvis.position = Vector3.Lerp(body.tPelvis.position, new Vector3(preAnimPelvisPos.x, body.tPelvis.position.y, preAnimPelvisPos.z), 0.5f);
    }
}
