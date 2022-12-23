#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TriangleNet;
using TriangleNet.Meshing;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using Random = System.Random;

namespace DME {
    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour {
        [SerializeField] public Vector2Int coordinates;
        [SerializeField] [HideInInspector] public Vector2 chunkPosV2;
        [SerializeField] [HideInInspector] MeshTerrain meshTerrain;
        [SerializeField] [HideInInspector] public MeshFilter meshFilter;
        [SerializeField] [HideInInspector] MeshRenderer meshRenderer;
        [SerializeField] [HideInInspector] MeshCollider meshCollider;

        int historyLength = 30;
        private  List<Dictionary<int, Vector3>> history = new List<Dictionary<int, Vector3>>(); // { new Dictionary<int, Vector3>()};
        public int[] edgeLeft; // -x edge
        public int[] edgeRight; // x edge
        public int[] edgeFront; // z edge
        public int[] edgeBack; // -z edge
        public Vector3[] lastVerts;

        public void Initiate(MeshTerrain meshTerrain, Vector2Int coords) {
            this.meshTerrain = meshTerrain;
            coordinates = coords;
            chunkPosV2 = coords * meshTerrain.chunkSize;

            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = meshTerrain.defaultMaterial;

            TriangleNet.Mesh triNetMesh;
            triNetMesh = GenerateTriNetMesh();
            if (meshTerrain.flatShaded)
                ConvertToUnityMesh(triNetMesh);
            else
                ConvertToUnityMeshNoDuplicate(triNetMesh);
        }

        private void OnEnable() {
            Brush.brushEvent -= OnBrushEvent;
            Brush.brushEvent += OnBrushEvent;
            MeshEditor.lmb_downEvent -= OnMouseDown;
            MeshEditor.lmb_downEvent += OnMouseDown;
            MeshEditor.lmb_upEvent -= OnMouseUp;
            MeshEditor.lmb_upEvent += OnMouseUp;
            MeshEditor.buttonEvent -= OnButtonEvent;
            MeshEditor.buttonEvent += OnButtonEvent;
        }
        private void OnDestroy() {
            Brush.brushEvent -= OnBrushEvent;
            MeshEditor.lmb_downEvent -= OnMouseDown;
            MeshEditor.lmb_upEvent -= OnMouseUp;
            MeshEditor.buttonEvent -= OnButtonEvent;
        }

        [SerializeField] private bool fixSeams;
        private void OnValidate() {
            if (fixSeams) {
                SmoothSeams();
                fixSeams = false;
            }
        }

        private void OnButtonEvent(MeshEditor.Buttons button) {
            //Brush.brushEvent -= Brush_brushEvent;
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            foreach (int i in history[history.Count - 1].Keys) {
                vertices[i] = history[history.Count - 1][i];
            }

            history.RemoveAt(history.Count - 1);

            meshFilter.sharedMesh.vertices = vertices;
            meshFilter.sharedMesh.RecalculateNormals();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        private void OnMouseDown() {
            history.Add(new Dictionary<int, Vector3>());
            lastVerts = meshFilter.sharedMesh.vertices;
        }

        private void OnMouseUp() {
            if (history.Count > historyLength)
                history.RemoveAt(0);

            lastVerts = null;
        }

        private void OnBrushEvent(Brush.Settings settings, Vector3 position, Vector3 normal) {

            // Return if isSmoothing, "MeshTerrain.cs" handels that.
            if (settings.isSmoothing)
                return;

            // Adjust pos for meshTerrain position.
            position -= meshTerrain.GetPos();

            // Return if chunk not affected.
            bool isAffected = false;
            if ((position.x > chunkPosV2.x - settings.radius && position.x < chunkPosV2.x + meshTerrain.chunkSize.x + settings.radius)) {
                if ((position.z > chunkPosV2.y - settings.radius && position.z < chunkPosV2.y + meshTerrain.chunkSize.y + settings.radius))
                    isAffected = true;
            }
            if (!isAffected)
                return;

            // Adjust position for local position.
            position -= transform.localPosition;

            // Edit mesh.
            EditAndOrPaintMesh(settings, position, normal);
        }

        public void SmoothSeams() {

            Vector2Int up = coordinates + new Vector2Int(0, 1);
            Vector2Int right = coordinates + new Vector2Int(1, 0);
            Vector2Int down = coordinates + new Vector2Int(0, -1);
            Vector2Int left = coordinates + new Vector2Int(-1, 0);
            
            if (meshTerrain.chunkCoords.Contains(up))
                meshTerrain.SmoothSeam(this, meshTerrain.chunks[meshTerrain.chunkCoords.IndexOf(up)], MeshTerrain.DirEnum.up);
            if (meshTerrain.chunkCoords.Contains(right))
                meshTerrain.SmoothSeam(this, meshTerrain.chunks[meshTerrain.chunkCoords.IndexOf(right)], MeshTerrain.DirEnum.right);
            if (meshTerrain.chunkCoords.Contains(down))
                meshTerrain.SmoothSeam(this, meshTerrain.chunks[meshTerrain.chunkCoords.IndexOf(down)], MeshTerrain.DirEnum.down);
            if (meshTerrain.chunkCoords.Contains(left))
                meshTerrain.SmoothSeam(this, meshTerrain.chunks[meshTerrain.chunkCoords.IndexOf(left)], MeshTerrain.DirEnum.left);
        }

        private void EditAndOrPaintMesh(Brush.Settings settings, Vector3 position, Vector3 normal) {
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            Color32[] colors32 = meshFilter.sharedMesh.colors32;

            // Edit vertexes inside brush.
            bool meshModified = false;
            for (int i = 0; i < vertices.Length; i++) {
                float dst = Vector3.Distance(vertices[i], position);
                if (Vector3.Distance(vertices[i], position) < settings.radius) {

                    if (settings.isEditing)
                        EditMesh(i, dst);
                    if (settings.isPainting)
                        PaintMesh(i, dst);

                    meshModified = true;
                }
            }

            void EditMesh(int i, float dst) {
                if (settings.editDirection == MeshEditor.Directions.contract) {
                    vertices[i] = new Vector3(vertices[i].x, position.y, vertices[i].z);
                }
                else {
                    float smoothingValue = Mathf.Pow((settings.radius - dst) / settings.radius, settings.editSmoothing);
                    float delta = smoothingValue * settings.editStrength;

                    // Add to history.
                    if (history[history.Count - 1].ContainsKey(i))
                        history[history.Count - 1][i] = lastVerts[i];
                    else
                        history[history.Count - 1].Add(i, lastVerts[i]);

                    // Set edit direction
                    Vector3 direction = Vector3.up;
                    switch (settings.editDirection) {
                        case MeshEditor.Directions.globalY:
                            direction = Vector3.up;
                            break;
                        case MeshEditor.Directions.brushToCamera:
                            direction = (MeshEditor.sceneViewCamera.transform.position - position).normalized;
                            break;
                        case MeshEditor.Directions.cameraZ:
                            direction = MeshEditor.sceneViewCamera.transform.forward * -1;
                            break;
                        case MeshEditor.Directions.normal:
                            direction = normal;
                            break;
                    }
                    if (settings.invertDirection)
                        direction *= -1;

                    // Modify vertex.
                    vertices[i] += direction * delta;
                }
            }

            void PaintMesh(int i, float dst) {
                float paintLerpConst = Mathf.Pow((settings.radius - dst) / settings.radius, settings.paintSmoothing) * settings.paintStrength;

                if (settings.activeChannels.r)
                    colors32[i].r = (byte)Mathf.Lerp(colors32[i].r, settings.color.r, paintLerpConst);
                
                if (settings.activeChannels.g)
                    colors32[i].g = (byte)Mathf.Lerp(colors32[i].g, settings.color.g, paintLerpConst);
                
                if (settings.activeChannels.b)
                    colors32[i].b = (byte)Mathf.Lerp(colors32[i].b, settings.color.b, paintLerpConst);
                
                if (settings.activeChannels.a)
                    colors32[i].a = (byte)Mathf.Lerp(colors32[i].a, settings.color.a, paintLerpConst);
                
                meshModified = true;
            }

            // Update mesh if modified.
            if (meshModified) {
                meshFilter.sharedMesh.vertices = vertices;
                meshFilter.sharedMesh.colors32 = colors32;
                meshFilter.sharedMesh.RecalculateNormals();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        #region Smoothing
        public List<Pair> GetVertsInside(Brush.Settings settings, Vector3 position) {
            List<Pair> vertsInside = new List<Pair>();
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                //Debug.Log("!!!!   " + i);
                if (Vector3.Distance(vertices[i] + transform.localPosition, position) < settings.radius) {
                    vertsInside.Add(new Pair(i, vertices[i] + transform.localPosition));
                }
            }
            return vertsInside;
        }

        public void Smooth(List<Pair> modifiedVerts) {
            Vector3[] vertices = meshFilter.sharedMesh.vertices;

            for (int i = 0; i < modifiedVerts.Count; i++) {
                vertices[modifiedVerts[i].id] = modifiedVerts[i].value - transform.localPosition;
            }

            meshFilter.sharedMesh.vertices = vertices;
            meshFilter.sharedMesh.RecalculateNormals();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public struct Pair {
            public int id;
            public Vector3 value;
            public Pair(int id, Vector3 value) {
                this.id = id;
                this.value = value;
            }
        }
        #endregion

        #region Generate mesh
        private TriangleNet.Mesh GenerateTriNetMesh() {
            Polygon polygon = new Polygon();
            List<Vector2> points = PointGenerator.GeneratePoints(meshTerrain.seperationDst, meshTerrain.chunkSize, 30);

            if (points != null) {
                foreach (Vector2 point in points) {
                    polygon.Add(new Vertex(point.x, point.y));
                }
            }
            ConstraintOptions constraints = new ConstraintOptions();
            constraints.ConformingDelaunay = false;
            constraints.Convex = false;

            QualityOptions qualityOptions = new QualityOptions();
            qualityOptions.MinimumAngle = 15;

            return (polygon.Triangulate(constraints, qualityOptions) as TriangleNet.Mesh);
        }

        private void GetEdges(List<Vector3> vertices) {
            Vector3 chunkSize = new Vector3((meshTerrain.chunkSize).x, 0, (meshTerrain.chunkSize).x);
            List<int> edgeLeftList = new List<int>(); // -x edge
            List<int> edgeRightList = new List<int>(); // x edge
            List<int> edgeFrontList = new List<int>(); // z edge
            List<int> edgeBackList = new List<int>(); // -z edge
            
            for (int i = 0; i < vertices.Count; i++) {
                if (vertices[i].x == 0)
                    edgeLeftList.Add(i);

                if (vertices[i].x >= chunkSize.x)
                    edgeRightList.Add(i);

                if (vertices[i].z == 0)
                    edgeBackList.Add(i);
                
                if (vertices[i].z >= chunkSize.z)
                    edgeFrontList.Add(i);
            }
            
            SortEdge(edgeLeftList, true);
            SortEdge(edgeRightList, true);
            SortEdge(edgeFrontList, false);
            SortEdge(edgeBackList, false);
            
            for (int i = 0; i < edgeRightList.Count; i++) {
                Debug.Log(vertices[edgeRightList[i]].z);
            }

            void SortEdge(List<int> edge, bool sortByZ) {
                bool nothingChnaged = false;
                while (!nothingChnaged) {
                    nothingChnaged = true;

                    if (sortByZ) {
                        for (int i = 0; i < edge.Count - 1; i++) {
                            if (vertices[edge[i]].z > vertices[edge[i + 1]].z) {
                                int tmp = edge[i + 1];
                                edge[i + 1] = edge[i];
                                edge[i] = tmp;
                                nothingChnaged = false;
                            }
                        }
                    }
                    else { // Sort by x
                        for (int i = 0; i < edge.Count - 1; i++) {
                            if (vertices[edge[i]].x > vertices[edge[i + 1]].x) {
                                int tmp = edge[i + 1];
                                edge[i + 1] = edge[i];
                                edge[i] = tmp;
                                nothingChnaged = false;
                            }
                        }
                    }
                }
            }
            
            edgeLeft = edgeLeftList.ToArray();
            edgeRight = edgeRightList.ToArray();
            edgeFront = edgeFrontList.ToArray();
            edgeBack = edgeBackList.ToArray();
        }
        
        private List<Vector3> NoisifyEdges(List<Vector3> vertices) {

            for (int i = 0; i < edgeFront.Length; i++) {
                Vector3 noise = new Vector3();
                noise.x = (Mathf.PerlinNoise(vertices[edgeFront[i]].x * -91.43f, vertices[edgeFront[i]].x * -0.43f) - 0.5f) * meshTerrain.seperationDst;
                noise.z = (Mathf.PerlinNoise(vertices[edgeFront[i]].x * 5.43f, vertices[edgeFront[i]].x * -547.43f) - 0.5f) * meshTerrain.seperationDst;
                vertices[edgeFront[i]] += noise;
            }
            
            for (int i = 0; i < edgeRight.Length; i++) {
                Vector3 noise = new Vector3();
                noise.x = (Mathf.PerlinNoise(vertices[edgeRight[i]].z * -91.43f, vertices[edgeRight[i]].z * -0.43f) - 0.5f) * meshTerrain.seperationDst;
                noise.z = (Mathf.PerlinNoise(vertices[edgeRight[i]].z * 5.43f, vertices[edgeRight[i]].z * -547.43f) - 0.5f) * meshTerrain.seperationDst;
                vertices[edgeRight[i]] += noise;
            }
            
            for (int i = 0; i < edgeBack.Length; i++) {
                Vector3 noise = new Vector3();
                noise.x = (Mathf.PerlinNoise(vertices[edgeBack[i]].x * -91.43f, vertices[edgeBack[i]].x * -0.43f) - 0.5f) * meshTerrain.seperationDst;
                noise.z = (Mathf.PerlinNoise(vertices[edgeBack[i]].x * 5.43f, vertices[edgeBack[i]].x * -547.43f) - 0.5f) * meshTerrain.seperationDst;
                vertices[edgeBack[i]] += noise;
            }
            
            for (int i = 0; i < edgeLeft.Length; i++) {
                Vector3 noise = new Vector3();
                noise.x = (Mathf.PerlinNoise(vertices[edgeLeft[i]].z * -91.43f, vertices[edgeLeft[i]].z * -0.43f) - 0.5f) * meshTerrain.seperationDst;
                noise.z = (Mathf.PerlinNoise(vertices[edgeLeft[i]].z * 5.43f, vertices[edgeLeft[i]].z * -547.43f) - 0.5f) * meshTerrain.seperationDst;
                vertices[edgeLeft[i]] += noise;
            }
            
            return vertices;
        }

        private void ConvertToUnityMesh(TriangleNet.Mesh triMesh) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color32> colors32 = new List<Color32>();

            foreach (Triangle triangle in triMesh.Triangles) {
                Vector3 v1 = new Vector3((float)triangle.GetVertex(2).X, 0, (float)triangle.GetVertex(2).Y);
                Vector3 v2 = new Vector3((float)triangle.GetVertex(1).X, 0, (float)triangle.GetVertex(1).Y);
                Vector3 v3 = new Vector3((float)triangle.GetVertex(0).X, 0, (float)triangle.GetVertex(0).Y);

                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);

                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);

                for (int i = 0; i < 3; i++) {
                    colors32.Add(meshTerrain.defaultColor);
                }
            }

            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();
            unityMesh.vertices = vertices.ToArray();
            unityMesh.triangles = triangles.ToArray();
            unityMesh.colors32 = colors32.ToArray();
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateTangents();
            unityMesh.RecalculateBounds();
            meshFilter.sharedMesh = unityMesh;
            meshCollider.sharedMesh = unityMesh;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void ConvertToUnityMeshNoDuplicate(TriangleNet.Mesh triMesh) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color32> colors32 = new List<Color32>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (Triangle triangle in triMesh.Triangles) {
                Vector3 v1 = new Vector3((float)triangle.GetVertex(2).X, 0, (float)triangle.GetVertex(2).Y);
                Vector3 v2 = new Vector3((float)triangle.GetVertex(1).X, 0, (float)triangle.GetVertex(1).Y);
                Vector3 v3 = new Vector3((float)triangle.GetVertex(0).X, 0, (float)triangle.GetVertex(0).Y);

                int index1;
                int index2;
                int index3;

                if (vertices.Contains(v1)) {
                    index1 = vertices.IndexOf(v1);
                    triangles.Add(index1);
                }
                else {
                    vertices.Add(v1);
                    triangles.Add(vertices.Count - 1);
                    uvs.Add(new Vector2(v1.x / meshTerrain.chunkSize.x, v1.z / meshTerrain.chunkSize.y));
                    colors32.Add(meshTerrain.defaultColor);
                }

                if (vertices.Contains(v2)) {
                    index2 = vertices.IndexOf(v2);
                    triangles.Add(index2);
                }
                else {
                    vertices.Add(v2);
                    triangles.Add(vertices.Count - 1);
                    uvs.Add(new Vector2(v2.x / meshTerrain.chunkSize.x, v2.z / meshTerrain.chunkSize.y));
                    colors32.Add(meshTerrain.defaultColor);
                }

                if (vertices.Contains(v3)) {
                    index3 = vertices.IndexOf(v3);
                    triangles.Add(index3);
                }
                else {
                    vertices.Add(v3);
                    triangles.Add(vertices.Count - 1);
                    uvs.Add(new Vector2(v3.x / meshTerrain.chunkSize.x, v3.z / meshTerrain.chunkSize.y));
                    colors32.Add(meshTerrain.defaultColor);
                }
            }
            
            GetEdges(vertices);
            vertices = NoisifyEdges(vertices);

            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();
            unityMesh.vertices = vertices.ToArray();
            unityMesh.triangles = triangles.ToArray();
            unityMesh.colors32 = colors32.ToArray();
            unityMesh.uv = uvs.ToArray();
            unityMesh.RecalculateNormals();
            unityMesh.RecalculateTangents();
            unityMesh.RecalculateBounds(); 
            
            meshFilter.mesh = unityMesh;
            meshFilter.sharedMesh.MarkModified();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        #endregion
    }
}
#endif
