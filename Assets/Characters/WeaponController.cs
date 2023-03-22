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

    //public TWrapper Interpolator = new TWrapper(0, 1, 0);
    public float weaponSwapProgress = 0;
    public float weaponSwapAnimationThing = 0;
    private Coroutine weaponSwapCorutine;
    private UpperBody.UpperBodyRotationModifier upperBodyRotationModifier_weaponYaw = new UpperBody.UpperBodyRotationModifier(Vector3.zero, new Vector3(0, 30, 0));

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

        character.characterInput.action_equipSlot1.keyDownEvent += Action_equipSlot1_keyDownEvent;
        character.characterInput.action_equipSlot2.keyDownEvent += Action_equipSlot2_keyDownEvent;

        if (equipedGun != null) {
            equipedGun.reloadStartedEvent += EquipedGun_reloadStartedEvent;
        }

        if (character.isPlayer)
            EquipGun(pistol);

        character.upperBody.AddModifier(upperBodyRotationModifier_weaponYaw);
    }

    private void EquipGun(Gun gun) {
        if (equipedGun != null) {
            equipedGun.ReloadCanceled();
            equipedGun.reloadStartedEvent -= EquipedGun_reloadStartedEvent;
            equipedGun.Unequip();
            equipedGun.gameObject.SetActive(false);
        }

        equipedGun = gun;
        equipedGun.gameObject.SetActive(true);
        equipedGun.reloadStartedEvent += EquipedGun_reloadStartedEvent;
        equipedGun.Equip(character);

        tHandTarget.localPosition = gun.targetHandPosition;
        tHandTarget.localPosition = Vector3.zero;

        //character.arms.hand_L.SetCOM();
        //character.handMovement_L.SetCOM();
        weaponEquipedEvent?.Invoke();
    }

    private void Action_attack_keyDownEvent() {
        equipedGun.TryFire(true);
    }

    private void EquipedGun_reloadStartedEvent(float reloadTime) {
        reloadStartedEvent?.Invoke(reloadTime);
    }

    private void Action_reload_keyDownEvent() {

        if (!equipedGun.isReloading)
            equipedGun.Reload();
    }

    private void Action_equipSlot1_keyDownEvent() {
        Debug.Log("SWAP!");
        if (equipedGun != pistol) {
            if (weaponSwapCorutine != null)
                character.StopCoroutine(weaponSwapCorutine);

            character.StartCoroutine(WeaponSwapCorutine(pistol));

            //EquipGun(pistol);
            JointDrive jd = handJoint.slerpDrive;
            jd.positionSpring = 100;
            jd.positionDamper = 2f;
            handJoint.slerpDrive = jd;
        }
    }

    private void Action_equipSlot2_keyDownEvent() {
        if (equipedGun != rifle) {
            if (weaponSwapCorutine != null)
                character.StopCoroutine(weaponSwapCorutine);

            character.StartCoroutine(WeaponSwapCorutine(rifle));

            //EquipGun(rifle);
            JointDrive jd = handJoint.slerpDrive;
            jd.positionSpring = 180;
            jd.positionDamper = 3.5f;
            handJoint.slerpDrive = jd;
        }
    }

    private void Character_updateEvent() {

        // Set off hand position and rotation
        if (equipedGun != null) {
            tOffHandPosition.position = equipedGun.tOffHandTarget.position;
            tOffHandPosition.rotation = equipedGun.tOffHandTarget.rotation;
        }

        // Rotate torso 2
        if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.Grounded && !character.locomotion.state_grounded.isSprinting && !character.locomotion.state_grounded.isSliding)
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 30, 0), Time.deltaTime * 4);
        else
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 0, 0), Time.deltaTime * 4);

        // Automatic fire
        if (equipedGun != null) {
            if (!equipedGun.isAuto)
                return;
        }

        if (character.characterInput.action_attack.isDown )
            equipedGun.TryFire(false);
    }

    private IEnumerator WeaponSwapCorutine(Gun newWeapon) {
        if (weaponSwapProgress > 0.5f) {
            weaponSwapProgress = 1 - weaponSwapProgress;
        }

        bool weaponSwaped = false;

        while (weaponSwapProgress < 1) {

            if (!weaponSwaped && weaponSwapProgress > 0.5f) {
                weaponSwaped = true;
                EquipGun(newWeapon);
            }

            weaponSwapAnimationThing = Mathf.Sin(weaponSwapProgress * Mathf.PI);

            weaponSwapProgress += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        weaponSwapProgress = 1;
        weaponSwapAnimationThing = Mathf.Sin(weaponSwapProgress * Mathf.PI);
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
