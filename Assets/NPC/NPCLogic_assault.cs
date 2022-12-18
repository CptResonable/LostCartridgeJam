using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic_assault : NPCLogic {
    [SerializeField] private Character target;
    [SerializeField] private float perlinScale;
    [SerializeField] private LayerMask losCheckLayerMask;

    private Vector3 toTargetVector;

    private float perlinOffset;

    private float dashAttackCooldown;

    private CharacterInput input;

    private bool isBursting = false;
    private bool burstOnCooldown = false;

    protected override void Awake() {
        base.Awake();

        perlinOffset = Random.Range(0f, 20000f);
    }

    public override void UpdateInput(CharacterInput input) {
        base.UpdateInput(input);
        this.input = input;

        toTargetVector = VectorUtils.FromToVector(input.transform.position, target.transform.position);

        Rotation(input);
        Translation(input);

        Shooting();


        if (character.weaponController.equipedGun.bulletsInMagCount <= 0)
            input.action_reload.Click();
        //dashAttackCooldown -= Time.deltaTime;
        //if (dashAttackCooldown < 0 && toTargetVector.magnitude < 2)
        //    Dash();
    }

    private void Shooting() {

        //RaycastHit lineHit;
        if (!Physics.Linecast(character.transform.position, target.transform.position, losCheckLayerMask) && !burstOnCooldown) {
            StartCoroutine(BurstCorutine());
            //input.action_attack.isDown = true;
        }
        else {
            //input.action_attack.isDown = false;
        }
    }

    private IEnumerator BurstCorutine() {
        isBursting = true;
        burstOnCooldown = true;
        float burstDuration = Random.Range(0.2f, 1.5f);
        input.action_attack.isDown = true;
        yield return new WaitForSeconds(burstDuration);
        input.action_attack.isDown = false;
        isBursting = false;
        float burstCooldown = Random.Range(0.4f, 1.5f) * burstDuration;
        yield return new WaitForSeconds(burstCooldown);
        burstOnCooldown = false;
    }
    //private void LateUpdate() {
    //    Debug.DrawRay(transform.position, toTargetVector, Color.red);
    //    Debug.DrawRay(transform.position, character.body.tHead.forward, Color.blue);
    //}

    private void Rotation(CharacterInput input) {
        float dAngle = Vector3.SignedAngle(transform.forward, toTargetVector, Vector3.up);
        input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

        float dAnglePitch = Vector3.SignedAngle(character.body.tHead.forward, toTargetVector, character.transform.right);
        //Debug.DrawRay(input.transform.position, toTargetVector, Color.red);
        //Debug.DrawRay(input.transform.position, -character.fpCamera.pitch, Color.blue);
        dAnglePitch = Vector3.Angle(character.body.tHead.forward, toTargetVector);
        Debug.Log("dAnglePitch: " + dAnglePitch);
        input.mouseMovement.yDelta = character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
    }

    private void Translation(CharacterInput input) {

        float timeSample = Time.time * perlinScale;
        Vector3 randomDir = Vector3.zero;
        randomDir.x = Mathf.PerlinNoise(timeSample + perlinOffset - 830, timeSample + perlinOffset + 32130) - 0.5f;
        randomDir.z = Mathf.PerlinNoise(timeSample + perlinOffset + 230, timeSample + perlinOffset - 62130) - 0.5f;
        randomDir.Normalize();

        randomDir = transform.InverseTransformVector(randomDir);
        Vector3 moveDir = Vector3.Lerp(transform.InverseTransformVector(Vector3.ProjectOnPlane(toTargetVector, Vector3.up).normalized), randomDir, 0.5f);

        if (toTargetVector.magnitude > 0.2f)
            input.moveInput = moveDir;
    }

    private void Dash() {
        dashAttackCooldown = 1;
        character.DashAttack(toTargetVector);
    }
}
