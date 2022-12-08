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
    private float currentHeight = 1;

    public Vector3 inputDir;
    private Vector3 inputDirLocal;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float airTime = 0;
    private bool jumpOnCooldown = false;

    private Player player;

    public void Initialize(Player player) {
        this.player = player;
        rb = player.GetComponent<Rigidbody>();
        player.updateEvent += player_updateEvent;
        player.fixedUpdateEvent += Player_fixedUpdateEvent;
    }

    private void player_updateEvent() {
        VerticalMovement();

        inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            inputDir.z += 1;
        if (Input.GetKey(KeyCode.S))
            inputDir.z -= 1;
        if (Input.GetKey(KeyCode.A))
            inputDir.x -= 1;
        if (Input.GetKey(KeyCode.D))
            inputDir.x += 1;

        if (inputDir.magnitude > 1)
            inputDir.Normalize();

        inputDirLocal = player.transform.TransformDirection(inputDir);

        //player.transform.rotation = Quaternion.Euler(0, player.fpCamera.yaw, 0);

        if (!jumpOnCooldown && isGrounded && Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    private void Player_fixedUpdateEvent() {
        if (isGrounded)
            airTime = 0;
        else
            airTime += Time.deltaTime;

        if (!isGrounded) {
            rb.AddForce(Vector3.down * 9.81f * airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }


        HorizontalMovement();
        //if (!Input.GetKey(KeyCode.LeftShift))
        player.rb.rotation = Quaternion.Euler(0, player.fpCamera.yaw, 0);
        //player.transform.rotation = Quaternion.Euler(0, player.head.yaw, 0);
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(player.transform.position, Vector3.down, out downHit, 100 )) {
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

    float force;
    private void HorizontalMovement() {
        rb.velocity = Vector3.Lerp(rb.velocity, inputDirLocal * moveSpeed + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);

        if (player.handMovement.targetDeltaPos.magnitude > 0.3f) {
            Vector3 v = Vector3.Project(rb.velocity, player.handMovement.targetDeltaPos);
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity - v, Time.fixedDeltaTime * 20);
        }

        //force = Mathf.Lerp(force, handDistanceForceCurve.Evaluate(player.handMovement.targetDeltaPos.magnitude) * -400000, 5 * Time.fixedDeltaTime);
        //rb.AddForce(player.handMovement.targetDeltaPos.normalized * force * Time.fixedDeltaTime);
        ////rb.velocity = Vector3.Lerp(rb.velocity, player.handMovement.targetDeltaPos.normalized, -player.handMovement.targetDeltaPos.sqrMagnitude * 10 * Time.deltaTime);
    }
    //private void HorizontalMovement() {
    //    rb.velocity = Vector3.Lerp(rb.velocity, inputDirLocal * moveSpeed + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);
    //    //rb.AddForce(player.handMovement.targetDeltaPos * -900000 * Time.fixedDeltaTime);
    //    force = Mathf.Lerp(force, handDistanceForceCurve.Evaluate(player.handMovement.targetDeltaPos.magnitude) * -400000, 5 * Time.fixedDeltaTime);
    //    rb.AddForce(player.handMovement.targetDeltaPos.normalized * force * Time.fixedDeltaTime);
    //    //rb.velocity = Vector3.Lerp(rb.velocity, player.handMovement.targetDeltaPos.normalized, -player.handMovement.targetDeltaPos.sqrMagnitude * 10 * Time.deltaTime);
    //}

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpOnCooldown = true;
        player.StartCoroutine(JumpCorutine());
    }

    private IEnumerator JumpCorutine() {
        yield return new WaitForSeconds(0.3f);
        jumpOnCooldown = false;
    }
}
