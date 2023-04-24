using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Body {
    public Transform tPelvis, tTorso_1, tTorso_2, tHead, tLegL_1, tLegL_2, tLegR_1, tLegR_2, tArmL_1, tArmL_2, tHandL, tArmR_1, tArmR_2, tHandR;
    public Transform rPelvis, rTorso_1, rTorso_2, rHead, rLegL_1, rLegL_2, rLegR_1, rLegR_2, rArmL_1, rArmL_2, rHandL, rArmR_1, rArmR_2, rHandR;

    public BodyStateData postAnimationState = new BodyStateData();

    public enum BoneEnums { Pelvis, Torso_1, Torso_2, Head, LegL_1, LegL_2, LegR_1, LegR_2, ArmL_1, ArmL_2, HandL, ArmR_1, ArmR_2, HandR };

    [HideInInspector] public Transform[] tBones;
    [HideInInspector] public RagdollBone[] ragdollBones;
    [HideInInspector] public MomentumData[] momentumDatas;

    public void Init(Character character) {
        tBones = new Transform[14] {
            tPelvis,
            tTorso_1,
            tTorso_2,
            tHead,
            tLegL_1,
            tLegL_2,
            tLegR_1,
            tLegR_2,
            tArmL_1,
            tArmL_2,
            tHandL,
            tArmR_1,
            tArmR_2,
            tHandR,
        };

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

        momentumDatas = new MomentumData[14];
        for (int i = 0; i < momentumDatas.Length; i++) {
            momentumDatas[i] = new MomentumData(tBones[i]);
        }

        character.fixedUpdateEvent += Character_fixedUpdateEvent;
        character.animatorController.animatorUpdatedEvent += Character_animatorUpdatedEvent;
    }

    private void Character_fixedUpdateEvent() {
        for (int i = 0; i < momentumDatas.Length; i++) {
            momentumDatas[i].RecordPositionAndRotation(tBones[i]);
        }
    }

    private void Character_animatorUpdatedEvent() {
        postAnimationState.UpdateDatas(this);
    }

    public void Ragdollify() {
        ragdollBones[0].tDead.position = ragdollBones[0].tAlive.position;
        for (int i = 0; i < ragdollBones.Length; i++) {
            ragdollBones[i].tDead.rotation = ragdollBones[i].tAlive.rotation;

            Rigidbody rb;
            if (ragdollBones[i].tDead.TryGetComponent<Rigidbody>(out rb)) {
                rb.angularVelocity = momentumDatas[i].ComputeAngularVelocity() * 10;
                rb.velocity = momentumDatas[i].ComputeVelocity();
            }
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

    public class BodyStateData {
        private BodypartData[] boneDatas = new BodypartData[14];

        public void UpdateDatas(Body body) {
            for (int i = 0; i < boneDatas.Length; i++) {
                boneDatas[i] = new BodypartData(body.tBones[i].position, body.tBones[i].rotation);
            }
        }

        public BodypartData GetBoneState(BoneEnums boneEnum) {
            return boneDatas[(uint)boneEnum];
        }
    }

    public struct BodypartData {
        public Vector3 position;
        public Quaternion rotation;

        public BodypartData(Vector3 position, Quaternion rotation) {
            this.position = position;
            this.rotation = rotation;
        }
    }

    public struct MomentumData {
        private Vector3 angularVelocity;
        private Vector3 velocity;

        private Vector3 lastPosition, newPosition;
        private Quaternion lastRotation, newRotation;

        public MomentumData(Transform tBone) {
            lastPosition = tBone.position;
            lastRotation = tBone.rotation;
            newPosition = tBone.position;
            newRotation = tBone.rotation;

            angularVelocity = Vector3.zero;
            velocity = Vector3.zero;
        }

        public void RecordPositionAndRotation(Transform tBone) {
            lastPosition = newPosition;
            lastRotation = newRotation;
            newPosition = tBone.position;
            newRotation = tBone.rotation;
        }

        public Vector3 ComputeVelocity() {
            return (newPosition - lastPosition) / Time.fixedDeltaTime;
        }

        public Vector3 ComputeAngularVelocity() {
            var deltaRot = newRotation * Quaternion.Inverse(lastRotation);
            var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
            return eulerRot / Time.fixedDeltaTime * Mathf.Deg2Rad;
        }
    }
}

