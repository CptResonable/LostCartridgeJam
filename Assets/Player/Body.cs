using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Body {
    public Transform tPelvis, tTorso_1, tTorso_2, tHead, tLegL_1, tLegL_2, tLegR_1, tLegR_2, tArmL_1, tArmL_2, tHandL, tArmR_1, tArmR_2, tHandR;
    public Transform rPelvis, rTorso_1, rTorso_2, rHead, rLegL_1, rLegL_2, rLegR_1, rLegR_2, rArmL_1, rArmL_2, rHandL, rArmR_1, rArmR_2, rHandR;

    private RagdollBone[] ragdollBones;

    public void Init(Character character) {
        ragdollBones = new RagdollBone[14] {
            new RagdollBone(tPelvis, rPelvis),
            new RagdollBone(tTorso_1, rTorso_1),
            new RagdollBone(tTorso_2, rTorso_2),
            new RagdollBone(tHead, rHead),
            new RagdollBone(tLegL_1, rLegL_1),
            new RagdollBone(tLegL_2, rLegL_2),
            new RagdollBone(tLegR_1, rLegR_1),
            new RagdollBone(tLegR_2, rLegR_2),
            new RagdollBone(tArmL_1, rArmL_1),
            new RagdollBone(tArmL_2, rArmL_2),
            new RagdollBone(tHandL, rHandL),
            new RagdollBone(tArmR_1, rArmR_1),
            new RagdollBone(tArmR_2, rArmR_2),
            new RagdollBone(tHandR, rHandR),
        };
    }

    public void Ragdollify() {
        ragdollBones[0].tDead.position = ragdollBones[0].tAlive.position;
        for (int i = 0; i < ragdollBones.Length; i++) {
            ragdollBones[i].tDead.rotation = ragdollBones[i].tAlive.rotation;
        }
    }

    public class RagdollBone {
        public RagdollBone(Transform tAlive, Transform tDead) {
            this.tAlive = tAlive;
            this.tDead = tDead;
        }

        public Transform tDead;
        public Transform tAlive;
    }
}

