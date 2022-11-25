using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollider : MonoBehaviour {
    [SerializeField] private Collider colliderToIgnore;
    private void Awake() {
        Physics.IgnoreCollision(transform.GetComponent<Collider>(), colliderToIgnore);
    }
}
