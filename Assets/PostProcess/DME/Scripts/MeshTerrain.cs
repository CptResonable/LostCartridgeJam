#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DME {
    [ExecuteInEditMode]
    public class MeshTerrain : MonoBehaviour {
        
        public enum DirEnum { up, right, down, left}
        
        [SerializeField] public bool flatShaded = true;
        [SerializeField] GameObject chunkPrefab;
        [SerializeField] public Vector2 chunkSize;
        [SerializeField] public float seperationDst;
        [SerializeField] public Color32 defaultColor;
        [SerializeField] public Material defaultMaterial;

        [SerializeField] public List<Vector2Int> chunkCoords;
        [SerializeField] public List<Chunk> chunks;
        [SerializeField] public List<Corner> corners = new List<Corner>();
        [SerializeField] public List<Vector2Int> cornerIndexes = new List<Vector2Int>();

        public void Initialize(string objectName, Vector2Int chunkSize, float vertexDensity, bool flatShaded) {
            this.chunkSize = chunkSize;
            this.seperationDst = 1 / vertexDensity;
            this.flatShaded = flatShaded;
            
            string filePath = EditorUtility.OpenFolderPanel("Save terrain object to folder", "Assets", "");
            
            // Trim file path to only the project path
            if (!filePath.Contains("Assets")) {
                Debug.LogError("You can only select a folder inside your project!");
                return;
            }
            filePath = filePath.Substring(filePath.IndexOf("Assets"));

            // Create object folder
            AssetDatabase.CreateFolder(filePath, name);
            filePath += "/" + name;

            try {
                
                // Create prefab
                bool saveSuccess;
                PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, filePath + "/" + name + ".prefab", InteractionMode.AutomatedAction, out saveSuccess);

                // Destory gameObject if save fails.
                if (!saveSuccess) {
                    Debug.LogError("Saving terrain object prefab failed!");
                    DestroyImmediate(gameObject);
                }
            }
            catch (Exception e) { // Destory gameObject if save fails
                Debug.LogError(e);
                DestroyImmediate(gameObject);
            }
            
            // Create folder for mesh chunks.
            AssetDatabase.CreateFolder(filePath, "Meshes");
        }
        
        private void OnEnable() {

            // Get chunks
            chunks = new List<Chunk>();
            chunkCoords = new List<Vector2Int>();
            for (int i = 0; i < transform.childCount; i++) {
                Chunk chunk = transform.GetChild(i).GetComponent<Chunk>();
                chunks.Add(chunk);
                chunkCoords.Add(chunk.coordinates);
            }

            Brush.brushEvent -= OnBrushEvent;
            Brush.brushEvent += OnBrushEvent;
        }
        
        private void OnDestroy() {
            Brush.brushEvent -= OnBrushEvent;
        }

        private void OnBrushEvent(Brush.Settings settings, Vector3 position, Vector3 normal) {
            if (settings.isSmoothing)
                Smooth(settings, position);
        }

        private void Smooth(Brush.Settings settings, Vector3 position) {

            List<int> chunkIndexes = new List<int>();
            List<List<Chunk.Pair>> allVertsInside = new List<List<Chunk.Pair>>();

            // Adjust pos for meshTerrain position.
            position -= GetPos();

            // Get verts inside brush for all chunks.
            List<Vector3> allVertsInsideCombined = new List<Vector3>();
            for (int i = 0; i < chunks.Count; i++) {


                // Return if chunk not affected.
                bool isAffected = false;
                if ((position.x > chunks[i].chunkPosV2.x - settings.radius && position.x < chunks[i].chunkPosV2.x + chunkSize.x + settings.radius)) {
                    if ((position.z > chunks[i].chunkPosV2.y - settings.radius && position.z < chunks[i].chunkPosV2.y + chunkSize.y + settings.radius))
                        isAffected = true;
                }

                if (isAffected) {
                    List<Chunk.Pair> vertsInside = chunks[i].GetVertsInside(settings, position);
                    if (vertsInside.Count > 0) {
                        for (int j = 0; j < vertsInside.Count; j++) {
                            allVertsInsideCombined.Add(vertsInside[j].value);
                        }
                    }
                    allVertsInside.Add(vertsInside);
                    chunkIndexes.Add(i);
                }
            }

            // Remove duplicates.
            List<Vector3> vertsInsideNoDup = new List<Vector3>();
            for (int i = 0; i < allVertsInsideCombined.Count; i++) {
                if (!vertsInsideNoDup.Contains(allVertsInsideCombined[i])) {
                    vertsInsideNoDup.Add(allVertsInsideCombined[i]);
                }
            }

            for (int i = 0; i < vertsInsideNoDup.Count; i++) {

                // Get closest point for every vert.
                List<Vector3> closestVerts = new List<Vector3>();
                for (int k = 0; k < vertsInsideNoDup.Count; k++) {
                    if (closestVerts.Count < 6)
                        closestVerts.Add(vertsInsideNoDup[k]);
                    else {
                        for (int l = 0; l < closestVerts.Count; l++) {
                            if (Vector3.Distance(vertsInsideNoDup[k], vertsInsideNoDup[i]) < Vector3.Distance(closestVerts[l], vertsInsideNoDup[i])) {
                                closestVerts[l] = vertsInsideNoDup[k];
                                break;
                            }
                        }
                    }
                }

                // Calculate avrage position from nearby points.
                Vector3 totalPos = Vector3.zero;
                totalPos += Vector3.zero;
                for (int k = 0; k < closestVerts.Count; k++) {
                    totalPos += closestVerts[k];
                }
                Vector3 avgPos = totalPos / closestVerts.Count;

                float dst = Vector3.Distance(vertsInsideNoDup[i], position);
                float t = Mathf.Pow((settings.radius - dst) / settings.radius, 1.5f) * settings.smoothingStrenth;

                for (int j = 0; j < allVertsInside.Count; j++) {
                    for (int k = 0; k < allVertsInside[j].Count; k++) {
                        if (allVertsInside[j][k].value == vertsInsideNoDup[i])
                            allVertsInside[j][k] = new Chunk.Pair(allVertsInside[j][k].id, new Vector3(allVertsInside[j][k].value.x, Mathf.Lerp(allVertsInside[j][k].value.y, avgPos.y, t), allVertsInside[j][k].value.z));
                    }
                }
            }

            // Send new verex positions to the chunks.
            for (int i = 0; i < chunkIndexes.Count; i++) {
                chunks[chunkIndexes[i]].Smooth(allVertsInside[i]);
            }
        }

        public void AddChunk(Vector2Int newChunkCoord) {
            if (chunkCoords.Contains(newChunkCoord)) {
                Debug.LogWarning("Add chunk failed: chunk already exist");
                return;
            }

            chunkCoords.Add(newChunkCoord);
            GameObject goNewChunk = Instantiate(chunkPrefab, transform);
            goNewChunk.name = newChunkCoord.ToString();
            Chunk newChunk = goNewChunk.GetComponent<Chunk>();
            //Debug.Log(newChunk);
            chunks.Add(newChunk);
            newChunk.Initiate(this, newChunkCoord);

            goNewChunk.transform.localPosition = new Vector3(newChunkCoord.x * chunkSize.x, 0, newChunkCoord.y * chunkSize.y);
            
            // Down left corner
            if (!cornerIndexes.Contains(newChunk.coordinates)) {
                corners.Add(new Corner());
                cornerIndexes.Add(newChunk.coordinates);
            }
            
            corners[cornerIndexes.IndexOf(newChunk.coordinates)].AddCorner(newChunk, newChunk.edgeLeft[0]);

            // Up left corner
            if (!cornerIndexes.Contains(newChunk.coordinates + new Vector2Int(0, 1))) {
                corners.Add(new Corner());
                cornerIndexes.Add(newChunk.coordinates + new Vector2Int(0, 1));
            }
            
            corners[cornerIndexes.IndexOf(newChunk.coordinates + new Vector2Int(0, 1))].AddCorner(newChunk, newChunk.edgeLeft.Last());
            
            // Up right corner
            if (!cornerIndexes.Contains(newChunk.coordinates + new Vector2Int(1, 1))) {
                corners.Add(new Corner());
                cornerIndexes.Add(newChunk.coordinates + new Vector2Int(1, 1));
            }

            corners[cornerIndexes.IndexOf(newChunk.coordinates + new Vector2Int(1, 1))].AddCorner(newChunk, newChunk.edgeRight.Last());
            
            // Down right corner
            if (!cornerIndexes.Contains(newChunk.coordinates + new Vector2Int(1, 0))) {
                corners.Add(new Corner());
                cornerIndexes.Add(newChunk.coordinates + new Vector2Int(1, 0));
            }

            corners[cornerIndexes.IndexOf(newChunk.coordinates + new Vector2Int(1, 0))].AddCorner(newChunk, newChunk.edgeRight[0]);
            SaveChunk(newChunk);
        }

        private void SaveChunk(Chunk chunk) {
            string filePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            filePath = FilePathUtils.StepBack(filePath) + "Meshes/" + chunk.coordinates + ".asset";
            AssetDatabase.CreateAsset(chunk.meshFilter.sharedMesh, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }

        public void SavePrefab() {
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }
        
        public void SmoothSeam(Chunk chunk1, Chunk chunk2, DirEnum dir) {
            Vector3[] normalsC1 = chunk1.meshFilter.sharedMesh.normals;
            Vector3[] normalsC2 = chunk2.meshFilter.sharedMesh.normals;
            Vector3[] verticesC1 = chunk1.meshFilter.sharedMesh.vertices;
            Vector3[] verticesC2 = chunk2.meshFilter.sharedMesh.vertices;

            switch (dir) {
                case DirEnum.up:
                    for (int i = 0; i < chunk1.edgeFront.Length; i++) {
                        Vector3 avgNormal = (chunk1.meshFilter.sharedMesh.normals[chunk1.edgeFront[i]] + chunk2.meshFilter.sharedMesh.normals[chunk2.edgeBack[i]]) / 2;
                        Vector3 avgPos = (chunk1.meshFilter.sharedMesh.vertices[chunk1.edgeFront[i]] + chunk1.transform.position
                            + chunk2.meshFilter.sharedMesh.vertices[chunk2.edgeBack[i]] + chunk2.transform.position)
                            / 2;
                    
                        normalsC1[chunk1.edgeFront[i]] = avgNormal;
                        normalsC2[chunk2.edgeBack[i]] = avgNormal;
                        verticesC1[chunk1.edgeFront[i]] = avgPos - chunk1.transform.position;
                        verticesC2[chunk2.edgeBack[i]] = avgPos - chunk2.transform.position;
                    }
                    break;
                case DirEnum.right:
                    for (int i = 0; i < chunk1.edgeRight.Length; i++) {
                        Vector3 avgNormal = (chunk1.meshFilter.sharedMesh.normals[chunk1.edgeRight[i]] + chunk2.meshFilter.sharedMesh.normals[chunk2.edgeLeft[i]]) / 2;
                        Vector3 avgPos = (chunk1.meshFilter.sharedMesh.vertices[chunk1.edgeRight[i]] + chunk1.transform.position
                            + chunk2.meshFilter.sharedMesh.vertices[chunk2.edgeLeft[i]] + chunk2.transform.position)
                            / 2;
                    
                        normalsC1[chunk1.edgeRight[i]] = avgNormal;
                        normalsC2[chunk2.edgeLeft[i]] = avgNormal;
                        verticesC1[chunk1.edgeRight[i]] = avgPos - chunk1.transform.position;
                        verticesC2[chunk2.edgeLeft[i]] = avgPos - chunk2.transform.position;
                    }
                    break;
                case DirEnum.down:
                    for (int i = 0; i < chunk1.edgeBack.Length; i++) {
                        Vector3 avgNormal = (chunk1.meshFilter.sharedMesh.normals[chunk1.edgeBack[i]] + chunk2.meshFilter.sharedMesh.normals[chunk2.edgeFront[i]]) / 2;
                        Vector3 avgPos = (chunk1.meshFilter.sharedMesh.vertices[chunk1.edgeBack[i]] + chunk1.transform.position
                            + chunk2.meshFilter.sharedMesh.vertices[chunk2.edgeFront[i]] + chunk2.transform.position)
                            / 2;
                    
                        normalsC1[chunk1.edgeBack[i]] = avgNormal;
                        normalsC2[chunk2.edgeFront[i]] = avgNormal;
                        verticesC1[chunk1.edgeBack[i]] = avgPos - chunk1.transform.position;
                        verticesC2[chunk2.edgeFront[i]] = avgPos - chunk2.transform.position;
                    }
                    break;
                case DirEnum.left:
                    for (int i = 0; i < chunk1.edgeLeft.Length; i++) {
                        Vector3 avgNormal = (chunk1.meshFilter.sharedMesh.normals[chunk1.edgeLeft[i]] + chunk2.meshFilter.sharedMesh.normals[chunk2.edgeRight[i]]) / 2;
                        Vector3 avgPos = (chunk1.meshFilter.sharedMesh.vertices[chunk1.edgeLeft[i]] + chunk1.transform.position
                            + chunk2.meshFilter.sharedMesh.vertices[chunk2.edgeRight[i]] + chunk2.transform.position)
                            / 2;

                        normalsC1[chunk1.edgeLeft[i]] = avgNormal;
                        normalsC2[chunk2.edgeRight[i]] = avgNormal;
                        verticesC1[chunk1.edgeLeft[i]] = avgPos - chunk1.transform.position;
                        verticesC2[chunk2.edgeRight[i]] = avgPos - chunk2.transform.position;
                    }
                    break;
            }

            chunk1.meshFilter.sharedMesh.vertices = verticesC1;
            chunk2.meshFilter.sharedMesh.vertices = verticesC2;
            chunk1.meshFilter.sharedMesh.normals = normalsC1;
            chunk2.meshFilter.sharedMesh.normals = normalsC2;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public void FixSeams() {
            for (int i = 0; i < chunks.Count; i++) {
                chunks[i].SmoothSeams();
            }

            for (int i = 0; i < corners.Count; i++) {
                corners[i].FixCorner();
            }
        }
        
        public Vector3 GetPos() {
            return transform.position;
        }
    }

    [Serializable]
    public class Corner {
        [SerializeField] private List<Chunk> chunks = new List<Chunk>();
        [SerializeField] private List<int> vertexIndexes = new List<int>();

        public void AddCorner(Chunk chunk, int vertexIndex) {
            if (chunks.Count > 3) {
                Debug.LogError("Corner already contains 4 vertices, this is should not be possible");
                return;
            }
            
            chunks.Add(chunk);
            vertexIndexes.Add(vertexIndex);
        }
        
        public void FixCorner() {
            List<Vector3[]> normals = new List<Vector3[]>();
            List<Vector3[]> vertices = new List<Vector3[]>();
            //List<Color32[]> colors = new List<Color32[]>();

            Vector3 avgPos = Vector3.zero;
            Vector3 avgNormal = Vector3.zero;
            // Vector4 avgColorV4 = new Vector4(0, 0, 0, 0);
            
            for (int i = 0; i < chunks.Count; i++) {
                normals.Add(chunks[i].meshFilter.sharedMesh.normals);
                vertices.Add(chunks[i].meshFilter.sharedMesh.vertices);
                //colors.Add(chunks[i].meshFilter.sharedMesh.colors32);

                avgPos += vertices[i][vertexIndexes[i]] + chunks[i].transform.position;
                avgNormal += normals[i][vertexIndexes[i]];
                // Vector4 colorV4 = new Vector4(colors[i][vertexIndexes[i]].r, colors[i][vertexIndexes[i]].g, colors[i][vertexIndexes[i]].b,colors[i][vertexIndexes[i]].a);
                // avgColorV4 += colorV4;
            }

            avgPos /= chunks.Count;
            avgNormal /= chunks.Count;
            //avgColorV4 /= chunks.Count;
            //Color32 avgColor = new Color32((byte)avgColorV4.x, (byte)avgColorV4.y, (byte)avgColorV4.z, (byte)avgColorV4.w);

            for (int i = 0; i < chunks.Count; i++) {
                vertices[i][vertexIndexes[i]] = avgPos - chunks[i].transform.position;
                normals[i][vertexIndexes[i]] = avgNormal;
                //colors[i][vertexIndexes[i]] = avgColor;
                //colors[i][vertexIndexes[i]] = new Color32(0, 0, 0, 0);
                
                chunks[i].meshFilter.sharedMesh.vertices = vertices[i];
                chunks[i].meshFilter.sharedMesh.normals = normals[i];
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
#endif