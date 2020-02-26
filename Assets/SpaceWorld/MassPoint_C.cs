using System;
using Unity.Entities;
//质量组件
[GenerateAuthoringComponent]
public struct MassPoint_C : IComponentData, IComparable<MassPoint_C>
{
    //质量
    public double Mass;

    public int Compare(object x, object y)
    {
        double m1 = ((MassPoint_C)x).Mass;
        double m2 = ((MassPoint_C)y).Mass;
        if (m1 < m2) return 1;
        if (m1 == m2) return 0;
        return -1;
    }
    public int CompareTo(MassPoint_C other)
    {
        return this.Mass.CompareTo(other.Mass);
    }
}

public static class UnitHelper
{
    // Mass单位;暂定kg
    static readonly double MassUnit = 1f;
    // Range单位;暂定m
    static readonly double RangeUnit = 10;
    // 体积换算成质量
    public static double Volume2Mass(double volume)
    {
        return volume / MassUnit;
    }
    //质量换算成体积
    public static double Mass2Volume(double mass)
    {
        return mass * MassUnit;
    }
    //体积换算成半径
    public static double Volume2Range(double volume)
    {
        return Math.Pow(volume , 1.0 / 3) / RangeUnit;
    }
    //半径换算成体积
    public static double Range2Volume(double range)
    {
        return Math.Pow(range * RangeUnit, 3);
    }

    public static double Mass2Range(double mass)
    {
        return Volume2Range(Mass2Volume(mass));
    }
    //半径换算成体积
    public static double Range2Mass(double range)
    {
        return Volume2Mass(Range2Volume(range));
    }
}