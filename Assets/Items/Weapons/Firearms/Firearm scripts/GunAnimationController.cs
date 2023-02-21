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

        gun.gunFiredEvent += Gun_gunFiredEvent;
        gun.bulletChaimeredEvent += Gun_bulletChaimeredEvent;
        gun.equipedEvent += Gun_equipedEvent;
        gun.unequipedEvent += Gun_unequipedEvent;
        animator.keepAnimatorControllerStateOnDisable = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.H))
            animator.SetTrigger("rackBolt");
        if (Input.GetKeyDown(KeyCode.J))
            animator.SetTrigger("reload");

        leftHandMagGrabT = tLeftHandMagGrabT.localPosition.x;
    }

    public void InitReload() {
        animator.SetTrigger("reload");
    }

    private void Gun_gunFiredEvent(Vector3 rotationalRecoil, Vector3 translationalRecoil) {
        animator.SetTrigger("fire");
        animator.SetBool("bulletInChaimber", false);
    }

    private void Gun_bulletChaimeredEvent() {
        animator.SetBool("bulletInChaimber", true);
    }

    private void Gun_equipedEvent() {
        animator.SetTrigger("equip");
    }

    private void Gun_unequipedEvent() {
        animator.SetTrigger("unequip");
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
