using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic_zombie : NPCLogic {
    [SerializeField] private float perlinScale;

    private Vector3 toTargetVector;

    private float perlinOffset;

    private float dashAttackCooldown;

    protected override void Awake() {
        base.Awake();

        perlinOffset = Random.Range(0f, 20000f);
    }

    public override void UpdateInput(CharacterInput input) {
        base.UpdateInput(input);

        toTargetVector = VectorUtils.FromToVector(input.transform.position, target.transform.position);

        Rotation(input);
        Translation(input);

        dashAttackCooldown -= Time.deltaTime;
        if (dashAttackCooldown < 0 && toTargetVector.magnitude < 2)
            Dash();
    }

    private void Rotation(CharacterInput input) {
        float dAngle = Vector3.SignedAngle(transform.forward, toTargetVector, Vector3.up);
        input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;
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
