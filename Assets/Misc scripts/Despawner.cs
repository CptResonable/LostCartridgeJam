using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZ_Pooling;

public class Despawner : MonoBehaviour {
    private Coroutine currentDespawnRutine;

    public void DelayedDespawn(float delaySec) {
        if (currentDespawnRutine != null)
            StopCoroutine(currentDespawnRutine);

        currentDespawnRutine = StartCoroutine(_delayedDespawn(delaySec));
    }

    private IEnumerator _delayedDespawn(float delaySec) {
        yield return new WaitForSeconds(delaySec);
        EZ_PoolManager.Despawn(transform);
        yield return null;
    }
}
