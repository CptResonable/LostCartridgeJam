using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float moveAcceleration;

    [SerializeField] private float bounceUpSpeed;
    [SerializeField] private float yVelLerpSpeed;

    [SerializeField] private float jumpVelocity;
    [SerializeField] private AnimationCurve airTimeToGravityScale;
    [SerializeField] private AnimationCurve handDistanceForceCurve;
    [SerializeField] private AnimationCurve bounceDownCurve;

    [SerializeField] private float targetHeight = 0.8f;
    [SerializeField] private float crouchTargetHeight = 0.45f;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] public WallrunController wallrunController;

    public bool isSprinting;
    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    public bool isCrouching;
    public event Delegates.EmptyDelegate crouchStartedEvent;
    public event Delegates.EmptyDelegate crouchEndedEvent;

    private float currentHeight = 1;

    private Vector3 inputDir;

    private Rigidbody rb;
    public bool isGrounded = true;
    private float airTime = 0;
    private bool jumpOnCooldown = false;

    private List<Bouncer.BounceInstance> bounceInstances = new List<Bouncer.BounceInstance>();

    private Character character;

    // Dash 
    private bool isDashing;
    private Vector3 dashVector;

    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
        character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
        character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;
    }

    private void Action_sprint_keyDownEvent() {
        isSprinting = true;
        sprintStartedEvent?.Invoke();
    }

    private void Action_sprint_keyUpEvent() {
        isSprinting = false;
        sprintEndedEvent?.Invoke();
    }

    private void Character_updateEvent() {
        inputDir = character.transform.TransformDirection(character.characterInput.moveInput);

        if (character.characterInput.action_crouch.isDown) {
            if (!isCrouching) {
                isCrouching = true;
                crouchStartedEvent?.Invoke();
            }
        }
        else {
            if (isCrouching) {
                isCrouching = false;
                crouchEndedEvent?.Invoke();
            }
        }

        character.tRig.localPosition = -0.5f * Vector3.up;
        foreach (var instance in bounceInstances) {
            character.tRig.position += instance.offset;
        }
    }

    private float yRotationError;
    private float yDeltaError;
    [SerializeField] private float rotateForce;
    [SerializeField] private float rotateDamperCoef;
    [SerializeField] private Transform tTargetRoation;
    private void Character_fixedUpdateEvent() {
        if (isGrounded)
            airTime = 0;
        else 
            airTime += Time.deltaTime;
            
        if (!isGrounded) {

            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            rb.AddForce(Vector3.down * 9.81f * airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }
        else {
            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
        }

        if (wallrunController.isWallRunning) {
            character.rb.velocity = wallrunController.runVelocity;
        }
        else {
            HorizontalMovement();
        }

        VerticalMovement();

        Quaternion targetRotation;
        if (wallrunController.isWallRunning)
            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        else
            targetRotation = Quaternion.Euler(0, character.fpCamera.yaw, 0);

        tTargetRoation.rotation = targetRotation;
    }

    private void Action_jump_keyDownEvent() {

        if (!jumpOnCooldown && isGrounded) {
            bounceInstances.Add(new Bouncer.BounceInstance(OnBounceFinished, bounceDownCurve, Vector3.down, 0.3f, 0.15f));
        }
        else if (wallrunController.isWallRunning) {

            wallrunController.StopWallRun();

            Vector3 jumpVector = Vector3.Lerp(Vector3.Lerp(character.fpCamera.tCamera.forward, wallrunController.wallHit.normal, 0.5f), Vector3.up, 0.25f).normalized;
            character.rb.velocity += (jumpVector * jumpVelocity * 0.7f) + Vector3.up * jumpVelocity * 0.4f;
        }
    }

    private void OnBounceFinished(Bouncer.BounceInstance bounceInstance) {
        bounceInstances.Remove(bounceInstance);
        Jump();
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, layerMask)) {
            currentHeight = downHit.distance;
        }

        float finalTargetHeight = targetHeight;
        if (isCrouching)
            finalTargetHeight = crouchTargetHeight;

        float deltaHeight = currentHeight - finalTargetHeight;

        if (deltaHeight < 0 && !jumpOnCooldown) {
            isGrounded = true;
            float yVelTarget = (-deltaHeight) * bounceUpSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, yVelTarget, rb.velocity.z), yVelLerpSpeed * Time.deltaTime);
        }
        else {
            isGrounded = false;
        }
    }

    private void HorizontalMovement() {
        Vector3 moveDir_forwardPart = Vector3.zero;
        if (character.characterInput.moveInput.z > 0)
            moveDir_forwardPart = Vector3.Project(inputDir, character.transform.forward);

        Vector3 moveDir_otherPart = inputDir - moveDir_forwardPart;
        Vector3 moveVector = moveDir_otherPart * moveSpeed;

        if (isSprinting)
            moveVector += moveDir_forwardPart * sprintSpeed;
        else
            moveVector += moveDir_forwardPart * moveSpeed;

        float acc = moveAcceleration;
        if (!isGrounded)
            acc *= 0.075f;

        if (isDashing)
            rb.velocity = Vector3.Lerp(rb.velocity, dashVector * moveSpeed + Vector3.up * rb.velocity.y, acc * Time.deltaTime);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, moveVector + Vector3.up * rb.velocity.y, acc * Time.deltaTime);
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpOnCooldown = true;
        character.StartCoroutine(JumpCorutine());
    }

    private IEnumerator JumpCorutine() {
        yield return new WaitForSeconds(0.3f);
        jumpOnCooldown = false;
    }

    public void Dash(float time, Vector3 dashVector) {
        this.dashVector = dashVector;
        character.StartCoroutine(DashCorutine(time));
    }

    private IEnumerator DashCorutine(float time) {
        float defaultMoveSpeed = moveSpeed;
        float defaultAcceleration = moveAcceleration;
        isDashing = true;

        moveSpeed *= 3;
        moveAcceleration *= 2;
        yield return new WaitForSeconds(time);

        moveAcceleration = defaultAcceleration;
        moveSpeed = defaultMoveSpeed;
        isDashing = false;
    }
}
