using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static void DelayedFunctionCall(Delegates.EmptyDelegate funtion, float time) {
        GameManager.i.StartCoroutine(DelayedFunctionCallCorutine(funtion, time));
    }

    private static IEnumerator DelayedFunctionCallCorutine(Delegates.EmptyDelegate funtion, float time) {
        yield return new WaitForSeconds(time);
        funtion();

        yield return null;
    }
}
