using Unity.Entities;

public struct SpawnFormEntity_C : IComponentData {
    public Entity entity;
    public int CountX;
    public int CountY;
}