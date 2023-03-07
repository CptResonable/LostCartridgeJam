using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeRibbon : MonoBehaviour {
    [SerializeField] private float pointSpawnInterval;
    [SerializeField] private int maxPointCount;
    public float riseSpeed;
    public float noiseDriftScale;
    public float noiseDriftSpeed;
    public AnimationCurve timeToRiseSpeed;
    public AnimationCurve timeToDriftSpeed;

    private LineRenderer lineRenderer;
    public bool spawnNewPoints;

    [SerializeField] private List<SmokeRibbonPoint> points = new List<SmokeRibbonPoint>();

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.positionCount = 0;
        lineRenderer.SetPosition(0, transform.position);
        points.Add(new SmokeRibbonPoint(this, transform.position));
        //points.Insert(1, new SmokeRibbonPoint(this, transform.position));

        StartCoroutine(SpawnPointCorutine());
    }

    private void Update() {
        lineRenderer.positionCount = points.Count + 1;
        lineRenderer.SetPosition(0, transform.position);
        for (int i = 0; i < points.Count; i++) {
            lineRenderer.SetPosition(i + 1, points[i].Update(Time.deltaTime));
        }
        //for (int i = points.Count - 1; i >= 0; i--) {
        //    lineRenderer.SetPosition(i, points[i].Update(Time.deltaTime));
        //}
    }

    //private void Update() {
    //    //for (int i = 0; i < points.Count; i++) {
    //    //    lineRenderer.SetPosition(i, points[i].Update(Time.deltaTime));
    //    //}
    //    for (int i = points.Count - 1; i >= 0; i--) {
    //        lineRenderer.SetPosition(i, points[i].Update(Time.deltaTime));
    //    }
    //}

    private IEnumerator SpawnPointCorutine() {
        while (spawnNewPoints) {
            points.Insert(0, new SmokeRibbonPoint(this, transform.position));
            //points.Add(new SmokeRibbonPoint(this, transform.position));
            //lineRenderer.positionCount++;

            if (points.Count > maxPointCount) {
                points.RemoveAt(points.Count - 1);
                lineRenderer.positionCount--;
            }
            yield return new WaitForSeconds(pointSpawnInterval);
        }
    }
}
