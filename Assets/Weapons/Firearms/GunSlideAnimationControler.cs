using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSlideAnimationControler : MonoBehaviour {
    [SerializeField] private Gun gun;

    private Animator animator_slide;
    private bool isSlideBack = false;

    private void Awake() {
        animator_slide = GetComponent<Animator>();

        gun.gunFiredEvent += Gun_gunFiredEvent;
        gun.reloadFinishedEvent += Gun_reloadCompletedEvent;
    }

    private void Gun_gunFiredEvent() {
        SlideBlowback();
    }

    private void Gun_reloadCompletedEvent() {
        if (isSlideBack)
            SlideForward();
    }

    private void SlideBlowback() {
        animator_slide.Play("SlideBack");
        Debug.Log("SLIDEBACK!");
        isSlideBack = true;
    }

    private void SlideForward() {
        animator_slide.Play("SlideForward");
        Debug.Log("SLIDEBACK!");
        isSlideBack = false;
    }

    // Called when blowback animation is done.
    private void SlideBackEvent() {
        //gun.EjectCasing();

        if (gun.bulletsInMagCount > 0 || !gun.hasSlideStop)
            SlideForward();
    }
}
