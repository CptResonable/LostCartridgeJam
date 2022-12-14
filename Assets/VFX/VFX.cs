using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour {
    [SerializeField] protected float lifetime;
    protected float time;

    private Despawner despawner;

    protected void Awake() {
        despawner = GetComponent<Despawner>();
        if (despawner == null)
            despawner = gameObject.AddComponent<Despawner>();
    }
    
    protected virtual void Update() {
        time += Time.deltaTime;
    }

    public virtual void Initiate(Transform origin) {
        time = 0;

        despawner.DelayedDespawn(lifetime);
    }
}