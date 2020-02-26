using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
//物体可以分离的组件标签
public struct Joiner_C : IComponentData
{
    private double range;
    public double Range
    {
        get => range;
        set => volume = UnitHelper.Range2Volume(range = value);
    }
    private double volume;
    //当前星体半径
    public double Volume
    {
        get => volume;
        set => range = UnitHelper.Volume2Range(volume = value);
    }
}


[MaterialProperty("_FeelTime", MaterialPropertyFormat.Float)]
public struct ColorLevel : IComponentData
{
    public float Value;
}