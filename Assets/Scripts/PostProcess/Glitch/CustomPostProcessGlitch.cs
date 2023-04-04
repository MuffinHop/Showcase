using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[Serializable, VolumeComponentMenuForRenderPipeline("My Post-Processing/Glitch", typeof(UniversalRenderPipeline))]
public class CustomPostProcessGlitch : VolumeComponent, IPostProcessComponent
{
    public FloatParameter glitchIntensity = new FloatParameter(1);
    public ColorParameter glitchColor = new ColorParameter(Color.white);
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
    
}