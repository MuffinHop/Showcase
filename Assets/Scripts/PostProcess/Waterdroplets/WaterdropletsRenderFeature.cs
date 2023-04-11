using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaterdropletsRenderFeature : ScriptableRendererFeature
{
    private WaterdropletsPass _fluidPass;
    class WaterdropletsPass : ScriptableRenderPass
    {
        private Material _material0, _material1, _material2;
        private RenderTargetIdentifier _src, _tint;
        private int _tintId = Shader.PropertyToID("_Temp");

        public WaterdropletsPass()
        {
            if (!_material0)
            {
                _material0 = CoreUtils.CreateEngineMaterial("My Post-Processing/Waterdroplets_A");
            }

            if (!_material1)
            {
                _material1 = CoreUtils.CreateEngineMaterial("My Post-Processing/Waterdroplets_B");
            }

            if (!_material2)
            {
                _material2 = CoreUtils.CreateEngineMaterial("My Post-Processing/Waterdroplets_C");
            }
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        RenderTexture _renderTextureA;
        RenderTexture _renderTextureB;
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            _src = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(_tintId, renderTextureDescriptor, FilterMode.Bilinear);
            _tint = new RenderTargetIdentifier(_tintId);
            _renderTextureA = RenderTexture.GetTemporary(Screen.width, Screen.height, 0,GraphicsFormat.R32G32B32A32_SFloat);
            _renderTextureB = RenderTexture.GetTemporary(Screen.width, Screen.height, 0,GraphicsFormat.R32G32B32A32_SFloat);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tintId);
            RenderTexture.ReleaseTemporary(_renderTextureA);
            RenderTexture.ReleaseTemporary(_renderTextureB);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            var camName = renderingData.cameraData.camera.name;
            CommandBuffer commandBuffer = CommandBufferPool.Get("WaterdropletsRenderFeature");
            VolumeStack volumeStack = VolumeManager.instance.stack;
            
            CustomPostProcessWaterdroplets fluidData = volumeStack.GetComponent<CustomPostProcessWaterdroplets>();
            
            if (fluidData.IsActive())
            {
                _material0.SetTexture("_MainTex", _renderTextureB);
                _material1.SetTexture("_MainTex", _renderTextureA);
                commandBuffer.Blit( _renderTextureB, _renderTextureA, _material0);
                commandBuffer.Blit( _renderTextureA, _renderTextureB, _material1);
                
                _material2.SetTexture("_OffsetTex", _renderTextureA);
                Blit(commandBuffer,_src, _tint, _material2,0);
                Blit(commandBuffer,_tint, _src);
                
            }   
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
    public override void Create()
    {
        _fluidPass = new WaterdropletsPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_fluidPass);
    }
}
