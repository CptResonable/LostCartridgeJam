using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {
    private static EquipmentManager _i;

    public static EquipmentManager i {
        get {
            if (_i == null) {
                GameObject go = new GameObject("EquipmentManager");
                _i = go.AddComponent<EquipmentManager>();
            }

            return _i;
        }
    }

    public void SpawnLoadout(Character character, Loadout loadout) {
        foreach (GameObject goPrefab in loadout.items) {
            Equipment item = SpawnItem(goPrefab);
            character.equipmentManager.AddItem(item);
        }
    }

    private Equipment SpawnItem(GameObject prefab) {
        GameObject goItem = Instantiate(prefab, transform);
        Equipment item = goItem.GetComponent<Equipment>();
        item.ID = IdManager.CreateNewID();
        return item;
    }
}
