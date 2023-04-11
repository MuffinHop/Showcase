using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[Serializable, VolumeComponentMenuForRenderPipeline("My Post-Processing/Waterdroplets", typeof(UniversalRenderPipeline))]
public class CustomPostProcessWaterdroplets : VolumeComponent, IPostProcessComponent
{
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}