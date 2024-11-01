using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Common.Unity.Drawing;

using System.Linq;
// using UnityEditor.UI;
using UnityEngine.Rendering.Universal;
// using UnityEditor.VersionControl;

using MarchingCubesProject;

using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

public enum ShadowState
{
    None,
    Normal,
    Oversize
}

public class ShadowMeshGenerator : MonoBehaviour
{
    public UniversalRendererData UniversalRendererData;
    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public bool showNormals = false;
    public GameObject shadowExtremeCaseUI;
    public event Action<GameObject> HasGeneratedShadow;

    private int pointCount;
    private Vector3[] pointDataArray;
    private float[] boundingBox = new float[6];
    private ShadowMaskRenderFeature shadowMaskRenderFeature;
    private GameObject existingShadowMesh;
    private AudioSource AppearSound;
    private ShadowExtremeCaseUIHandler shadowExtremeCaseUIHandler;

    // interaction level
    //public GameObject platform;
    void Start()
    {
        foreach (var feature in UniversalRendererData.rendererFeatures)
        {
            if (feature is ShadowMaskRenderFeature shadowMaskRF)
            {
                shadowMaskRenderFeature = shadowMaskRF;
            }
        }
        AppearSound = GetComponent<AudioSource>();
        shadowExtremeCaseUIHandler = shadowExtremeCaseUI.GetComponent<ShadowExtremeCaseUIHandler>();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    if (shadowMaskRenderFeature != null)
        //    {
        //        shadowMaskRenderFeature.EnableShadowCatching();
        //    }
        //}
    }
    public void CatchShadow() 
    {
        if (shadowMaskRenderFeature != null)
        {
            shadowMaskRenderFeature.EnableShadowCatching();
        }
    }

    public void ToGenerateShadowMeshHandler(Texture2D shadowMaskTex)
    {
        Color[] pixels = shadowMaskTex.GetPixels();

        //ConcurrentBag<Vector3> pointPositions = new ConcurrentBag<Vector3>();
        //Parallel.For(0, pixels.Length, i =>
        //{
        //    Color pixel = pixels[i];
        //    if (pixel.a > 0)
        //    {
        //        pointPositions.Add(new Vector3(pixel.r, pixel.g, pixel.b));
        //    }
        //});

        List<Vector3> pointPositions = new List<Vector3>();
        this.boundingBox = new float[] { float.MaxValue, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue, float.MinValue };
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            if (pixel.a > 0)
            {
                Vector3 worldPos = new Vector3(pixel.r, pixel.g, pixel.b);
                pointPositions.Add(worldPos);

                this.boundingBox[0] = Mathf.Min(this.boundingBox[0], worldPos.x);
                this.boundingBox[1] = Mathf.Min(this.boundingBox[1], worldPos.y);
                this.boundingBox[2] = Mathf.Min(this.boundingBox[2], worldPos.z);

                this.boundingBox[3] = Mathf.Max(this.boundingBox[3], worldPos.x);
                this.boundingBox[4] = Mathf.Max(this.boundingBox[4], worldPos.y);
                this.boundingBox[5] = Mathf.Max(this.boundingBox[5], worldPos.z);
            }
        }

        Vector3[] pointPositionsArray = pointPositions.ToArray();
        int pointCount = pointPositionsArray.Length;
        ShadowState state = ShadowState.Normal;
        if (pointCount == 0)
        {
            Debug.LogWarning("No points found.");
            state = ShadowState.None;
        }
        Debug.Log("Point count: " + pointCount);
        if (pointCount > 200000)
        {
            state = ShadowState.Oversize;
        }

