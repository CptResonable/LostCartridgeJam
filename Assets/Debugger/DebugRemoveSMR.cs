using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRemoveSMR : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F11)) {
            Destroy(GetComponent<SkinnedMeshRenderer>());
        }
    }

}
