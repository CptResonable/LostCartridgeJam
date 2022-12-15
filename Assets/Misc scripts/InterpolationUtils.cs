using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper for a float t used for interpolation,
/// </summary>
public class TWrapper {
    private float _t = 0;
    public float t {
        get { return _t; }
        set {
            _t = value;
            _t = Mathf.Clamp(value, min, max);
            if (_t == min || _t == max)
                isTransitioning = false;
            else 
                isTransitioning = true;
        }
    }

    public bool isTransitioning { get; private set; } = false;

    private float max, min;

    public TWrapper(float t, float max, float min) {
        this.t = t;
        this._t = t;
        this.max = max;
        this.min = min;
    }
}

public static class InterpolationUtils {
    static public InterpolationUtilsInstance i = new InterpolationUtilsInstance();

    public static float LinearToSmoothStep(float linearT) {
        return linearT * linearT * (3f - 2f * linearT);
    }
}

public class InterpolationUtilsInstance {

    public IEnumerator SmoothStep(float from, float to, float transitionSpeed, TWrapper tWrap) {
        float linearTransition = 0;

        while (linearTransition < 1) {
            linearTransition += Time.deltaTime * transitionSpeed;

            if (linearTransition > 1)
                linearTransition = 1;

            float t = InterpolationUtils.LinearToSmoothStep(linearTransition);
            tWrap.t = Mathf.Lerp(from, to, t);

            yield return null;
        }
        tWrap.t = to;

        yield return null;
    }

    public IEnumerator SmoothStep(float from, float to, float transitionSpeed, TWrapper tWrap, Delegates.EmptyDelegate OnFinished) {
        float linearTransition = 0;

        while (linearTransition < 1) {
            linearTransition += Time.deltaTime * transitionSpeed;
            if (linearTransition > 1)
                linearTransition = 1;

            float t = InterpolationUtils.LinearToSmoothStep(linearTransition);
            tWrap.t = Mathf.Lerp(from, to, t);

            yield return null;
        }
        tWrap.t = to;
        OnFinished();

        yield return null;
    }
}
