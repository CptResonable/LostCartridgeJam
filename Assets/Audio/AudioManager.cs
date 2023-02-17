using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    private static AudioManager _i;
    public SfxLibrary sfxLibrary;

    public static AudioManager i {
        get {
            if (_i == null) {
                GameObject go = new GameObject("AudioManager");
                _i = go.AddComponent<AudioManager>();
                _i.sfxLibrary = Resources.Load("SfxLibrary") as SfxLibrary;
            }

            return _i;
        }
    }

    public void PlaySoundStatic(GameObject sfxPrefab, Vector3 position) {
        GameObject goSFX = EZ_Pooling.EZ_PoolManager.Spawn(sfxPrefab.transform, position, Quaternion.identity).gameObject;
        SFX sfx = goSFX.GetComponent<SFX>();
        sfx.Play();
    }
}
