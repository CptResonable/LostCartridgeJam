using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {
    [SerializeField] private float destroyTimer;

    private void Awake() {
        DestroyCorutine();
    }

    private IEnumerator DestroyCorutine() {
        yield return new WaitForSeconds(destroyTimer);
        Destroy(gameObject);
    }
}
