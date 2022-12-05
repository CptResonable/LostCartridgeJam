using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] bool localRotation;
    [SerializeField] bool x = true;
    [SerializeField] bool y = true;
    [SerializeField] bool z = true;
    [SerializeField] bool lateUpdate;
    public Vector3 eulerOffset;

    float old_x;
    float old_y;
    float old_z;

    private void FixedUpdate() {
        if (!lateUpdate)
            DoTheCopying();
    }

    void LateUpdate() {
        if (lateUpdate)
            DoTheCopying();
    }

    void DoTheCopying() {
        old_x = transform.rotation.eulerAngles.x;
        old_y = transform.rotation.eulerAngles.y;
        old_z = transform.rotation.eulerAngles.z;

        if (localRotation)
            transform.localRotation = target.localRotation;
        else
            transform.rotation = target.rotation;

        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles);
        transform.Rotate(eulerOffset, Space.Self);

        Vector3 newRotation = transform.eulerAngles;
        if (!x)
            newRotation.x = old_x;
        if (!y)
            newRotation.y = old_y;
        if (!z)
            newRotation.z = old_z;

        transform.rotation = Quaternion.Euler(newRotation);
    }
}
