using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SmokeRibbonPoint {
    public Vector3 position;

    private SmokeRibbon smoke;
    private float aliveTime;

    private Vector3 startPosition;

    private float noiseOffset;

    public SmokeRibbonPoint(SmokeRibbon smoke, Vector3 position) {
        this.smoke = smoke;
        this.position = position;
        startPosition = position;
        noiseOffset = Time.time * 1;
    }

    public Vector3 Update(float deltaTime) {
        Debug.Log(aliveTime);
        aliveTime += deltaTime;
        position += Vector3.up * smoke.riseSpeed * smoke.timeToRiseSpeed.Evaluate(aliveTime);
        float driftChnageVel = 1; // smoke.noiseDriftSpeed / (1 + aliveTime);
        Vector3 positionOffset = new Vector3((Mathf.PerlinNoise(aliveTime * driftChnageVel + noiseOffset + 232, aliveTime * driftChnageVel + noiseOffset - 3444) - 0.5f) * smoke.noiseDriftScale * smoke.timeToDriftSpeed.Evaluate(aliveTime), 0,
            (Mathf.PerlinNoise(aliveTime * driftChnageVel + noiseOffset - 1111, aliveTime * driftChnageVel + noiseOffset + 3232) - 0.5f) * smoke.noiseDriftScale * smoke.timeToDriftSpeed.Evaluate(aliveTime));
        //Vector3 positionOffset = new Vector3(Mathf.PerlinNoise((aliveTime + Time.time * 1.5f) * driftChnageVel + 232, (aliveTime + Time.time * 1.5f) * driftChnageVel - 3444) * smoke.noiseDriftScale * smoke.timeToDriftSpeed.Evaluate(aliveTime), 0, 
        //    Mathf.PerlinNoise((aliveTime + Time.time * 1.5f) * driftChnageVel - 1111, (aliveTime + Time.time * 1.5f) * driftChnageVel + 3232) * smoke.noiseDriftScale * smoke.timeToDriftSpeed.Evaluate(aliveTime));
        //position.x += (Mathf.PerlinNoise(aliveTime + Time.time * 0.0f, aliveTime + Time.time) - 0.5f) * 0.1f * smoke.timeToDriftSpeed.Evaluate(aliveTime);
        return position + positionOffset;
    }
}
