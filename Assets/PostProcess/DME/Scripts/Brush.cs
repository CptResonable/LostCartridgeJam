#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DME {
    [ExecuteInEditMode]
    public class Brush : MonoBehaviour {
        [SerializeField] Settings settings;
        [SerializeField] GameObject marker;
        bool enabled;

        public delegate void EmptyDelegate();
        public static event EmptyDelegate requestValuesEvent;
        
        public delegate void BrushDelegate(Settings settings, Vector3 position, Vector3 normal);
        public static event BrushDelegate brushEvent;
        bool smooth;

        void OnEnable() {
            EditorApplication.update += EditorUpdate;
            MeshEditor.floatEvent -= OnFloatEvent;
            MeshEditor.floatEvent += OnFloatEvent;
            MeshEditor.boolEvent -= OnBoolEvent;
            MeshEditor.boolEvent += OnBoolEvent;
            MeshEditor.directionEvent -= OnDirectionEvent;
            MeshEditor.directionEvent += OnDirectionEvent;
            MeshEditor.colorEvent -= OnColorEvent;
            MeshEditor.colorEvent += OnColorEvent;
            MeshEditor.paintChannelEvent -= OnPaintChanelEvent;
            MeshEditor.paintChannelEvent += OnPaintChanelEvent;
            
            requestValuesEvent?.Invoke();
            
            if (!MeshEditor.isActive)
                DestroyImmediate(gameObject);
        }
        
        void OnDisable() {
            EditorApplication.update -= EditorUpdate;
            MeshEditor.floatEvent -= OnFloatEvent;
            MeshEditor.boolEvent -= OnBoolEvent;
            MeshEditor.directionEvent -= OnDirectionEvent;
            MeshEditor.colorEvent -= OnColorEvent;
            MeshEditor.paintChannelEvent -= OnPaintChanelEvent;
        }
        
        void EditorUpdate() {
            if (!enabled)
                return;
            RaycastHit hit;
            if (MeshEditor.sceneViewCamera != null) {
                Ray ray = MeshEditor.sceneViewCamera.ScreenPointToRay(MeshEditor.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    if (hit.transform.tag == "Chunk") {
                        transform.position = hit.point;
                        Debug.DrawRay(hit.point, hit.normal, Color.green, Time.deltaTime);
                    }
                }
                if (MeshEditor.lmb) {
                    if (brushEvent != null) {
                        brushEvent.Invoke(settings, transform.position, hit.normal);
                    }
                }
            }
        }

        void OnDirectionEvent(MeshEditor.Directions direction) {
            settings.editDirection = direction;
        }

        void OnFloatEvent(MeshEditor.Floats setting, float value) {
            switch (setting) {
                case MeshEditor.Floats.radius:
                    settings.radius = value;
                    transform.localScale = Vector3.one * value * 2;
                    break;
                case MeshEditor.Floats.editStrength:
                    settings.editStrength = value;
                    break;
                case MeshEditor.Floats.editSmoothing:
                    settings.editSmoothing = value;
                    break;
                case MeshEditor.Floats.smoothingStrength:
                    settings.smoothingStrenth = value;
                    break;
                case MeshEditor.Floats.paintStrength:
                    settings.paintStrength = value;
                    break;
                case MeshEditor.Floats.paintSmoothing:
                    settings.paintSmoothing = value;
                    break;
            }
        }

        void OnBoolEvent(MeshEditor.Bools setting, bool value) {
            switch (setting) {

                case MeshEditor.Bools.enableDisable:
                    enabled = value;
                    marker.SetActive(value);
                    break;

                case MeshEditor.Bools.editMesh:
                    settings.isEditing = value;
                    break;

                case MeshEditor.Bools.paint:
                    settings.isPainting = value;
                    break;

                case MeshEditor.Bools.subtract:
                    settings.invertDirection = value;
                    break;

                case MeshEditor.Bools.smooth:
                    settings.isSmoothing = value;
                    break;
            }
        }

        void OnColorEvent(Color32 color) {
            settings.color = color;
        }
        
        void OnPaintChanelEvent(PaintChannels activeChannels) {
            settings.activeChannels = activeChannels;
        }

        [Serializable]
        public class Settings {
            [SerializeField] public float radius;

            [SerializeField] public bool isEditing;
            [SerializeField] public bool isPainting;
            [SerializeField] public bool isSmoothing;

            // Mesh edit
            [SerializeField] public MeshEditor.Directions editDirection;
            [SerializeField] public float editSmoothing;
            [SerializeField] public float editStrength;
            [SerializeField] public bool invertDirection;

            // Paint
            [SerializeField] public float paintStrength;
            [SerializeField] public float paintSmoothing;
            [SerializeField] public Color32 color = Color.white;
            [SerializeField] public PaintChannels activeChannels = new PaintChannels(false, false, false, false);

            // Smoothing
            [SerializeField] public float smoothingStrenth;
        }
    }

    public class PaintChannels {
        public bool r, g, b, a;

        public PaintChannels(bool r, bool g, bool b, bool a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
#endif