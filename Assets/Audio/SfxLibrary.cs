using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SfxLibrary", menuName = "ScriptableObjects/SfxLibrary")]
public class SfxLibrary : ScriptableObject {

    [Header("Weapon sounds")]
    public GameObject rileShot_01;
    public GameObject pistolShot_01;
    public GameObject magOut_01;
    public GameObject magIn_01;
    public GameObject rackBolt_01;
    public GameObject dryFire_01;

    [Header("Footstep sounds")]
    public GameObject footstepWalkDirt;
    public GameObject footstepRunDirt;
    public GameObject wallClimb_01;
}
