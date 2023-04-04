using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("My Post-Processing/SpaceCut", typeof(UniversalRenderPipeline))]
public class CustomPostProcessSpacecut : VolumeComponent, IPostProcessComponent
{
    
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}