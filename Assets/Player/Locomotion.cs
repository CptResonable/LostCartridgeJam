using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private LocomotionSettings settings;
    [SerializeField] public WallrunController wallrunController;

    [SerializeField] private Transform tTargetRoation;

    public LocomotionState_grounded state_grounded;
    public LocomotionState_inAir state_inAir;
    public LocomotionState_wallClimbing state_wallClimbing;
    [HideInInspector] public LocomotionState activeState;

    //public bool isSprinting;
    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    public bool isCrouching;
    public event Delegates.EmptyDelegate crouchStartedEvent;
    public event Delegates.EmptyDelegate crouchEndedEvent;

    public event Delegates.EmptyDelegate jumpStartedEvent;

    public bool isGrounded = true;
    public delegate void LandedDelegate(float airTime);
    public event LandedDelegate landedEvent;
    private bool jumpOnCooldown = false;

    private float currentHeight = 1;
    private Vector3 inputDir;

    private List<Bouncer.BounceInstance> bounceInstances = new List<Bouncer.BounceInstance>();

    private Character character;
    private Rigidbody rb;


    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;

        state_grounded.Init(this);
        state_inAir.Init(this);
        state_wallClimbing.Init(this);
        activeState = state_grounded;
        activeState.EnterState();
    }

    private void Character_updateEvent() {

        // Convert movement input into world space vector
        inputDir = character.transform.TransformDirection(character.characterInput.moveInput);

        // Set rig base hight (Default model is not at the correct height)
        character.tRig.localPosition = -0.75f * Vector3.up;

        // Bounces the model, now only used just before character jumps
        foreach (var instance in bounceInstances) {
            character.tRig.position += instance.offset;
        }
    }

    private void Character_fixedUpdateEvent() {
        HeightCheck();

        Quaternion targetRotation;
        if (wallrunController.isWallRunning)
            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        else
            targetRotation = Quaternion.Euler(0, character.fpCamera.yaw, 0);

        tTargetRoation.rotation = targetRotation;
    }

    private void HeightCheck() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, LayerMasks.i.environment)) {
            currentHeight = downHit.distance;
        }
        else {
            currentHeight = 100;
        }
    }

    private void EnterState_inAir() {
        activeState.ExitState();
        state_inAir.EnterState();
        activeState = state_inAir;

        Debug.Log("New state: AIR");
    }

    private void EnterState_grounded() {
        activeState.ExitState();
        state_grounded.EnterState();
        activeState = state_grounded;

        Debug.Log("New state: GROUND");
    }

    private void EnterState_wallClimbing() {
        activeState.ExitState();
        state_wallClimbing.EnterState();
        activeState = state_wallClimbing;

        Debug.Log("New state: Wall climb");
    }

    [System.Serializable]
    public class LocomotionState {
        public enum StateIDEnum { Grounded, InAir, WallClimbing }
        [HideInInspector] public StateIDEnum stateID;

        protected Locomotion locomotion;

        public virtual void Init(Locomotion locomotion) {
            this.locomotion = locomotion;
        }

        public virtual void EnterState() {
        }

        public virtual void ExitState() {
        }
    }

    [System.Serializable]
    public class LocomotionState_grounded : LocomotionState {

        public bool isSprinting;
        private bool isPreparingJump = false;
        private float timeSinceGroundTouched; // Grace period, stops character from exiting ground state for something like going over a bump

        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.Grounded;
        }

        public override void EnterState() {
            base.EnterState();

            if (locomotion.character.characterInput.action_sprint.isDown)
                SprintStarted();

            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;
        }

        public override void ExitState() {
            base.ExitState();

            if (isSprinting)
                SprintEnded();

            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent -= Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent -= Action_sprint_keyUpEvent;
        }

        private void Character_fixedUpdateEvent() {
            StillOnGroundCheck();
            VerticalMovement();
            HorizontalMovement();
        }

        private void StillOnGroundCheck() {
            if (locomotion.currentHeight > locomotion.settings.targetHeight + 0.1f)
                timeSinceGroundTouched += Time.fixedDeltaTime;
            else
                timeSinceGroundTouched = 0;

            // If time above ground is above the grace period time, then enter inAir state
            if (timeSinceGroundTouched > 0.15f)
                locomotion.EnterState_inAir();

        }

        private void VerticalMovement() {

            // Return if jumping, don't want to intefere with jump stuff
            if (locomotion.jumpOnCooldown)
                return;

            float finalTargetHeight = locomotion.settings.targetHeight;
            //if (isCrouching)
            //    finalTargetHeight = settings.crouchTargetHeight;

            float deltaHeight = locomotion.currentHeight - finalTargetHeight;

            if (deltaHeight < 0) {
                float yVelTarget = (-deltaHeight) * locomotion.settings.bounceUpSpeed;
                locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, new Vector3(locomotion.rb.velocity.x, yVelTarget, locomotion.rb.velocity.z), locomotion.settings.yVelLerpSpeed * Time.deltaTime);
            }
        }

        private void HorizontalMovement() {

            // Get forward part of inputDir
            Vector3 inputDir_forward = Vector3.zero;
            if (locomotion.character.characterInput.moveInput.z > 0)
                inputDir_forward = Vector3.Project(locomotion.inputDir, locomotion.character.transform.forward);

            // Get strafe part of input dir my subtracting forward part
            Vector3 inputDir_strafe = locomotion.inputDir - inputDir_forward;

            // Create move vector
            Vector3 moveVector = Vector3.zero;
            if (isSprinting) {
                moveVector += inputDir_forward * locomotion.settings.sprintSpeed;
                moveVector += inputDir_strafe * locomotion.settings.sprintStrafeSpeed;
            }
            else {
                moveVector += inputDir_forward * locomotion.settings.moveSpeed;
                moveVector += inputDir_strafe * locomotion.settings.strafeSpeed;
            }

            // Lerp character velocity towards new target velocity
            locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, moveVector + Vector3.up * locomotion.rb.velocity.y, locomotion.settings.moveAcceleration * Time.deltaTime);
        }

        #region Sprint
        private void Action_sprint_keyDownEvent() {
            SprintStarted();
        }
        private void Action_sprint_keyUpEvent() {
            SprintEnded();
        }

        private void SprintStarted() {
            isSprinting = true;
            locomotion.sprintStartedEvent?.Invoke();
        }

        private void SprintEnded() {
            isSprinting = false;
            locomotion.sprintEndedEvent?.Invoke();
        }
        #endregion

        #region Jump
        private void Action_jump_keyDownEvent() {

            // Don't jump again before you have jumped
            if (isPreparingJump)
                return;

            locomotion.bounceInstances.Add(new Bouncer.BounceInstance(OnBounceFinished, locomotion.settings.bounceDownCurve, Vector3.down, 0.3f, 0.15f));
            locomotion.jumpStartedEvent?.Invoke();
            isPreparingJump = true;
        }

        private void OnBounceFinished(Bouncer.BounceInstance bounceInstance) {
            locomotion.bounceInstances.Remove(bounceInstance);
            isPreparingJump = false;
            Jump();
        }

        private void Jump() {
            locomotion.rb.velocity = new Vector3(locomotion.rb.velocity.x, locomotion.settings.jumpVelocity, locomotion.rb.velocity.z);
            locomotion.jumpOnCooldown = true;

            Utils.DelayedFunctionCall(JumpCooldownDone, 0.3f); // Jump cooldown

            locomotion.EnterState_inAir();
        }

        private void JumpCooldownDone() {
            locomotion.jumpOnCooldown = false;
        }

        #endregion
    }

    [System.Serializable]
    public class LocomotionState_inAir : LocomotionState {
        public float airTime;

        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.InAir;
        }

        public override void EnterState() {
            base.EnterState();

            locomotion.character.updateEvent += Character_updateEvent;
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        }

        public override void ExitState() {
            base.ExitState();

            locomotion.character.updateEvent -= Character_updateEvent;
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStarted -= WallrunController_verticalRunStarted;

            airTime = 0f;
        }

        private void Character_updateEvent() {

            // Check for wall to run if pressing jump key
            if (locomotion.character.characterInput.action_jump.isDown)
                locomotion.wallrunController.AttemptWallRun();
        }

        private void Character_fixedUpdateEvent() {
            StillInAirCheck();
            VerticalMovement();

            airTime += Time.deltaTime;
        }

        private void StillInAirCheck() {
            if (locomotion.currentHeight < locomotion.settings.targetHeight && airTime > 0.1f)
                locomotion.EnterState_grounded();
        }

        private void VerticalMovement() {
            locomotion.rb.AddForce(Vector3.down * 9.81f * locomotion.settings.airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }

        private void WallrunController_verticalRunStarted() {
            locomotion.EnterState_wallClimbing();
        }
    }

    [System.Serializable]
    public class LocomotionState_wallClimbing : LocomotionState {
        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.WallClimbing;
        }

        public override void EnterState() {
            base.EnterState();
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        }

        public override void ExitState() {
            base.ExitState();
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStopped -= WallrunController_verticalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
        }

        private void Character_fixedUpdateEvent() {
            locomotion.rb.velocity = locomotion.wallrunController.runVelocity;
        }

        private void WallrunController_verticalRunStopped() {
            locomotion.EnterState_inAir();
        }

        private void Action_jump_keyDownEvent() {
            locomotion.wallrunController.StopWallRun();

            Vector3 lookDir = Vector3.ProjectOnPlane(locomotion.character.fpCamera.tCamera.forward, locomotion.wallrunController.wallUpVector).normalized;
            float lookDirToCameraAngle = Vector3.SignedAngle(lookDir, locomotion.wallrunController.wallHit.normal, locomotion.wallrunController.wallUpVector);

            // Get jump vector by rotating wall normal with camera to wall angle
            Vector3 jumpVector = Quaternion.AngleAxis(-lookDirToCameraAngle, Vector3.up) * locomotion.wallrunController.wallHit.normal;

            // If character has move input
            if (locomotion.character.characterInput.moveInput.magnitude > 0.5f) {
                float wasdAngle = Vector3.SignedAngle(new Vector3(0, 0, 1), locomotion.character.characterInput.moveInput, Vector3.up);
                jumpVector = Quaternion.AngleAxis(wasdAngle, Vector3.up) * jumpVector;
            }

            // Stops jump vector from goin into wall, also add some velocity away from wall
            if (Vector3.Dot(jumpVector, locomotion.wallrunController.wallHit.normal) < 0)
                jumpVector = Vector3.ProjectOnPlane(jumpVector, locomotion.wallrunController.wallHit.normal) + locomotion.wallrunController.wallHit.normal * 0.2f;

            locomotion.character.rb.velocity = jumpVector * locomotion.settings.wallJumpVelocity + Vector3.up * locomotion.character.rb.velocity.y;
        }
    }
}
