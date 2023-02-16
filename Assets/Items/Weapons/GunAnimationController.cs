using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimationController : MonoBehaviour {
    private Animator animator;
    private Gun gun;

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
