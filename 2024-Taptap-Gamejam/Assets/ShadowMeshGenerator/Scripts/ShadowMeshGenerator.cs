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

public class ShadowMeshGenerator : MonoBehaviour
{
    public UniversalRendererData UniversalRendererData;
    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public bool showNormals = false;

    private int pointCount;
    private Vector3[] pointDataArray;
    private float[] boundingBox = new float[6];
    private ShadowMaskRenderFeature shadowMaskRenderFeature;
    private GameObject existingShadowMesh;

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
    public void ShadowCatch() 
    {
        if (shadowMaskRenderFeature != null)
        {
            shadowMaskRenderFeature.EnableShadowCatching();
        }
    }
    public void ToGenerateShadowMeshHandler(int pointCount, Vector3[] pointDataArray)
    {
        this.pointCount = pointCount;
        this.pointDataArray = pointDataArray;

        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        Marching marching = null;
        if(mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingTertrahedron();
        else
            marching = new MarchingCubes();

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        marching.Surface = 0.0f;

        int width = 128, height = 128, depth = 128; //The size of voxel array.
        var voxels = new VoxelArray(width, height, depth);
        CalculateBoundingBox();
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

    private void CalculateBoundingBox()
    {
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = this.pointDataArray[i];
            this.boundingBox[0] = worldPos.x < this.boundingBox[0] ? worldPos.x : this.boundingBox[0];
            this.boundingBox[1] = worldPos.y < this.boundingBox[1] ? worldPos.y : this.boundingBox[1];
            this.boundingBox[2] = worldPos.z < this.boundingBox[2] ? worldPos.z : this.boundingBox[2];
            this.boundingBox[3] = worldPos.x > this.boundingBox[3] ? worldPos.x : this.boundingBox[3];
            this.boundingBox[4] = worldPos.y > this.boundingBox[4] ? worldPos.y : this.boundingBox[4];
            this.boundingBox[5] = worldPos.z > this.boundingBox[5] ? worldPos.z : this.boundingBox[5];
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
        existingShadowMesh.transform.localPosition = position;
        existingShadowMesh.AddComponent<MeshFilter>().mesh = mesh;
        existingShadowMesh.AddComponent<MeshCollider>().sharedMesh = mesh;
        existingShadowMesh.GetComponent<MeshCollider>().convex = true;

        existingShadowMesh.AddComponent<InteractableObject>().enableInteraction = true;
        existingShadowMesh.AddComponent<ShadowMeshController>().Appear(mesh);

        existingShadowMesh.AddComponent<Rigidbody>();
        existingShadowMesh.GetComponent<Rigidbody>().isKinematic = true;
        existingShadowMesh.layer = LayerMask.GetMask("ShadowMesh");
        //existingShadowMesh.AddComponent<PlatformInteractor>();

        //existingShadowMesh.GetComponent<PlatformInteractor>().interactionRange = 2.0f;
        //existingShadowMesh.GetComponent<PlatformInteractor>().interactableObj = transform.gameObject;
        //existingShadowMesh.GetComponent<PlatformInteractor>().playerObj = platform.GetComponent<PlatformInteractor>().playerObj;
        //existingShadowMesh.GetComponent<PlatformInteractor>().playerManager = platform.GetComponent<PlatformInteractor>().playerManager;
        //existingShadowMesh.GetComponent<PlatformInteractor>().ui = platform.GetComponent<PlatformInteractor>().ui;
    }
}
