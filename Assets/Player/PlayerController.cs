using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerController {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveAcceleration;

    [SerializeField] private float bounceUpSpeed;
    [SerializeField] private float yVelLerpSpeed;

    [SerializeField] private float jumpVelocity;
    [SerializeField] private AnimationCurve airTimeToGravityScale;
    [SerializeField] private AnimationCurve handDistanceForceCurve;

    [SerializeField] private float targetHeight = 0.8f;

    [SerializeField] private LayerMask layerMask;

    private float currentHeight = 1;

    private Vector3 inputDirLocal;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float airTime = 0;
    private bool jumpOnCooldown = false;

    private Character character;

    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
        character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
    }

    private void Character_updateEvent() {
        VerticalMovement();

        inputDirLocal = character.transform.TransformDirection(character.characterInput.moveInput);
    }

    private void Character_fixedUpdateEvent() {
        if (isGrounded)
            airTime = 0;
        else
            airTime += Time.deltaTime;

        if (!isGrounded) {
            rb.AddForce(Vector3.down * 9.81f * airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }


        HorizontalMovement();
        character.rb.rotation = Quaternion.Euler(0, character.fpCamera.yaw, 0);
    }

    private void Action_jump_keyDownEvent() {
        if (!jumpOnCooldown && isGrounded)
            Jump();
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, layerMask)) {
            currentHeight = downHit.distance;
        }

        float deltaHeight = currentHeight - targetHeight;

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
        rb.velocity = Vector3.Lerp(rb.velocity, inputDirLocal * moveSpeed + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);
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
}
