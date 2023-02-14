using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bouncer {
    public class BounceInstance {
        public Vector3 offset;

        public BounceInstance(Action<BounceInstance> OnBounceFinished, AnimationCurve curve, Vector3 bounceVector, float amount, float duration) {
            GameManager.i.StartCoroutine(BounceCorutine(OnBounceFinished, curve, bounceVector, amount, duration));
        }

        private IEnumerator BounceCorutine(Action<BounceInstance> OnBounceFinished, AnimationCurve curve, Vector3 bounceVector, float amount, float duration) {
            float t = 0;

            while (t < 1) {
                t += Time.deltaTime / duration;
                offset = bounceVector * amount * curve.Evaluate(t);
                yield return null;
            }
            offset = Vector3.zero;

            OnBounceFinished(this);
        }
    }
}
