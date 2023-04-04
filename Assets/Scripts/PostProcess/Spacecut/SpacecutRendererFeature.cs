using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpacecutRendererFeature : ScriptableRendererFeature
{
    private SpacecutPass _spacecutPass;
    class SpacecutPass : ScriptableRenderPass
    {
        private Material _material;
        private RenderTargetIdentifier _src, _tint, _sceneDepth;
        private int _tintId = Shader.PropertyToID("_Temp");

        public SpacecutPass()
        {
            if (!_material)
            {
                _material = CoreUtils.CreateEngineMaterial("Raymarching/RayMarchingCut");
            }

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            _src = renderingData.cameraData.renderer.cameraColorTarget;
            _sceneDepth = renderingData.cameraData.renderer.cameraDepthTarget;
            cmd.GetTemporaryRT(_tintId, renderTextureDescriptor, FilterMode.Bilinear);
            _tint = new RenderTargetIdentifier(_tintId);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tintId);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            var camName = renderingData.cameraData.camera.name;
            if (camName.ToLower().Contains("spacecut") || camName.ToLower().Contains("scenecamera")) return;
            CommandBuffer commandBuffer = CommandBufferPool.Get("SpacecutRendererFeature");
            VolumeStack volumeStack = VolumeManager.instance.stack;
            CustomPostProcessSpacecut spacecutData = volumeStack.GetComponent<CustomPostProcessSpacecut>();
            if (spacecutData.IsActive())
            {
                var camera = renderingData.cameraData.camera.GetComponent<Camera>();
                var renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
                commandBuffer.Blit(_sceneDepth,renderTexture);
                 _material.SetTexture("_DepthTex", renderTexture);
                 Transform transform = camera.transform;
                 if (transform!= null)
                 {
                     _material.SetVector("_CameraPosition", transform.position);
                     _material.SetVector("_CameraRotation", transform.rotation.eulerAngles);
                 }

                 Blit(commandBuffer,_src, _tint, _material,0);
                 Blit(commandBuffer,_tint, _src);
                 RenderTexture.ReleaseTemporary(renderTexture);
            }   
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
    public override void Create()
    {
        _spacecutPass = new SpacecutPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_spacecutPass);
    }
}
