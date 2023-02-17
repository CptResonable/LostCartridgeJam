using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSoundController : MonoBehaviour {
    [SerializeField] private GameObject gunShotSfxPrefab;

    private Gun gun;

    private void Awake() {
        gun = GetComponentInParent<Gun>();
        gun.gunFiredEvent += Gun_gunFiredEvent;
    }

    private void Gun_gunFiredEvent(Vector3 rotationalRecoil, Vector3 translationalRecoil) {
        AudioManager.i.PlaySoundStatic(gunShotSfxPrefab, transform.position);
    }

    public void OnMagInSoundEvent() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.magIn_01, transform.position);
    }

    public void OnMagOutSoundEvent() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.magOut_01, transform.position);
    }

    public void OnRackBoltSoundEvent() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.rackBolt_01, transform.position);
    }
}
