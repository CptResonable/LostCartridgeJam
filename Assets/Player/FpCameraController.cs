using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpCameraController : MonoBehaviour {
    [HideInInspector] public Camera camera;
    [HideInInspector] public Animator animator;

    [SerializeField] private Character character;
    private Shaker shaker;

    private float headbobAmount = 0;


    private void Awake() {
        camera = GetComponent<Camera>();
        animator = GetComponent<Animator>();
        shaker = GetComponent<Shaker>();

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
        character.locomotion.wallrunController.horizontalRunStopped += WallrunController_horizontalRunStopped;

        character.locomotion.slideStartedEvent += Locomotion_slideStartedEvent;
        character.locomotion.slideEndedEvent += Locomotion_slideEndedEvent;
        character.locomotion.jumpStartedEvent += Locomotion_jumpStartedEvent;
    }

    private void Update() {
        headbobAmount = Mathf.Lerp(headbobAmount, character.rb.velocity.magnitude / 4, Time.deltaTime * 4);
        animator.SetFloat("Velocity", headbobAmount);
    }

    private void LateUpdate() {
        transform.Rotate(Vector3.Lerp(Vector3.zero, character.damageReactionController.bdrHead.eulerRotation, 0.3f), Space.World);
    }

    private void WallrunController_verticalRunStarted() {
        animator.SetBool("IsWallClimbing", true);
    }

    private void WallrunController_verticalRunStopped() {
        animator.SetBool("IsWallClimbing", false);
    }

    private void WallrunController_horizontalRunStarted() {
    }

    private void WallrunController_horizontalRunStopped() {
    }

    private void Locomotion_slideStartedEvent() {
        animator.SetBool("IsSliding", true);
    }

    private void Locomotion_slideEndedEvent() {
        animator.SetBool("IsSliding", false);
    }

    private void Locomotion_jumpStartedEvent() {
        animator.SetTrigger("Jump");
        shaker.Shake(1, 0.18f);
    }
}
