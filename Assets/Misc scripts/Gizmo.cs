using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour {
    private enum GizmoType { sphere }

    [SerializeField] private GizmoType type;
    [SerializeField] private Color color;

    [Header("Sphere Settings")]
    [SerializeField] float sphereRadius;

    private void OnDrawGizmos() {
        switch (type) {
            case GizmoType.sphere:
                DrawSphere();
                break;
            default:
                break;
        }
    }

    private void DrawSphere() {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, sphereRadius);
    }
}
