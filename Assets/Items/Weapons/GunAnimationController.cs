using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimationController : MonoBehaviour {
    [SerializeField] private Transform tLeftHandMagGrabT;

    private Animator animator;
    private Gun gun;

    public float leftHandMagGrabT;

    public event Delegates.EmptyDelegate magInsertedEvent;
    public event Delegates.EmptyDelegate magDroppedEvent;
    public event Delegates.EmptyDelegate boltRackedEvent;

    private void Awake() {
        gun = GetComponentInParent<Gun>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.H))
            animator.SetTrigger("rackBolt");
        if (Input.GetKeyDown(KeyCode.J))
            animator.SetTrigger("reload");

        leftHandMagGrabT = tLeftHandMagGrabT.localPosition.x;
    }

    public void InitReload() {
        Debug.Log("RELOAD!");
        animator.SetTrigger("reload");
    }

    public void OnMagInsertedEvent() {
        magInsertedEvent?.Invoke();
    }

    public void OnMagDroppedEvent() {
        magDroppedEvent?.Invoke();
    }

    public void OnBoltRackedEvent() {
        boltRackedEvent?.Invoke();
    }
}
