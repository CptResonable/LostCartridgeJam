using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loadout", menuName = "ScriptableObjects/Loadout")]
public class Loadout : ScriptableObject {
    public GameObject[] items;
}
