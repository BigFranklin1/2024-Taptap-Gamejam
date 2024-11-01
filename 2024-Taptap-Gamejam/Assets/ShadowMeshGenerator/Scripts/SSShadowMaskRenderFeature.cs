using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using System;

public class ShadowMaskRenderFeature : ScriptableRendererFeature
{
    class ShadowMaskRenderPass : ScriptableRenderPass
    {
        #if UNITY_WEBGL
            public event Action<Texture2D> ToGenerateShadowMesh_WebGL;
        #else
            public event Action<int, Vector3[], ShadowState> ToGenerateShadowMesh;
        #endif

        private RTHandle maskRTHandle;
        private ShaderTagId shaderTagId;
        private FilteringSettings filteringSettings;

        //private RTHandle pointsRTHandle;
        private ComputeShader shadowPointsExtractionShader;
        private ComputeBuffer pointBuffer;
        private ComputeBuffer pointCountBuffer;
        private GraphicsFence fence;
        private ShadowMeshGenerator shadowMeshGenerator;

        public ShadowMaskRenderPass(ComputeShader shadowPointsExtractionShader)
        {
            //this.maskRTHandle = maskRTHandle;
            this.shaderTagId = new ShaderTagId("SSShadowMask");
            this.filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            //this.pointsRTHandle = pointsRTHandle;
            this.shadowPointsExtractionShader = shadowPointsExtractionShader;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (this.maskRTHandle == null)
            {
                this.maskRTHandle = RTHandles.Alloc(
                    width: (int)(Screen.width * 1.5f),
                    height: (int)(Screen.height * 1.5f),
                    depthBufferBits: DepthBits.None,
                    colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                    //enableRandomWrite: true,
                    name: "SSShadowMask"
                );
            }
            else if (this.maskRTHandle.rt.width != (int)(Screen.width * 1.5f) || this.maskRTHandle.rt.height != (int)(Screen.height * 1.5f))
            {
                this.maskRTHandle.Release();
                this.maskRTHandle = RTHandles.Alloc(
                    width: (int)(Screen.width * 1.5f),
                    height: (int)(Screen.height * 1.5f),
                    depthBufferBits: DepthBits.None,
                    colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                    //enableRandomWrite: true,
                    name: "SSShadowMask"
                );
            }
            ConfigureTarget(this.maskRTHandle);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("ShadowMaskRenderPass");

            var drawingSettings = CreateDrawingSettings(this.shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            context.Submit();

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            fence = cmd.CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation, SynchronisationStageFlags.PixelProcessing);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            OnRenderPassComplete(context);
        }

        private void OnRenderPassComplete(ScriptableRenderContext context)
        {
            if (this.shadowMeshGenerator == null)
            {
                this.shadowMeshGenerator = GameObject.FindWithTag("MeshGenerator").GetComponent<ShadowMeshGenerator>();
                if (this.shadowMeshGenerator == null)
                    Debug.LogError("MeshGenerator not found in the scene.");
            }

            #if UNITY_WEBGL
                this.ToGenerateShadowMesh_WebGL += this.shadowMeshGenerator.ToGenerateShadowMeshHandler;

                Texture2D shadowMaskTex = new Texture2D(this.maskRTHandle.rt.width, this.maskRTHandle.rt.height, TextureFormat.RGBAHalf, false);
                RenderTexture.active = this.maskRTHandle.rt;
                shadowMaskTex.ReadPixels(new Rect(0, 0, this.maskRTHandle.rt.width, this.maskRTHandle.rt.height), 0, 0);
                shadowMaskTex.Apply();
                RenderTexture.active = null;

                this.ToGenerateShadowMesh_WebGL?.Invoke(shadowMaskTex);

                this.ToGenerateShadowMesh_WebGL -= this.shadowMeshGenerator.ToGenerateShadowMeshHandler;
            #else
                this.ToGenerateShadowMesh += this.shadowMeshGenerator.ToGenerateShadowMeshHandler;

                ShadowPointsExtraction(context);
                int[] pointCountArray = new int[1];
                this.pointCountBuffer.GetData(pointCountArray);
                int pointCount = pointCountArray[0];

                ShadowState state = ShadowState.Normal;
                if (pointCount == 0)
                {
                    Debug.LogWarning("No points found.");
                    state = ShadowState.None;
                }
                Debug.Log("Point count: " + pointCount);
                Vector3[] pointDataArray = new Vector3[pointCount];
                try
                {
                    this.pointBuffer.GetData(pointDataArray);
                }
                catch (ArgumentException e)
                {
                    Debug.Log($"Shadow size out of buffer limit: {e.Message}");
                    state = ShadowState.Oversize;
                }

                this.ToGenerateShadowMesh?.Invoke(pointCount, pointDataArray, state);

                ReleaseBuffers();
                this.ToGenerateShadowMesh -= this.shadowMeshGenerator.ToGenerateShadowMeshHandler;
            #endif

            //AsyncGPUReadback.Request(this.pointCountBuffer, (AsyncGPUReadbackRequest request) =>
            //{
            //    if (request.hasError)
            //    {
            //        Debug.LogError("GPU readback error.");
            //        return;
            //    }

            //    int[] pointCountArray = request.GetData<int>().ToArray();
            //    int pointCount = pointCountArray[0];

            //    if (pointCount == 0)
            //    {
            //        Debug.LogWarning("No points found.");
            //        return;
            //    }

            //    Debug.Log("Point count: " + pointCount);
            //    Vector3[] pointDataArray = new Vector3[pointCount];
            //    this.pointBuffer.GetData(pointDataArray);

            //    this.ToGenerateShadowMesh?.Invoke(pointCount, pointDataArray);

            //    ReleaseBuffers();
            //});
        }

