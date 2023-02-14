using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponController {
    [SerializeField] public Gun pistol;
    [SerializeField] public Gun rifle;
    public Gun equipedGun;

    [SerializeField] private Transform tHandTarget;
    [SerializeField] private Transform tOffHandPosition;
    [SerializeField] private ConfigurableJoint handJoint;

    private Character character;

    public bool isADS;

    public event Delegates.EmptyDelegate adsEnteredEvent;
    public event Delegates.EmptyDelegate adsExitedEvent;
    public event Delegates.FloatDelegate reloadStartedEvent;
    public event Delegates.EmptyDelegate weaponEquipedEvent;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;

        character.characterInput.action_reload.keyDownEvent += Action_reload_keyDownEvent;

        character.characterInput.action_attack.keyDownEvent += Action_attack_keyDownEvent;

        if (equipedGun != null) {
            equipedGun.reloadStartedEvent += EquipedGun_reloadStartedEvent;
        }

        if (character.isPlayer)
            EquipGun(pistol);
    }

    private void EquipGun(Gun gun) {
        if (equipedGun != null) {
            equipedGun.ReloadCanceled();
            equipedGun.reloadStartedEvent -= EquipedGun_reloadStartedEvent;
            equipedGun.gameObject.SetActive(false);
        }

        equipedGun = gun;
        equipedGun.gameObject.SetActive(true);
        equipedGun.reloadStartedEvent += EquipedGun_reloadStartedEvent;

        tHandTarget.localPosition = gun.targetHandPosition;
        tHandTarget.localPosition = Vector3.zero;
        tOffHandPosition.position = gun.tOffHandTarget.position;

        //character.arms.hand_L.SetCOM();
        //character.handMovement_L.SetCOM();
        weaponEquipedEvent?.Invoke();
    }

    private void Action_attack_keyDownEvent() {
        if (equipedGun.isAuto)
            return;

        equipedGun.TryFire();
    }

    private void EquipedGun_reloadStartedEvent(float reloadTime) {
        reloadStartedEvent?.Invoke(reloadTime);
    }

    private void Action_reload_keyDownEvent() {
        equipedGun.Reload();
    }

    private void Character_updateEvent() {

        if (character.isPlayer) {
            if (Input.GetKeyDown(KeyCode.Alpha2) && equipedGun != rifle) {
                EquipGun(rifle);
                JointDrive jd = handJoint.slerpDrive;
                jd.positionSpring = 180;
                jd.positionDamper = 3.5f;
                handJoint.slerpDrive = jd;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) && equipedGun != pistol) {
                EquipGun(pistol);
                JointDrive jd = handJoint.slerpDrive;
                jd.positionSpring = 100;
                jd.positionDamper = 2f;
                handJoint.slerpDrive = jd;
            }
        }


        if (equipedGun != null) {
            if (!equipedGun.isAuto)
                return;
        }

        if (character.characterInput.action_attack.isDown)
            equipedGun.TryFire();
    }

    private void Action_ads_keyUpEvent() {
        isADS = false;
        adsExitedEvent?.Invoke();
    }

    private void Action_ads_keyDownEvent() {
        isADS = true;
        adsEnteredEvent?.Invoke();
    }
}