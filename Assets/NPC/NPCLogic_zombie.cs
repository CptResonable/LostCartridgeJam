using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic_zombie : NPCLogic {
    [SerializeField] private Character target;

    private Vector3 toTargetVector;
    public override void UpdateInput(CharacterInput input) {
        base.UpdateInput(input);

        toTargetVector = VectorUtils.FromToVector(input.transform.position, target.transform.position);

        Rotation(input);
        Translation(input);
    }

    private void Rotation(CharacterInput input) {
        float dAngle = Vector3.SignedAngle(transform.forward, toTargetVector, Vector3.up);
        input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;
    }

    private void Translation(CharacterInput input) {
        if (toTargetVector.magnitude > 0.2f)
            input.moveInput = transform.InverseTransformVector(Vector3.ProjectOnPlane(toTargetVector, Vector3.up).normalized);
    }
}
