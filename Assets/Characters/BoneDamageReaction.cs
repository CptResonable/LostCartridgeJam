using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneDamageReaction {
    private Transform tBone;
    private Character character;
    public Vector3 eulerRotation;

    private float reactionDuration = 0.5f;
    public float reactionDurationModifier = 1;

    private List<ReactionInstance> reactions = new List<ReactionInstance>();

    public BoneDamageReaction(Transform tBone, Character character) {
        this.tBone = tBone;
        this.character = character;
    }

    public void ApplyAndUpdate() {

        eulerRotation = Vector3.zero;
        for (int i = 0; i < reactions.Count; i++) {
            eulerRotation += reactions[i].eulerOffset;
        }

        tBone.Rotate(eulerRotation, Space.World);
    }

    public void AddReaction(Vector3 euler) {
        reactions.Add(new ReactionInstance(this, euler));
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
}
