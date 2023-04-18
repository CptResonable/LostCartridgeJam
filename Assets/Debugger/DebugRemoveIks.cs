using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRemoveIks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private RootMotion.FinalIK.LimbIK[] armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9)) {
            armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG = GetComponents<RootMotion.FinalIK.LimbIK>();
            for (int i = 0; i < armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG.Length; i++) {
                Destroy(armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG[i]);
                //armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG[i].enabled = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.F10)) {
            EasyIK[] ez = GetComponents<EasyIK>();
            for (int j = 0; j < ez.Length; j++) {
                //Destroy(armAndLegIks_REMOVETHISIJUSTHAVETHISFORTESTIG[i]);
                Destroy(ez[j]);
            }
        }
    }
}
