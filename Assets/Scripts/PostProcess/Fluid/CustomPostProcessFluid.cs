using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[Serializable, VolumeComponentMenuForRenderPipeline("My Post-Processing/Fluid", typeof(UniversalRenderPipeline))]
public class CustomPostProcessFluid : VolumeComponent, IPostProcessComponent
{
    [FormerlySerializedAs("rayMarchFluidTexture")] public TextureParameter fluidParticleTexture = new TextureParameter(null,true);
    public TextureParameter fluidParticleDepthTex = new TextureParameter(null,true);
    public FloatParameter fluidIntensity = new FloatParameter(1);
    public ColorParameter fluidColor = new ColorParameter(Color.white);
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
