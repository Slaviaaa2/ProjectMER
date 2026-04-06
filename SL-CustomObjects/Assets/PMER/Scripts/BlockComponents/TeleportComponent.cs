using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Teleport block component for ProjectMER schematics.
/// Fully functional - compiles to a separate Teleports JSON file
/// that the ProjectMER server reads at runtime.
/// </summary>
[ExecuteInEditMode]
public class TeleportComponent : SchematicBlock
{
    public override BlockType BlockType => BlockType.Teleport;

    [Tooltip("Unique ID for this teleporter. Auto-generated if empty.")]
    public string Id;

    [Tooltip("Drag and drop target teleporters here.")]
    public TargetTeleport[] TargetTeleporters = new TargetTeleport[0];

    [Tooltip("Cooldown in seconds before the teleporter can be used again.")]
    public float Cooldown = 5f;

    [Tooltip("The trigger area scale of this teleporter.")]
    public Vector3 TriggerScale = Vector3.one;

    public override void Compile(SchematicBlockData block)
    {
        base.Compile(block);

        foreach (TargetTeleport target in TargetTeleporters)
        {
            if (target.Teleport != null)
                target.Id = target.Teleport.Id;
        }

        SerializableTeleport serializableTeleport = new SerializableTeleport(block)
        {
            Id = Id,
            Targets = TargetTeleporters
                .Where(t => t.Teleport != null && !string.IsNullOrEmpty(t.Id))
                .Select(t => t.Id)
                .ToList(),
            Cooldown = Cooldown,
            TriggerScale = TriggerScale,
        };

        Schematic schematic = GetComponentInParent<Schematic>();
        if (schematic != null)
            schematic.Teleports.Add(serializableTeleport);
    }

    private MeshFilter _filter;
    private MeshRenderer _renderer;

    private void Awake()
    {
        TryGetComponent(out _filter);
        TryGetComponent(out _renderer);
    }

    private void Update()
    {
        if (_filter != null)
            _filter.hideFlags = HideFlags.HideInInspector;
        if (_renderer != null)
            _renderer.hideFlags = HideFlags.HideInInspector;

        if (string.IsNullOrEmpty(Id))
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);

        foreach (TargetTeleport teleport in TargetTeleporters)
        {
            if (teleport.Teleport != null)
                teleport.Id = teleport.Teleport.Id;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.11f, 0.98f, 0.92f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, TriggerScale);
        Gizmos.DrawWireCube(Vector3.zero, TriggerScale);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.cyan;
        foreach (TargetTeleport target in TargetTeleporters)
        {
            if (target.Teleport != null)
                Gizmos.DrawLine(transform.position, target.Teleport.transform.position);
        }
    }

    public override void Decompile(ref GameObject gameObject, SchematicBlockData block, Transform parent)
    {
        TeleportComponent teleport = Create<TeleportComponent>("Assets/Resources/Blocks/Teleporter.prefab");
        gameObject = teleport.gameObject;

        base.Decompile(ref gameObject, block, parent);
    }
}

[Serializable]
public class TargetTeleport
{
    [HideInInspector]
    public string Id;

    [Tooltip("Drag and drop target teleporter here.")]
    public TeleportComponent Teleport;
}
