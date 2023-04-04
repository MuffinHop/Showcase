using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchRenderFeature : ScriptableRendererFeature {
    private GlitchPass _glitchPass;
    class GlitchPass : ScriptableRenderPass
    {
        private Material _material;
        private RenderTargetIdentifier _src, _tint;
        private int _tintId = Shader.PropertyToID("_Temp");

        public GlitchPass()
        {
            if (!_material)
            {
                _material = CoreUtils.CreateEngineMaterial("My Post-Processing/Glitch");
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
            CommandBuffer commandBuffer = CommandBufferPool.Get("GlitchRenderFeature");
            VolumeStack volumeStack = VolumeManager.instance.stack;
            CustomPostProcessGlitch glitchData = volumeStack.GetComponent<CustomPostProcessGlitch>();
            if (glitchData.IsActive() )
            {
                _material.SetColor("_GlitchColor", (Color) glitchData.glitchColor);
                _material.SetFloat("_GlitchIntensity", (float)glitchData.glitchIntensity);
                Blit(commandBuffer,_src, _tint, _material,0);
                Blit(commandBuffer,_tint, _src);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
    public override void Create()
    {
        _glitchPass = new GlitchPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_glitchPass);
    }
}
