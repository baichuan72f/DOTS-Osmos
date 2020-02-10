using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ChunkRotationSpeed_Authoring : MonoBehaviour, IConvertGameObjectToEntity {
    public float DegressPerSecond = 360;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var data = new ChunkRotation_SpeedC () {
            RadiansPerSecond = math.radians (DegressPerSecond)
        };
        dstManager.AddComponentData (entity, data);
    }
}