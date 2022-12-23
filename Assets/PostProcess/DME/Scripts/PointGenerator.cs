#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DME {
    public class PointGenerator : MonoBehaviour {
        public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamlpleBeforeRejection = 30) {
            List<Vector2> edgePoints = GenerateEdgePoints(radius, sampleRegionSize);

            float cellSize = radius / Mathf.Sqrt(2);

            int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            for (int i = 0; i < edgePoints.Count; i++) {
                Vector2 candidate = edgePoints[i];
                points.Add(candidate);
                spawnPoints.Add(candidate);
                grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
            }

            spawnPoints.Add(sampleRegionSize / 2);
            while (spawnPoints.Count > 0) {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                Vector2 spawnCenter = spawnPoints[spawnIndex];
                bool candidateAccepted = false;

                for (int i = 0; i < numSamlpleBeforeRejection; i++) {
                    float angle = Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCenter + dir * Random.Range(radius, 2 * radius);
                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted) {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }

            return points;
        }

        static List<Vector2> GenerateEdgePoints(float seperationDst, Vector2 chunkSize) {
            List<Vector2> edgePoints = new List<Vector2>();
            Vector2 currentPoint = Vector2.zero;
            while (currentPoint.y < chunkSize.y) {
                edgePoints.Add(currentPoint);
                currentPoint.y += seperationDst;
            }
            while (currentPoint.x < chunkSize.x) {
                edgePoints.Add(currentPoint);
                currentPoint.x += seperationDst;
            }
            while (currentPoint.y > 0) {
                edgePoints.Add(currentPoint);
                currentPoint.y -= seperationDst;
            }
            while (currentPoint.x > 0) {
                edgePoints.Add(currentPoint);
                currentPoint.x -= seperationDst;
            }
            return edgePoints;
        }

        static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
            if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) {
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);
                int searchStartX = Mathf.Max(0, cellX - 2);
                int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - 2);
                int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++) {
                    for (int y = searchStartY; y <= searchEndY; y++) {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1) {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                            if (sqrDst < radius * radius) {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

    }
}
#endif