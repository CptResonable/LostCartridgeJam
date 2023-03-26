using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic_assault : NPCLogic {
    [SerializeField] private float perlinScale;
    [SerializeField] private LayerMask losCheckLayerMask;

    private Vector3 toTargetVector;

    private float perlinOffset;

    private float dashAttackCooldown;

    private bool isBursting = false;
    private bool burstOnCooldown = false;

    protected override void Awake() {
        base.Awake();

        perlinOffset = Random.Range(0f, 20000f);
    }

    private void Start() {
        input.action_equipSlot2.Click();
    }

    public override void UpdateInput(CharacterInput input) {
        return;
        base.UpdateInput(input);
        this.input = input;

        toTargetVector = VectorUtils.FromToVector(input.transform.position, target.transform.position);

        Rotation(input);
        Translation(input);

        Shooting();

        if (Input.GetKeyDown(KeyCode.H))
            input.action_equipSlot1.Click();

        if (Input.GetKeyDown(KeyCode.J))
            input.action_equipSlot2.Click();

        if (character.weaponController.equipedGun.bulletsInMagCount <= 0 && !character.weaponController.equipedGun.bulletInChaimber && character.weaponController.equipedGun.magIn)
            input.action_reload.Click();
        //dashAttackCooldown -= Time.deltaTime;
        //if (dashAttackCooldown < 0 && toTargetVector.magnitude < 2)
        //    Dash();
    }

    private void Shooting() {

        //RaycastHit lineHit;
        if (!Physics.Linecast(character.transform.position + Vector3.up, target.transform.position + Vector3.up, losCheckLayerMask) && !burstOnCooldown) {
            burstOnCooldown = true;
            StartCoroutine(BurstCorutine());
            //input.action_attack.isDown = true;
        }
        else {
            //input.action_attack.isDown = false;
        }
    }

    private IEnumerator BurstCorutine() {
        yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        isBursting = true;
        float burstDuration = Random.Range(0.2f, 1.5f);
        input.action_attack.isDown = true;
        yield return new WaitForSeconds(burstDuration);
        input.action_attack.isDown = false;
        isBursting = false;
        float burstCooldown = Random.Range(0.3f, 1.2f) * burstDuration;
        yield return new WaitForSeconds(burstCooldown);
        burstOnCooldown = false;
    }
    //private void LateUpdate() {
    //    Debug.DrawRay(transform.position, toTargetVector, Color.red);
    //    Debug.DrawRay(transform.position, character.body.tHead.forward, Color.blue);
    //}

    private void Rotation(CharacterInput input) {
        Vector3 lookFlatDirection = Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up);
        float dAngle = Vector3.SignedAngle(lookFlatDirection, toTargetVector, Vector3.up);
        input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

        float dAnglePitch = Vector3.SignedAngle(character.body.tHead.forward, toTargetVector, character.transform.right);
        //Debug.DrawRay(input.transform.position, toTargetVector, Color.red);
        //Debug.DrawRay(input.transform.position, -character.fpCamera.pitch, Color.blue);
        dAnglePitch = Vector3.Angle(character.body.tHead.forward, toTargetVector);
        input.mouseMovement.yDelta = character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
    }

    private void Translation(CharacterInput input) {

        float timeSample = Time.time * perlinScale;
        Vector3 randomDirTarget = Vector3.zero;
        randomDirTarget.x = Mathf.PerlinNoise(timeSample + perlinOffset - 230, timeSample + perlinOffset + 1330) - 0.5f;
        randomDirTarget.z = Mathf.PerlinNoise(timeSample + perlinOffset + 4330, timeSample + perlinOffset - 6130) - 0.5f;
        randomDirTarget.Normalize();

        float timeSample2 = Time.time * perlinScale * 0.2f;
        float targetDistance = Mathf.PerlinNoise(timeSample + perlinOffset - 4230, timeSample + perlinOffset + 330) * (10 + 5);

        navMeshAgent.transform.position = transform.position;
        navMeshAgent.SetDestination(target.transform.position + randomDirTarget * targetDistance);

        Vector3 randomDir = Vector3.zero;
        randomDir.x = Mathf.PerlinNoise(timeSample + perlinOffset - 830, timeSample + perlinOffset + 32130) - 0.5f;
        randomDir.z = Mathf.PerlinNoise(timeSample + perlinOffset + 230, timeSample + perlinOffset - 62130) - 0.5f;
        randomDir.Normalize();

        //RaycastHit randomHit;
        if (Physics.Raycast(transform.position, randomDir, 1, environmentLayerMask))
            perlinOffset += Random.Range(-40, 40);

        randomDir = transform.InverseTransformVector(randomDir);

        Vector3 moveDir = transform.InverseTransformVector(navMeshAgent.desiredVelocity.normalized);
        if (Vector3.Angle(randomDir, moveDir) > 90)
            randomDir = Vector3.ProjectOnPlane(randomDir, moveDir);
        moveDir = Vector3.Lerp(moveDir, randomDir, 0.5f);

        if (target.transform.position.y > transform.position.y + 0.1f && Vector3.Distance(transform.position, target.transform.position) < 2)
            input.action_jump.Click();

        if (toTargetVector.magnitude > 0.2f)
            input.moveInput = moveDir;
    }

    //private void Dash() {
    //    dashAttackCooldown = 1;
    //    character.DashAttack(toTargetVector);
    //}
}
