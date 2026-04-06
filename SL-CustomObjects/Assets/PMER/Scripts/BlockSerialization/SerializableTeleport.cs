using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Teleport data serialized to a separate JSON file.
/// Matches ProjectMER's expected format (Id, Targets, Cooldown).
/// </summary>
public class SerializableTeleport : SchematicBlockData
{
    public SerializableTeleport() { }

    public SerializableTeleport(SchematicBlockData block)
    {
        Name = block.Name;
        ObjectId = block.ObjectId;
        ParentId = block.ParentId;
        Position = block.Position;
        Rotation = block.Rotation;
        Scale = block.Scale;
    }

    public string Id { get; set; }

    public List<string> Targets { get; set; } = new List<string>();

    public float Cooldown { get; set; }

    public SerializableVector TriggerScale { get; set; }

    [JsonIgnore]
    public override BlockType BlockType { get; set; }

    [JsonIgnore]
    public override string AnimatorName { get; set; }

    [JsonIgnore]
    public override Dictionary<string, object> Properties { get; set; }
}
