using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpRotation : MonoBehaviour {
    [SerializeField] Transform firstRot;
    [SerializeField] Transform secondRot;
    [SerializeField] float t;
    [SerializeField] bool YaxisOnly;
    [SerializeField] bool XZaxisOnly;
    [SerializeField] bool scaleWithDeltaTime = false;
    [SerializeField] bool fixedUpdate;
    public Vector3 eulerOffset;

    void LateUpdate() {
        if (!fixedUpdate)
            DoTheLerp();
    }

    private void FixedUpdate() {
        if (fixedUpdate)
            DoTheLerp();
    }

    private void DoTheLerp() {
        if (YaxisOnly) {
            float y;
            if (scaleWithDeltaTime)
                y = Mathf.LerpAngle(firstRot.rotation.eulerAngles.y, secondRot.rotation.eulerAngles.y, t * Time.deltaTime);
            else
                y = Mathf.LerpAngle(firstRot.rotation.eulerAngles.y, secondRot.rotation.eulerAngles.y, t);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, y, transform.rotation.eulerAngles.z);
        }
        else if (XZaxisOnly) {
            Quaternion tmp;
            if (scaleWithDeltaTime)
                tmp = Quaternion.Lerp(firstRot.rotation, secondRot.rotation, t * Time.deltaTime);
            else
                tmp = Quaternion.Lerp(firstRot.rotation, secondRot.rotation, t);

            transform.rotation = Quaternion.Euler(-tmp.x, transform.rotation.eulerAngles.y, -tmp.z);
        }
        else {
            if (scaleWithDeltaTime)
                transform.rotation = Quaternion.SlerpUnclamped(firstRot.rotation, secondRot.rotation, t * Time.deltaTime);
            else
                transform.rotation = Quaternion.SlerpUnclamped(firstRot.rotation, secondRot.rotation, t);
        }

        // Apply additional rotational offset.
        if (eulerOffset.magnitude != 0) {
            //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + eulerOffset);
            transform.Rotate(eulerOffset, Space.Self);
        }
    }
}
