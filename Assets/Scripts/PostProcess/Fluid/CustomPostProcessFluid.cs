using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("My Post-Processing/Fluid", typeof(UniversalRenderPipeline))]
public class CustomPostProcessFluid : VolumeComponent, IPostProcessComponent
{
    public TextureParameter rayMarchFluidTexture = new TextureParameter(null,true);
    public TextureParameter matcapTexture = new TextureParameter(null,true);
    public CubemapParameter cubemapTexture = new CubemapParameter(null, true);
    public FloatParameter fluidIntensity = new FloatParameter(1);
    public ColorParameter fluidColor = new ColorParameter(Color.white);
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
