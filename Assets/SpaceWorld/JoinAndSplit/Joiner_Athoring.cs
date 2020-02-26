using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public class Joiner_Athoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Volume;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Joiner_C joiner = new Joiner_C();
        joiner.Volume = Volume;
        dstManager.AddComponentData(entity, joiner);
    }
}