        BoundingBoxCheck();
        ToGenerateShadowMeshHandler(pointPositionsArray.Length, pointPositionsArray, state);
    }

    public void ToGenerateShadowMeshHandler(int pointCount, Vector3[] pointDataArray, ShadowState shadowState)
    {
        shadowExtremeCaseUIHandler.SwitchUI(shadowState);

        if (shadowState == ShadowState.Normal)
        {
            this.pointCount = pointCount;
            this.pointDataArray = pointDataArray;

            #if UNITY_WEBGL
            #else
                CalculateBoundingBox();
            #endif

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if (mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            int width = 128, height = 128, depth = 128; //The size of voxel array.
            var voxels = new VoxelArray(width, height, depth);
            foreach (Vector3 point in this.pointDataArray)
            {
                int x = Mathf.FloorToInt((point.x - this.boundingBox[0]) / (this.boundingBox[3] - this.boundingBox[0]) * (width - 1));
                int y = Mathf.FloorToInt((point.y - this.boundingBox[1]) / (this.boundingBox[4] - this.boundingBox[1]) * (height - 1));
                int z = Mathf.FloorToInt((point.z - this.boundingBox[2]) / (this.boundingBox[5] - this.boundingBox[2]) * (depth - 1));

                voxels[x, y, z] = 1.0f;
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //Create the normals from the voxel.
            if (showNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }
            }

            Vector3 pivotOffset = new Vector3(
                (this.boundingBox[0] + this.boundingBox[3]) / 2,
                (this.boundingBox[1] + this.boundingBox[4]) / 2,
                (this.boundingBox[2] + this.boundingBox[5]) / 2
            );

            for (int i = 0; i < verts.Count; i++)
            {
                Vector3 v = verts[i];
                v.x = Mathf.Lerp(this.boundingBox[0], this.boundingBox[3], v.x / (float)(width - 1));
                v.y = Mathf.Lerp(this.boundingBox[1], this.boundingBox[4], v.y / (float)(height - 1));
                v.z = Mathf.Lerp(this.boundingBox[2], this.boundingBox[5], v.z / (float)(depth - 1));
                verts[i] = v - pivotOffset;
            }

            CreateShadowMesh(verts, normals, indices, pivotOffset);
        }
    }

    private void CalculateBoundingBox()
    {
        this.boundingBox = new float[] { float.MaxValue, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue, float.MinValue };
        
        if (pointCount > 5000)
        {
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            Parallel.ForEach(Partitioner.Create(0, pointCount), range =>
            {
                float localMinX = float.MaxValue, localMinY = float.MaxValue, localMinZ = float.MaxValue;
                float localMaxX = float.MinValue, localMaxY = float.MinValue, localMaxZ = float.MinValue;

                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Vector3 worldPos = this.pointDataArray[i];
                    localMinX = Mathf.Min(localMinX, worldPos.x);
                    localMinY = Mathf.Min(localMinY, worldPos.y);
                    localMinZ = Mathf.Min(localMinZ, worldPos.z);
                    localMaxX = Mathf.Max(localMaxX, worldPos.x);
                    localMaxY = Mathf.Max(localMaxY, worldPos.y);
                    localMaxZ = Mathf.Max(localMaxZ, worldPos.z);
                }

                Interlocked.Exchange(ref minX, Mathf.Min(minX, localMinX));
                Interlocked.Exchange(ref minY, Mathf.Min(minY, localMinY));
                Interlocked.Exchange(ref minZ, Mathf.Min(minZ, localMinZ));
                Interlocked.Exchange(ref maxX, Mathf.Max(maxX, localMaxX));
                Interlocked.Exchange(ref maxY, Mathf.Max(maxY, localMaxY));
                Interlocked.Exchange(ref maxZ, Mathf.Max(maxZ, localMaxZ));
            });

            this.boundingBox[0] = minX;
            this.boundingBox[1] = minY;
            this.boundingBox[2] = minZ;
            this.boundingBox[3] = maxX;
            this.boundingBox[4] = maxY;
            this.boundingBox[5] = maxZ;
        }
        else
        {
            for (int i = 0; i < pointCount; i++)
            {
                Vector3 worldPos = this.pointDataArray[i];

                this.boundingBox[0] = Mathf.Min(this.boundingBox[0], worldPos.x);
                this.boundingBox[1] = Mathf.Min(this.boundingBox[1], worldPos.y);
                this.boundingBox[2] = Mathf.Min(this.boundingBox[2], worldPos.z);

                this.boundingBox[3] = Mathf.Max(this.boundingBox[3], worldPos.x);
                this.boundingBox[4] = Mathf.Max(this.boundingBox[4], worldPos.y);
                this.boundingBox[5] = Mathf.Max(this.boundingBox[5], worldPos.z);
            }
        }

        BoundingBoxCheck();
    }

    private void BoundingBoxCheck()
    {
        float epsilon = 1e-3f;
        if (Mathf.Approximately(this.boundingBox[3], this.boundingBox[0]))
        {
            this.boundingBox[0] -= epsilon;
            this.boundingBox[3] += epsilon;
        }
        if (Mathf.Approximately(this.boundingBox[4], this.boundingBox[1]))
        {
            this.boundingBox[1] -= epsilon;
            this.boundingBox[4] += epsilon;
        }
        if (Mathf.Approximately(this.boundingBox[5], this.boundingBox[2]))
        {
            this.boundingBox[2] -= epsilon;
            this.boundingBox[5] += epsilon;
        }
    }

    private void CreateShadowMesh(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);

        if (showNormals)
        {
            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();
        }

        mesh.RecalculateBounds();

        if (existingShadowMesh != null)
            existingShadowMesh.GetComponent<ShadowMeshController>().Destroy();

        existingShadowMesh = new GameObject("Generated Shadow Mesh");
        existingShadowMesh.transform.position = position;
        existingShadowMesh.AddComponent<MeshFilter>().mesh = mesh;

        existingShadowMesh.AddComponent<MeshCollider>().sharedMesh = mesh;
        existingShadowMesh.GetComponent<MeshCollider>().convex = true;
        existingShadowMesh.AddComponent<InteractableObject>().enableInteraction = true;
        existingShadowMesh.AddComponent<Rigidbody>();
        existingShadowMesh.GetComponent<Rigidbody>().isKinematic = true;
        existingShadowMesh.layer = LayerMask.NameToLayer("ShadowMesh");

        HasGeneratedShadow?.Invoke(existingShadowMesh);
        AppearSound.Play();
        existingShadowMesh.AddComponent<ShadowMeshController>().Appear(mesh);
    }
}
