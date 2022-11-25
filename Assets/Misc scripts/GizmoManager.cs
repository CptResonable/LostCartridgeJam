using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : MonoBehaviour {
    public static GizmoManager i;

    private List<Gizmo> gizmos = new List<Gizmo>();

    private void Awake() {
        i = this;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < gizmos.Count; i++) {

            // Remove from list if time to destroy
            if (gizmos[i].ShouldBeDestroyed()) {
                gizmos.RemoveAt(i);
                i--;
                continue;
            }

            gizmos[i].Draw();
        }
    }

    public void DrawSphere(float duration, Color color, Vector3 position, float radius) {
        Sphere newSphere = new Sphere(duration, color, position, radius);
        gizmos.Add(newSphere);
    }

    public void DrawLine(float duration, Color color, Vector3 startPoint, Vector3 endPoint) {
        Line newLine = new Line(duration, color, startPoint, endPoint);
        gizmos.Add(newLine);
    }

    private class Gizmo {
        private Color color;
        private float timeToDestroy;

        public Gizmo(float duration, Color color) {
            this.color = color;
            timeToDestroy = Time.time + duration;
        }

        public bool ShouldBeDestroyed() {
            if (Time.time > timeToDestroy)
                return true;
            else
                return false;
        }

        public virtual void Draw() {
            Gizmos.color = color;
        }
    }

    private class Sphere : Gizmo {
        private Vector3 position;
        private float radius;

        public Sphere(float duration, Color color, Vector3 position, float radius) : base(duration, color) {
            this.radius = radius;
            this.position = position;
        }

        public override void Draw() {
            base.Draw();
            Gizmos.DrawSphere(position, radius);
        }
    }

    private class Line : Gizmo {
        private Vector3 startPoint;
        private Vector3 endPoint;

        public Line(float duration, Color color, Vector3 startPoint, Vector3 endPoint) : base(duration, color) {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public override void Draw() {
            base.Draw();
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
