using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectMaterial_", menuName = "ScriptableObjects/EffectMaterial")]
public class EffectMaterial : ScriptableObject {
    public GameObject VFX_bulletHit;
    public bool useColorReader;
    public Color color_bulletHit;
    public float size;

    public GameObject SFX_bulletHit;
}
