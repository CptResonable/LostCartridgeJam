using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour {
    public float noiseScrollSpeed;
    [SerializeField] private Vector3 translationScale;
    [SerializeField] private Vector3 rotationalScale;
    [SerializeField] private AnimationCurve shakeScaleCurve;

    public float scale;
    private Vector3 translationOffset;

    private List<ShakeInstance> shakeInstances = new List<ShakeInstance>();

    public void Shake(float scale, float duration) {
        shakeInstances.Add(new ShakeInstance(this, scale, duration));
    }

    private void LateUpdate() {

        for (int i = 0; i < shakeInstances.Count; i++) {
            transform.Rotate(shakeInstances[i].eulerOffset, Space.Self);
        }
    }

    public class ShakeInstance {
        private Shaker shaker;
        public float scale;
        public float duration;
        public Vector3 eulerOffset;

        private float noiseOffset;
            
        public ShakeInstance(Shaker shaker, float scale, float duration) {
            this.shaker = shaker;
            this.scale = scale;
            this.duration = duration;

            noiseOffset = Random.Range(-1000, 1000);

            shaker.StartCoroutine(ShakeCorutine(scale, duration));
        }

        private IEnumerator ShakeCorutine(float scale, float duration) {
            float t = 0;

            while (t < 1) {
                t += Time.deltaTime / duration;
                noiseOffset += Time.deltaTime * shaker.noiseScrollSpeed;

                eulerOffset.x = (Mathf.PerlinNoise(noiseOffset, noiseOffset + 32f) - 0.5f) * shaker.rotationalScale.x * scale;
                eulerOffset.y = (Mathf.PerlinNoise(noiseOffset - 132f, noiseOffset + 322f) - 0.5f) * shaker.rotationalScale.y * scale;
                eulerOffset.z = (Mathf.PerlinNoise(noiseOffset - 532f, noiseOffset + 122f) - 0.5f) * shaker.rotationalScale.z * scale;

                yield return null;
            }

            shaker.shakeInstances.Remove(this);
        }
    }
}
