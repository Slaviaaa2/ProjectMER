[System.Serializable]
public class SerializableLockerItem
{
    public SerializableLockerItem() { }

    public SerializableLockerItem(LockerItem lockerItem)
    {
        Item = !string.IsNullOrEmpty(lockerItem.CustomItem) ? lockerItem.CustomItem : lockerItem.ItemType.ToString();
        Count = lockerItem.Count;
        Chance = lockerItem.Chance;
    }

    public string Item { get; set; }
    public uint Count { get; set; }
    public float Chance { get; set; }
}
