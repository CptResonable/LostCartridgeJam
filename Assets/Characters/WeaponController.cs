using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponController {
    //[SerializeField] public Gun pistol;
    //[SerializeField] public Gun rifle;
    public Equipment equipedItem;

    [SerializeField] private Transform tHandTarget;
    [SerializeField] private Transform tOffHandPosition;
    [SerializeField] private ConfigurableJoint handJoint;

    public Equipment[] equipment = new Equipment[4];

    public enum State { nothingEquiped, weaponEquiped}
    public State state;

    //public TWrapper Interpolator = new TWrapper(0, 1, 0);
    public float itemSwapProgress = 0;
    public float itemSwapAnimationThing = 0;
    private Coroutine itemSwapCorutine;
    private UpperBody.UpperBodyRotationModifier upperBodyRotationModifier_weaponYaw = new UpperBody.UpperBodyRotationModifier(Vector3.zero, new Vector3(0, 30, 0));

    private Character character;

    public bool isADS;

    public event Delegates.EmptyDelegate adsEnteredEvent;
    public event Delegates.EmptyDelegate adsExitedEvent;
    public event Delegates.EmptyDelegate weaponEquipedEvent;
    public event Delegates.EmptyDelegate weaponUnEquipedEvent;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;

        character.characterInput.action_reload.keyDownEvent += Action_reload_keyDownEvent;
        character.characterInput.action_attack.keyDownEvent += Action_attack_keyDownEvent;

        character.characterInput.action_equipSlot1.keyDownEvent += Action_equipSlot1_keyDownEvent;
        character.characterInput.action_equipSlot2.keyDownEvent += Action_equipSlot2_keyDownEvent;
        character.characterInput.action_unEquip.keyDownEvent += Action_unEquip_keyDownEvent;

        //if (character.isPlayer)
        //    EquipGun(pistol);

        character.upperBody.AddModifier(upperBodyRotationModifier_weaponYaw);
    }

    private void EquipItem(Equipment newItem) {
        if (newItem.GetType() == typeof(Gun))
            EquipGun((Gun)newItem);

        handJoint.slerpDrive = newItem.handJointDriveOverride;
    }

    private void EquipGun(Gun gun) {
        if (equipedItem != null) {
            equipedItem.Unequip();
        }

        equipedItem = gun;
        equipedItem.gameObject.SetActive(true);
        equipedItem.Equip(character);

        tHandTarget.localPosition = gun.targetHandPosition;
        tHandTarget.localPosition = Vector3.zero;

        //character.arms.hand_L.SetCOM();
        //character.handMovement_L.SetCOM();
        state = State.weaponEquiped;
        weaponEquipedEvent?.Invoke();
    }

    private void Action_attack_keyDownEvent() {
        //if (state == State.weaponEquiped)
        //    equipedGun.TryFire(true);
    }

    private void Action_reload_keyDownEvent() {

        //if (!equipedGun.isReloading)
        //    equipedGun.Reload();
    }

    private void Action_equipSlot1_keyDownEvent() {
        TryEquipItem(0);
    }

    private void Action_equipSlot2_keyDownEvent() {
        TryEquipItem(1);
    }

    private void Action_unEquip_keyDownEvent() {
        if (state == State.nothingEquiped)
            return;

        character.StartCoroutine(UnEquipCorutine());

        //equipedItem.Unequip();
        //equipedItem.gameObject.SetActive(false);
        //equipedItem = null;

        //state = State.nothingEquiped;

        //weaponUnEquipedEvent?.Invoke();
    }

    private void TryEquipItem(int equipmentIndex) {
        if (equipment[equipmentIndex] == null) {
            return;
        }

        Equipment newEquipedItem = equipment[equipmentIndex];

        // Return if trying to equip already equiped item
        if (newEquipedItem == equipedItem)
            return;

        if (itemSwapCorutine != null)
            character.StopCoroutine(itemSwapCorutine);

        character.StartCoroutine(WeaponSwapCorutine(newEquipedItem));
    }

    private void Character_updateEvent() {

        // Set off hand position and rotation
        if (equipedItem != null) {
            tOffHandPosition.position = equipedItem.tOffHandTarget.position;
            tOffHandPosition.rotation = equipedItem.tOffHandTarget.rotation;
        }

        // Rotate torso 2
        if (state == State.weaponEquiped && character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.Grounded && !character.locomotion.state_grounded.isSprinting && !character.locomotion.state_grounded.isSliding)
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 30, 0), Time.deltaTime * 4);
        else
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 0, 0), Time.deltaTime * 4);

        //// Automatic fire
        //if (equipedGun != null) {
        //    if (!equipedGun.isAuto)
        //        return;
        //}
        //if (state == State.weaponEquiped)
        //    if (character.characterInput.action_attack.isDown)
        //        equipedGun.TryFire(false);
    }

    private IEnumerator WeaponSwapCorutine(Equipment newItem) {
        if (itemSwapProgress > 0.5f) {
            itemSwapProgress = 1 - itemSwapProgress;
        }

        bool itemSwaped = false;

        while (itemSwapProgress < 1) {

            if (!itemSwaped && itemSwapProgress > 0.5f) {
                itemSwaped = true;
                EquipItem(newItem);
            }

            itemSwapAnimationThing = Mathf.Sin(itemSwapProgress * Mathf.PI);

            itemSwapProgress += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        itemSwapProgress = 1;
        itemSwapAnimationThing = Mathf.Sin(itemSwapProgress * Mathf.PI);
    }

    //private IEnumerator WeaponSwapCorutine(Gun newWeapon) {
    //    if (weaponSwapProgress > 0.5f) {
    //        weaponSwapProgress = 1 - weaponSwapProgress;
    //    }

    //    bool weaponSwaped = false;

    //    while (weaponSwapProgress < 1) {

    //        if (!weaponSwaped && weaponSwapProgress > 0.5f) {
    //            weaponSwaped = true;
    //            EquipGun(newWeapon);
    //        }

    //        weaponSwapAnimationThing = Mathf.Sin(weaponSwapProgress * Mathf.PI);

    //        weaponSwapProgress += Time.deltaTime * 2;
    //        yield return new WaitForEndOfFrame();
    //    }

    //    weaponSwapProgress = 1;
    //    weaponSwapAnimationThing = Mathf.Sin(weaponSwapProgress * Mathf.PI);
    //}

    private IEnumerator UnEquipCorutine() {
        if (itemSwapProgress > 0.5f) {
            itemSwapProgress = 1 - itemSwapProgress;
        }

        while (itemSwapProgress < 1) {
            itemSwapAnimationThing = itemSwapProgress;

            itemSwapProgress += Time.deltaTime * 1;
            yield return new WaitForEndOfFrame();
        }

        itemSwapProgress = 1;
        itemSwapAnimationThing = 1;
        equipedItem.Unequip();
        equipedItem = null;

        state = State.nothingEquiped;

        weaponUnEquipedEvent?.Invoke();
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
