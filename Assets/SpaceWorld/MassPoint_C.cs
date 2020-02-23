using System;
using Unity.Entities;
//质量组件
[GenerateAuthoringComponent]
public struct MassPoint_C : IComponentData, IComparable<MassPoint_C> {
    //质量
    public double Mass;
    public density densityType;
    public int Compare (object x, object y) {
        double m1 = ((MassPoint_C) x).Mass;
        double m2 = ((MassPoint_C) y).Mass;
        if (m1 < m2) return 1;
        if (m1 == m2) return 0;
        return -1;
    }
    public int CompareTo (MassPoint_C other) {
        return this.Mass.CompareTo (other.Mass);
    }
}
public enum density {
    water = 1000
}
public static class UnitHelper {
    // Mass单位;暂定kg
    static readonly double MassUnit = 1f;
    // Range单位;暂定m
    static readonly double RangeUnit = 5;
    // 体积换算成质量
    public static double Volume2Mass (this density density, double volume) {
        return volume * ((int) density * 0.001f) / MassUnit;
    }
    //质量换算成体积
    public static double Mass2Volume (this density density, double mass) {
        return mass * MassUnit / ((int) density * 0.001f);
    }
    //体积换算成半径
    public static double Volume2Range (double volume) {
        //return  Math.Log10 (volume)*0.1f;
        return  Math.Pow (volume / (Math.PI * 4.0 / 3), 1.0 / 3) / RangeUnit;
    }
    //半径换算成体积
    public static double Range2Volume (double range) {
        //return  Math.Pow (10, range*10f);
        return  (Math.Pow (range * RangeUnit, 3) * Math.PI * 4.0 / 3);
    }
    // //质量换算成半径
    public static double Mass2Range (this density density, double mass) {
        return Volume2Range (Mass2Volume (density, mass));
    }
    //半径换算成质量
    public static double Range2Mass (this density density, double range) {
        return Volume2Mass (density, Range2Volume (range));
    }
    //根据拉格朗日点计算力的平衡距离
    public static double LagrangianDistance (double volume1, double range1, double volume2, double range2) {
        var a = (volume1 < volume2?volume1 : volume2) / (volume1 + volume2);
        var alimit = 1 - Math.Pow (a * 1.0 / 3, 1.0 / 3);
        var rocheDis = alimit * (range1 + range2);
        return  rocheDis;
    }
}