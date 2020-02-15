using Unity.Entities;

public struct SpawnAndRemoveEntity_C : IComponentData {
    public int CountX;// 预制体行数
    public int CountY;// 预制体列数
    public Entity entity;// 实体
    public float MinLifetime;// 最大生存时间
    public float MaxLifetime;// 最短生存时间
}