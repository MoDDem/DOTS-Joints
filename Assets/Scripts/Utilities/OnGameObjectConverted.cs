using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IConvertedEntityTarget : IEventSystemHandler
{
	void Converted(Entity entity, GameObject gameObject);
}

public class OnGameObjectConverted : MonoBehaviour, IConvertGameObjectToEntity
{
	public GameObject ropeSpawner;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		ExecuteEvents.Execute<IConvertedEntityTarget>(FindObjectOfType<RopeSpawner>().gameObject, null, (x, y) => x.Converted(entity, gameObject));
	}
}
