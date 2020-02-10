using System;
using Unity.Entities;
//质量组件
[GenerateAuthoringComponent]
public struct MassPoint_C : IComponentData, IComparable<MassPoint_C> {
    //质量
    public float Mass;
    public int Compare (object x, object y) {
        float m1 = ((MassPoint_C) x).Mass;
        float m2 = ((MassPoint_C) y).Mass;
        if (m1 < m2) return 1;
        if (m1 == m2) return 0;
        return -1;
    }
    public int CompareTo(MassPoint_C other)
    {
        return  this.Mass.CompareTo(other.Mass);
    }
}