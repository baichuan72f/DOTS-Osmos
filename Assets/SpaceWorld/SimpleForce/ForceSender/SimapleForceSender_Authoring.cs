using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SimapleForceSender_Authoring : MonoBehaviour, IConvertGameObjectToEntity {
    public float3 direction_default;
    public float time;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var force = new SimapleForceSender_C ();
        force.value = direction_default;
        force.time = time;
        force.to = entity; //受力物体为当前物体
        dstManager.AddComponentData (dstManager.CreateEntity (), force);
    }
}
