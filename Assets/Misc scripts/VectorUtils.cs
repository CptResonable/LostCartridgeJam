using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils {

    #region FromToVector
    public static Vector3 FromToVector(Vector3 from, Vector3 to) {
        return to - from; 
    }

    public static Vector3 FromToVector(Transform from, Transform to) {
        return to.position - from.position;
    }

    public static Vector3 FromToVector(GameObject from, GameObject to) {
        return to.transform.position - from.transform.position;
    }
    #endregion

    public static Vector3 RemoveYComponent(Vector3 vector) {
        return new Vector3(vector.x, 0, vector.z);
    }

    public static Vector3 RotateVectorAroundVector(Vector3 vector, Vector3 axis, float angle) {
        return Quaternion.AngleAxis(angle, axis) * vector;       
    }

    public static Vector3 RotatePointAroundVector(Vector3 point, Vector3 axis, Vector3 axisOrigin, float angle) {
        Vector3 v = FromToVector(axisOrigin, point); // Create vector from origin to point
        v = RotateVectorAroundVector(v, axis, angle); // Rotate the vector
        return axisOrigin + v;
    }

    public static Vector3 projectPointOnLine(Vector3 point, Vector3 lineStartPoint, Vector3 lineEndPoint) {
        Vector3 onVec = FromToVector(lineStartPoint, lineEndPoint);
        Vector3 pointVec = FromToVector(lineStartPoint, point);

        return Vector3.Project(pointVec, onVec) + lineStartPoint;
    }

    public static Vector3 PPPprojectPointOnLine(Vector3 point, Vector3 lineStartPoint, Vector3 lineEndPoint) {

        return Vector3.Project((point - lineStartPoint), lineStartPoint - lineEndPoint) + lineStartPoint;
    }

    public static Vector2 RandomUnitVector() {
        float random = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(random), Mathf.Sin(random));
    }
}
