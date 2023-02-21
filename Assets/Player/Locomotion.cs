using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private LocomotionSettings settings;
    [SerializeField] public WallrunController wallrunController;

    public bool isSprinting;
    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    public bool isCrouching;
    public event Delegates.EmptyDelegate crouchStartedEvent;
    public event Delegates.EmptyDelegate crouchEndedEvent;

    public event Delegates.EmptyDelegate jumpStartedEvent;

    public bool isGrounded = true;
    private float airTime = 0;
    public delegate void LandedDelegate(float airTime);
    public event LandedDelegate landedEvent;
    private bool jumpOnCooldown = false;

    private float currentHeight = 1;
    private Vector3 inputDir;

    private List<Bouncer.BounceInstance> bounceInstances = new List<Bouncer.BounceInstance>();

    private Character character;
    private Rigidbody rb;

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

        character.tRig.localPosition = -0.75f * Vector3.up;
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
        if (isGrounded) {
            if (airTime > 0.2f)
                landedEvent?.Invoke(airTime);
            airTime = 0;
        }
        else {
            airTime += Time.deltaTime;
        }

        if (wallrunController.isWallRunning)
            airTime = 0;
            
        if (!isGrounded) {

            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            rb.AddForce(Vector3.down * 9.81f * settings.airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
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
            bounceInstances.Add(new Bouncer.BounceInstance(OnBounceFinished, settings.bounceDownCurve, Vector3.down, 0.3f, 0.15f));
            jumpStartedEvent?.Invoke();
        }
        else if (wallrunController.isWallRunning) {

            wallrunController.StopWallRun();

            Vector3 jumpVector = Vector3.Lerp(Vector3.Lerp(character.fpCamera.tCamera.forward, wallrunController.wallHit.normal, 0.5f), Vector3.up, 0.25f).normalized;
            character.rb.velocity += (jumpVector * settings.jumpVelocity * 0.7f) + Vector3.up * settings.jumpVelocity * 0.4f;
        }
    }

    private void OnBounceFinished(Bouncer.BounceInstance bounceInstance) {
        bounceInstances.Remove(bounceInstance);
        Jump();
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, LayerMasks.i.environment)) {
            currentHeight = downHit.distance;
        }

        float finalTargetHeight = settings.targetHeight;
        if (isCrouching)
            finalTargetHeight = settings.crouchTargetHeight;

        float deltaHeight = currentHeight - finalTargetHeight;

        if (deltaHeight < 0 && !jumpOnCooldown) {
            isGrounded = true;
            float yVelTarget = (-deltaHeight) * settings.bounceUpSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, yVelTarget, rb.velocity.z), settings.yVelLerpSpeed * Time.deltaTime);
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
        Vector3 moveVector = moveDir_otherPart * settings.moveSpeed;

        if (isSprinting)
            moveVector += moveDir_forwardPart * settings.sprintSpeed;
        else
            moveVector += moveDir_forwardPart * settings.moveSpeed;

        float acc = settings.moveAcceleration;
        if (!isGrounded)
            acc *= 0.075f;

        if (isDashing)
            rb.velocity = Vector3.Lerp(rb.velocity, dashVector * settings.moveSpeed + Vector3.up * rb.velocity.y, acc * Time.deltaTime);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, moveVector + Vector3.up * rb.velocity.y, acc * Time.deltaTime);
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, settings.jumpVelocity, rb.velocity.z);
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
        float defaultMoveSpeed = settings.moveSpeed;
        float defaultAcceleration = settings.moveAcceleration;
        isDashing = true;

        settings.moveSpeed *= 3;
        settings.moveAcceleration *= 2;
        yield return new WaitForSeconds(time);

        settings.moveAcceleration = defaultAcceleration;
        settings.moveSpeed = defaultMoveSpeed;
        isDashing = false;
    }
}
