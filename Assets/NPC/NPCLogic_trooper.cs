using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPCLogic_trooper : NPCLogic {

    private Vector3 targetPoint;
    private Vector3 toTargetVector;
    private Vector3 moveDir;

    public LogicState_randomPatrol state_randomPatrol;
    [HideInInspector] public LogicState_randomPatrol activeState;

    protected override void Awake() {
        base.Awake();

        state_randomPatrol.Init(this);
        activeState = state_randomPatrol;
        activeState.EnterState();
    }

    public override void UpdateInput(CharacterInput input) {
        base.UpdateInput(input);
        this.input = input;

        return;

        Rotation(input);
        Translation(input);

        //if (character.weaponController.equipedGun.bulletsInMagCount <= 0 && !character.weaponController.equipedGun.bulletInChaimber && character.weaponController.equipedGun.magIn)
        //    input.action_reload.Click();

        if (Input.GetKeyDown(KeyCode.T))
            FindNewTargetPosition();

        //dashAttackCooldown -= Time.deltaTime;
        //if (dashAttackCooldown < 0 && toTargetVector.magnitude < 2)
        //    Dash();
        navMeshAgent.transform.position = transform.position;
    }

    private void Rotation(CharacterInput input) {
        Vector3 lookFlatDirection = Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up);
        float dAngle = Vector3.SignedAngle(lookFlatDirection, VectorUtils.FromToVector(input.transform.position, target.transform.position), Vector3.up);
        input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

        float dAnglePitch = Vector3.SignedAngle(character.body.tHead.forward, toTargetVector, character.transform.right);
        //Debug.DrawRay(input.transform.position, toTargetVector, Color.red);
        //Debug.DrawRay(input.transform.position, -character.fpCamera.pitch, Color.blue);
        dAnglePitch = Vector3.Angle(character.body.tHead.forward, toTargetVector);
        input.mouseMovement.yDelta = character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
    }

    private void Translation(CharacterInput input) {
        navMeshAgent.SetDestination(targetPoint);

        Vector3 moveDir = transform.InverseTransformVector(navMeshAgent.desiredVelocity.normalized);
        //moveDir = navMeshAgent.desiredVelocity.normalized;

        toTargetVector = VectorUtils.FromToVector(input.transform.position, targetPoint);

        if (target.transform.position.y > transform.position.y + 0.1f && Vector3.Distance(transform.position, target.transform.position) < 2)
            input.action_jump.Click();

        if (toTargetVector.magnitude > 1)
            input.moveInput = moveDir;
        else
            input.moveInput = Vector3.zero;
    }

    private void FindNewTargetPosition() {
        int maxTries = 10;
        NavMeshHit navMeshHit;

        for (int tries = 0; tries < maxTries; tries++) {
            Vector2 randDir = VectorUtils.RandomUnitVector();
            float randRange = Random.Range(2f, 20f);

            //NavMeshPath path = new NavMeshPath();
            //path.
            if (NavMesh.SamplePosition(transform.position + new Vector3(randDir.x, 0, randDir.y) * randRange, out navMeshHit, 1, NavMesh.AllAreas)) {
                targetPoint = navMeshHit.position;
                break;
            }
        }
    }

    private void FindNewTargetPosition(Vector3 origin, float radius) {
        int maxTries = 10;
        NavMeshHit navMeshHit;

        for (int tries = 0; tries < maxTries; tries++) {
            Vector2 randDir = VectorUtils.RandomUnitVector();

            //NavMeshPath path = new NavMeshPath();
            //path.
            if (NavMesh.SamplePosition(origin + new Vector3(randDir.x, 0, randDir.y) * radius, out navMeshHit, 1, NavMesh.AllAreas)) {
                targetPoint = navMeshHit.position;
                break;
            }
        }
    }

    [System.Serializable]
    public class LogicState {
        protected NPCLogic_trooper logic;

        public enum StateIDEnum { RandomPatrol }
        [HideInInspector] public StateIDEnum stateID;

        public virtual void Init(NPCLogic_trooper logic) {
            this.logic = logic;
        }

        public virtual void EnterState() {
        }

        public virtual void ExitState() {
        }
    }

    [System.Serializable]
    public class LogicState_randomPatrol : LogicState {

        private bool targetReached = false;
        public override void Init(NPCLogic_trooper logic) {
            base.Init(logic);
            stateID = StateIDEnum.RandomPatrol;
        }

        public override void EnterState() {
            base.EnterState();
            logic.updateInputEvent += Logic_updateInputEvent;
            FindNewTargetPosition();
        }

        public override void ExitState() {
            base.ExitState();
            logic.updateInputEvent -= Logic_updateInputEvent;
        }

        private void Logic_updateInputEvent() {
            if (targetReached)
                return;

            logic.navMeshAgent.SetDestination(logic.targetPoint);

            logic.moveDir = logic.transform.InverseTransformVector(logic.navMeshAgent.desiredVelocity.normalized);
            Vector3 toTargetVector = VectorUtils.FromToVector(logic.input.transform.position, logic.targetPoint);

            logic.input.moveInput = logic.moveDir;
            if (toTargetVector.magnitude > 1) {
                logic.input.moveInput = logic.moveDir;
            }
            else {
                logic.input.moveInput = Vector3.zero;
                TargetReached();
            }

            Rotation();

            logic.navMeshAgent.transform.position = logic.transform.position;
        }

        private void Rotation() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCameraTarget.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.navMeshAgent.desiredVelocity.normalized, Vector3.up);
            logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

            logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        }

        private void TargetReached() {
            targetReached = true;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 3);
        }

        private void FindNewTargetPosition() {
            logic.FindNewTargetPosition(logic.transform.position, Random.Range(2f, 20f));
            targetReached = false;
        }
    }
}
