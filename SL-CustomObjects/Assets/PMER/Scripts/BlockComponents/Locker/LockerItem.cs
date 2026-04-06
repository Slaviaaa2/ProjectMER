using System;
using UnityEngine;

[Serializable]
public class LockerItem
{
    public LockerItem() { }

    public LockerItem(SerializableLockerItem serializableLockerItem)
    {
        if (Enum.TryParse(serializableLockerItem.Item, out ItemType itemType))
        {
            ItemType = itemType;
        }
        else
        {
            CustomItem = serializableLockerItem.Item;
        }

        Count = serializableLockerItem.Count;
        Chance = serializableLockerItem.Chance;
    }

    [Tooltip("The ItemType of this pickup.")]
    public ItemType ItemType;

    public string CustomItem;

    public uint Count = 1;

    public float Chance = 100;
}
