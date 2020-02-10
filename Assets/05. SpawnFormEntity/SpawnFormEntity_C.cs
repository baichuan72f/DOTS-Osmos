using Unity.Entities;

public struct SpawnFormEntity_C : IComponentData
{
    public int CountX;
    public int CountY;
    public Entity EntityPrefab;
}
