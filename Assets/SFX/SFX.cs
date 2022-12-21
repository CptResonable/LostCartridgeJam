using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour {
    [SerializeField] private AudioClip[] audioClips;
    [Range(0, 1)] public float volume = 1;
    [Range(0, 2)] public float pitch = 1;
    [Range(0, 0.5f)] public float pitchVariance = 0.1f;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play() {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.pitch = pitch + (pitchVariance * Random.Range(-1f, 1f));
        audioSource.Play();

        StartCoroutine(DespawnCorutine(audioSource.clip.length * 2));
    }

    public IEnumerator DespawnCorutine(float time) {
        yield return new WaitForSeconds(time);
        EZ_Pooling.EZ_PoolManager.Despawn(transform);
    }
}