        private void ShadowPointsExtraction(ScriptableRenderContext context)
        {
            this.pointBuffer = new ComputeBuffer(200000, sizeof(float) * 3, ComputeBufferType.Append);
            this.pointCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            CommandBuffer cmd = CommandBufferPool.Get("ShadowPointsExtraction");
            cmd.WaitOnAsyncGraphicsFence(fence);

            this.pointBuffer.SetCounterValue(0);

            int kernelHandle = this.shadowPointsExtractionShader.FindKernel("CSShadowPointsExtraction");

            cmd.SetComputeTextureParam(this.shadowPointsExtractionShader, kernelHandle, "Input", this.maskRTHandle.rt);
            //cmd.SetComputeTextureParam(this.shadowPointsExtractionShader, kernelHandle, "Result", this.pointsRTHandle.rt);
            cmd.SetComputeBufferParam(this.shadowPointsExtractionShader, kernelHandle, "ShadowPoints", this.pointBuffer);

            cmd.DispatchCompute(this.shadowPointsExtractionShader, kernelHandle, this.maskRTHandle.rt.width / 8, this.maskRTHandle.rt.height / 8, 1);

            cmd.CopyCounterValue(this.pointBuffer, this.pointCountBuffer, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }

        private void ReleaseBuffers()
        {
            this.pointBuffer?.Release();
            this.pointCountBuffer?.Release();
        }

        public void Cleanup()
        {
            this.maskRTHandle?.Release();
        }
    }

    public ComputeShader shadowPointsExtractionShader;
    //public RenderTexture SSShadowMaskRenderTexture;
    //public RenderTexture SSShadowPointsRenderTexture;

    private bool isEnabled = false;
    private ShadowMaskRenderPass renderPass;
    private RTHandle maskRTHandle;
    //private RTHandle pointsRTHandle; // for visualization only

    public override void Create()
    {
        //GraphicsFormat rtColorFormat = GraphicsFormat.R16G16B16A16_SFloat;
        //GraphicsFormat rtColorFormat = GraphicsFormat.R32G32B32A32_SFloat;

        //this.maskRTHandle = RTHandles.Alloc(SSShadowMaskRenderTexture);
        //this.pointsRTHandle = RTHandles.Alloc(SSShadowPointsRenderTexture);

        this.renderPass = new ShadowMaskRenderPass(this.shadowPointsExtractionShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (isEnabled)
        {
            renderer.EnqueuePass(renderPass);
            isEnabled = false;
        }
    }
    public void EnableShadowCatching() 
    { 
        isEnabled = true;
    }

    void OnDisable()
    {
        this.renderPass.Cleanup();
        //if (maskRTHandle != null)
        //{
        //    maskRTHandle.Release();
        //}
        //if (edgeRTHandle != null)
        //{
        //    edgeRTHandle.Release();
        //}
    }
}
