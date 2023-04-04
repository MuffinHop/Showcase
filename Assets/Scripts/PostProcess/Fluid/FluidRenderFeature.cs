using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FluidRenderFeature : ScriptableRendererFeature
{
    private FluidPass _fluidPass;
    class FluidPass : ScriptableRenderPass
    {
        private Material _material;
        private RenderTargetIdentifier _src, _tint;
        private int _tintId = Shader.PropertyToID("_Temp");

        public FluidPass()
        {
            if (!_material)
            {
                _material = CoreUtils.CreateEngineMaterial("My Post-Processing/Fluid");
            }

            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            _src = renderingData.cameraData.renderer.cameraColorTarget;
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
            if (camName.ToLower().Contains("fluid") || camName.ToLower().Contains("scenecamera")) return;
            CommandBuffer commandBuffer = CommandBufferPool.Get("FluidRenderFeature");
            VolumeStack volumeStack = VolumeManager.instance.stack;
            CustomPostProcessFluid fluidData = volumeStack.GetComponent<CustomPostProcessFluid>();
            if (fluidData.rayMarchFluidTexture.value == null)
            {
                return;
            }
            if (fluidData.IsActive())
            {
                _material.SetTexture("_FluidRayMarchTex", (Texture)fluidData.rayMarchFluidTexture);
                _material.SetTexture("_MatCapTex", (Texture)fluidData.matcapTexture);
                _material.SetColor("_FluidColor", (Color) fluidData.fluidColor);
                _material.SetFloat("_FluidIntensity", (float)fluidData.fluidIntensity);
                Blit(commandBuffer,_src, _tint, _material,0);
                Blit(commandBuffer,_tint, _src);
            }   
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
    public override void Create()
    {
        _fluidPass = new FluidPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_fluidPass);
    }
}
