using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Rigidbody rb;
    public void Fire(Vector3 velocity) {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
    }
}
