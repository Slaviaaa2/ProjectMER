using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public class SerializableCustomTriggerPoint : SerializableObject, IIndicatorDefinition
{
	/// <summary>
	/// Gets or sets the tag used to identify this trigger point from plugins.
	/// </summary>
	public string Tag { get; set; } = "";

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		GameObject triggerPoint = instance ?? new GameObject("CustomTriggerPoint");
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		triggerPoint.transform.SetPositionAndRotation(position, rotation);
		triggerPoint.transform.localScale = Scale;

		return triggerPoint;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);

		primitive.NetworkPrimitiveType = UnityEngine.PrimitiveType.Sphere;
		primitive.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
		primitive.NetworkMaterialColor = new Color(1f, 0f, 1f, 0.75f);
		primitive.transform.localScale = Vector3.one * 0.35f;
		primitive.transform.SetPositionAndRotation(position, rotation);

		return primitive.gameObject;
	}
}
