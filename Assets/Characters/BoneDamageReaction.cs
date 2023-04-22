using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneDamageReaction {
    private Transform tBone;
    private Character character;

    private float pitch, yaw, roll;
    public Vector3 rotation;

    private float reactionDuration = 0.5f;
    public float reactionDurationModifier = 1;

    private List<ReactionInstance> reactions = new List<ReactionInstance>();

    public BoneDamageReaction(Transform tBone, Character character) {
        this.tBone = tBone;
        this.character = character;
    }

    public void AddReaction(float pitch) {
        this.pitch += pitch;
    }

    ////public void ApplyAndUpdate() {
    ////    //tBone.Rotate(new Vector3(0, yaw, pitch), Space.Self);
    ////    tBone.Rotate(character.transform.up, yaw, Space.World);
    ////    pitch = Mathf.Lerp(pitch, 0, Time.deltaTime * 2);
    ////    yaw = Mathf.Lerp(yaw, 0, Time.deltaTime * 2);
    ////}

    //public void ApplyAndUpdate() {
    //    //tBone.Rotate(new Vector3(0, yaw, pitch), Space.Self);
    //    tBone.Rotate(rotation, Space.World);
    //    rotation = Vector3.Lerp(rotation, Vector3.zero, 2 * Time.deltaTime);
    //}

    public void ApplyAndUpdate() {
        //tBone.Rotate(new Vector3(0, yaw, pitch), Space.Self);

        rotation = Vector3.zero;
        for (int i = 0; i < reactions.Count; i++) {
            rotation += reactions[i].eulerOffset;
        }

        tBone.Rotate(rotation, Space.World);
    }

    public void AddReaction(Vector3 euler) {
        reactions.Add(new ReactionInstance(this, euler));
        //rotation += euler;
    }

    public void AddReaction(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
    }

    private class ReactionInstance {
        public Vector3 eulerOffset;
        public ReactionInstance(BoneDamageReaction bdr, Vector3 euler) {
            bdr.character.StartCoroutine(ReactionCorutine(bdr, euler));
        }

        private IEnumerator ReactionCorutine(BoneDamageReaction bdr, Vector3 euler) {
            float t = 0;

            while (t < 1) {
                t += Time.deltaTime / (bdr.reactionDuration * bdr.reactionDurationModifier);
                eulerOffset = euler * MiscAnimationCurves.i.damageReactionCurve.Evaluate(t);
                yield return null;
            }

            eulerOffset = euler * MiscAnimationCurves.i.damageReactionCurve.Evaluate(1);
            bdr.reactions.Remove(this);
        }
    }

    //public void AddReaction(Quaternion rotation, float duration) {

    //}
}
