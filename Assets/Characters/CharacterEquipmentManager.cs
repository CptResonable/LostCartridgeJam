using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterEquipmentManager {

    public Equipment[] equipment = new Equipment[4];
    public Equipment equipedItem;

    public TWrapper unequipEquipInterpolator = new TWrapper(0, 1, 0); // 0 when hnads are down 1 when up holding item, iterpolate when eqiping unequiping items
    private Coroutine unequipEquipCorutine;
    private Equipment queuedItemToEquip;

    public enum State { nothingEquiped, gunEquiped }
    public State state;

    public delegate void EquipmentDelegate(Equipment item);
    public event EquipmentDelegate itemEquipedEvent;
    public event EquipmentDelegate itemUnequipedEvent;

    private Character character;
    public void Init(Character character) {
        this.character = character;

        character.characterInput.action_equipSlot1.keyDownEvent += Action_equipSlot1_keyDownEvent;
        character.characterInput.action_equipSlot2.keyDownEvent += Action_equipSlot2_keyDownEvent;
        character.characterInput.action_unEquip.keyDownEvent += Action_unEquip_keyDownEvent;
    }

    private void Action_equipSlot1_keyDownEvent() {
        TryEquipItem(0);
    }

    private void Action_equipSlot2_keyDownEvent() {
        TryEquipItem(1);
    }

    private void Action_unEquip_keyDownEvent() {
        queuedItemToEquip = null;

        // Return if no item to unequip
        if (equipedItem == null)
            return;

        if (unequipEquipCorutine != null)
            character.StopCoroutine(unequipEquipCorutine);

        unequipEquipCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(unequipEquipInterpolator.t, 0, 2, unequipEquipInterpolator, OnEquipUnequipMovementFinished));
    }

    private void TryEquipItem(int equipmentIndex) {

        // Check if item on index exists
        if (equipment[equipmentIndex] == null) {
            return;
        }

        queuedItemToEquip = equipment[equipmentIndex];

        // Hands already lowered, equip item
        if (unequipEquipInterpolator.t == 0) {

            // If item is queued to be equiped, then equip it and bring hands back up
            if (queuedItemToEquip != null) {
                EquipQueuedItem();

                if (unequipEquipCorutine != null)
                    character.StopCoroutine(unequipEquipCorutine);

                unequipEquipCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(unequipEquipInterpolator.t, 1, 2, unequipEquipInterpolator, OnEquipUnequipMovementFinished));
            }
        }
        else { // Lower hands, then equip item

            if (unequipEquipCorutine != null)
                character.StopCoroutine(unequipEquipCorutine);

            unequipEquipCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(unequipEquipInterpolator.t, 0, 2, unequipEquipInterpolator, OnEquipUnequipMovementFinished));
        }
    }


    private void OnEquipUnequipMovementFinished() {

        // Hands brougth back up
        if (unequipEquipInterpolator.t == 1) {
        }
        else if (unequipEquipInterpolator.t == 0) { // Hands down

            // Uneqip current item
            if (equipedItem != null)
                UnequipItem();

            // If item is queued to be equiped, then equip it and bring hands back up
            if (queuedItemToEquip != null) {
                EquipQueuedItem();

                if (unequipEquipCorutine != null)
                    character.StopCoroutine(unequipEquipCorutine);

                unequipEquipCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(unequipEquipInterpolator.t, 1, 2, unequipEquipInterpolator, OnEquipUnequipMovementFinished));
            }
        }
    }

    private void EquipQueuedItem() {
        equipedItem = queuedItemToEquip;
        queuedItemToEquip = null;

        equipedItem.Equip(character);

        state = State.gunEquiped;
        itemEquipedEvent?.Invoke(equipedItem);
    }

    private void UnequipItem() {
        Equipment unequipedItem = equipedItem;

        equipedItem.Unequip();
        equipedItem = null;

        state = State.nothingEquiped;
        itemUnequipedEvent?.Invoke(unequipedItem);
    }
}
