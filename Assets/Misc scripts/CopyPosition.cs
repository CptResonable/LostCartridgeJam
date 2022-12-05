using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] bool localPosition;
    [SerializeField] bool lateUpdate;
    [SerializeField] Vector3 offset;

    void FixedUpdate() {
        if (!lateUpdate)
            DoTheCopy();
    }

    private void LateUpdate() {
        if (lateUpdate)
            DoTheCopy();
    }

    void DoTheCopy() {
        if (localPosition) {
            transform.localPosition = target.localPosition + offset;
        }
        else {
            transform.position = target.position + offset;
        }
    }
}
