using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Imports/decompiles schematics from JSON back into Unity.
/// Teleporter and Locker decompilation are fully functional.
/// </summary>
public static class Decompiler
{
    public class SchematicBuilder : MonoBehaviour
    {
        public bool TryGetBlockFromType(BlockType blockType, out SchematicBlock schematicBlock) =>
            Dict.TryGetValue(blockType, out schematicBlock);

        private readonly Dictionary<BlockType, SchematicBlock> Dict = new();

        public SchematicBuilder Init()
        {
            Dict.Add(BlockType.Empty, gameObject.AddComponent<EmptyComponent>());
            Dict.Add(BlockType.Primitive, gameObject.AddComponent<PrimitiveComponent>());
            Dict.Add(BlockType.Light, gameObject.AddComponent<LightComponent>());
            Dict.Add(BlockType.Pickup, gameObject.AddComponent<PickupComponent>());
            Dict.Add(BlockType.Workstation, gameObject.AddComponent<WorkstationComponent>());
            Dict.Add(BlockType.Teleport, gameObject.AddComponent<TeleportComponent>());
            Dict.Add(BlockType.Locker, gameObject.AddComponent<LockerComponent>());
            Dict.Add(BlockType.Text, gameObject.AddComponent<TextComponent>());
            Dict.Add(BlockType.Interactable, gameObject.AddComponent<InteractableComponent>());
            Dict.Add(BlockType.Waypoint, gameObject.AddComponent<WaypointComponent>());

            return this;
        }
    }

    private static SchematicBuilder _schematicBuilder;

    [MenuItem("SchematicManager/Import Schematic/JSON")]
    private static void ImportSchematicJson()
    {
        string importPath = SchematicManager.Config.ExportPath;
        if (!Directory.Exists(importPath))
            Directory.CreateDirectory(importPath);

        string jsonFilePath = EditorUtility.OpenFilePanelWithFilters("Select json with the schematic", importPath, new string[] { "Schematic", "json" });
        if (string.IsNullOrEmpty(jsonFilePath))
        {
            Debug.LogError("Invalid schematic file. Path is empty.");
            return;
        }

        _schematicDirectoryPath = null;
        _schematicName = Path.GetFileNameWithoutExtension(jsonFilePath);
        _schematicData = JsonConvert.DeserializeObject<SchematicObjectDataList>(File.ReadAllText(jsonFilePath));

        PortBack();
    }

    [MenuItem("SchematicManager/Import Schematic/Folder")]
    private static void ImportSchematicFolder()
    {
        string importPath = SchematicManager.Config.ExportPath;
        if (!Directory.Exists(importPath))
            Directory.CreateDirectory(importPath);

        _schematicDirectoryPath = EditorUtility.OpenFolderPanel("Select folder with the schematic", importPath, "");
        if (string.IsNullOrEmpty(_schematicDirectoryPath))
        {
            Debug.LogError("Invalid schematic directory. Path is empty.");
            return;
        }

        _schematicName = Path.GetFileNameWithoutExtension(_schematicDirectoryPath);
        string jsonFilePath = Path.Combine(_schematicDirectoryPath, $"{_schematicName}.json");
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("No json file found in the schematic directory!");
            return;
        }

        _schematicData = JsonConvert.DeserializeObject<SchematicObjectDataList>(File.ReadAllText(jsonFilePath));

        PortBack();
    }

    private static void PortBack()
    {
        _rootTransform = new GameObject(_schematicName).AddComponent<Schematic>().transform;
        _objectFromId = new Dictionary<int, Transform>(_schematicData.Blocks.Count + 1)
        {
            { _schematicData.RootObjectId, _rootTransform },
        };

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Debug.Log("<color=#FFFF00>Importing schematic...</color>");

        _schematicBuilder = new GameObject("SchematicBuilder").AddComponent<SchematicBuilder>().Init();

        CreateRecursiveFromID(_schematicData.RootObjectId, _schematicData.Blocks, _rootTransform);

        if (_schematicDirectoryPath != null)
        {
            CreateTeleporters();
            AddRigidbodies();
        }

        Debug.Log($"<color=#00FF00>Successfully imported <b>{_schematicName}</b> schematic in {stopwatch.ElapsedMilliseconds} ms!</color>");
        NullifyFields();

        Object.DestroyImmediate(_schematicBuilder.gameObject);
    }

    private static void CreateRecursiveFromID(int id, List<SchematicBlockData> blocks, Transform parentGameObject)
    {
        Transform childGameObjectTransform = CreateObject(blocks.Find(c => c.ObjectId == id), parentGameObject) ?? _rootTransform;
        int[] parentSchematics = blocks.Where(bl => bl.BlockType == BlockType.Schematic).Select(bl => bl.ObjectId).ToArray();

        foreach (SchematicBlockData block in blocks.FindAll(c => c.ParentId == id))
        {
            if (parentSchematics.Contains(block.ParentId))
                continue;

            CreateRecursiveFromID(block.ObjectId, blocks, childGameObjectTransform);
        }
    }

    private static Transform CreateObject(SchematicBlockData block, Transform rootObject)
    {
        if (block == null)
            return null;

        GameObject gameObject = null;
        if (_schematicBuilder.TryGetBlockFromType(block.BlockType, out SchematicBlock schematicBlock))
        {
            schematicBlock.Decompile(ref gameObject, block, rootObject);
            _objectFromId.Add(block.ObjectId, gameObject.transform);
        }

        if (_schematicDirectoryPath != null && TryGetAnimatorController(block.AnimatorName, out RuntimeAnimatorController animatorController))
            gameObject.AddComponent<Animator>().runtimeAnimatorController = animatorController;

        return gameObject.transform;
    }

    private static bool TryGetAnimatorController(string animatorName, out RuntimeAnimatorController animatorController)
    {
        animatorController = null;

        if (!string.IsNullOrEmpty(animatorName))
        {
            Object animatorObject = AssetBundle.GetAllLoadedAssetBundles()
                .FirstOrDefault(x => x.mainAsset.name == animatorName)?.LoadAllAssets()
                .First(x => x is RuntimeAnimatorController);

            if (animatorObject == null)
            {
                string path = Path.Combine(_schematicDirectoryPath, animatorName);

                if (!File.Exists(path))
                    return false;

                animatorObject = AssetBundle.LoadFromFile(path).LoadAllAssets().First(x => x is RuntimeAnimatorController);
            }

            animatorController = animatorObject as RuntimeAnimatorController;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Imports teleporters from the separate Teleports JSON file
    /// and reconstructs target references between TeleportComponents.
    /// </summary>
    private static void CreateTeleporters()
    {
        string teleportPath = Path.Combine(_schematicDirectoryPath, $"{_schematicName}-Teleports.json");
        if (!File.Exists(teleportPath))
            return;

        List<SerializableTeleport> teleportDataList = JsonConvert.DeserializeObject<List<SerializableTeleport>>(File.ReadAllText(teleportPath));
        Dictionary<string, TeleportComponent> teleportById = new Dictionary<string, TeleportComponent>();

        // First pass: create all teleporter objects
        foreach (SerializableTeleport teleportData in teleportDataList)
        {
            TeleportComponent teleportComponent = SchematicBlock.Create<TeleportComponent>("Assets/Resources/Blocks/Teleporter.prefab");
            if (teleportComponent == null)
                continue;

            GameObject go = teleportComponent.gameObject;
            go.name = teleportData.Name;

            Transform parent = _objectFromId.ContainsKey(teleportData.ParentId)
                ? _objectFromId[teleportData.ParentId]
                : _rootTransform;

            go.transform.SetParent(parent);
            go.transform.localPosition = teleportData.Position;
            go.transform.localEulerAngles = teleportData.Rotation != null ? (Vector3)teleportData.Rotation : Vector3.zero;
            go.transform.localScale = teleportData.Scale != null ? (Vector3)teleportData.Scale : Vector3.one;

            teleportComponent.Id = teleportData.Id;
            teleportComponent.Cooldown = teleportData.Cooldown;
            if (teleportData.TriggerScale != null)
                teleportComponent.TriggerScale = teleportData.TriggerScale;

            teleportById[teleportData.Id] = teleportComponent;
            _objectFromId[teleportData.ObjectId] = go.transform;
        }

        // Second pass: link target references
        foreach (SerializableTeleport teleportData in teleportDataList)
        {
            if (!teleportById.TryGetValue(teleportData.Id, out TeleportComponent source))
                continue;

            List<TargetTeleport> targets = new List<TargetTeleport>();
            foreach (string targetId in teleportData.Targets)
            {
                if (teleportById.TryGetValue(targetId, out TeleportComponent target))
                {
                    targets.Add(new TargetTeleport
                    {
                        Id = targetId,
                        Teleport = target
                    });
                }
            }

            source.TargetTeleporters = targets.ToArray();
        }

        Debug.Log($"<color=#00FF00>Successfully imported <b>{teleportDataList.Count}</b> teleporters.</color>");
    }

    private static void AddRigidbodies()
    {
        string rigidbodyPath = Path.Combine(_schematicDirectoryPath, $"{_schematicName}-Rigidbodies.json");
        if (!File.Exists(rigidbodyPath))
            return;

        foreach (KeyValuePair<int, SerializableRigidbody> dict in JsonConvert.DeserializeObject<Dictionary<int, SerializableRigidbody>>(File.ReadAllText(rigidbodyPath)))
        {
            if (!_objectFromId.ContainsKey(dict.Key))
                continue;

            if (!_objectFromId[dict.Key].gameObject.TryGetComponent(out Rigidbody rigidbody))
                rigidbody = _objectFromId[dict.Key].gameObject.AddComponent<Rigidbody>();

            rigidbody.isKinematic = dict.Value.IsKinematic;
            rigidbody.useGravity = dict.Value.UseGravity;
            rigidbody.constraints = dict.Value.Constraints;
            rigidbody.mass = dict.Value.Mass;
        }
    }

    private static void NullifyFields()
    {
        _rootTransform = null;
        _schematicName = null;
        _schematicDirectoryPath = null;
        _schematicData = null;
        _objectFromId = null;
        AssetBundle.UnloadAllAssetBundles(false);
    }

    private static Transform _rootTransform;
    private static string _schematicName;
    private static string _schematicDirectoryPath;
    private static SchematicObjectDataList _schematicData;
    private static Dictionary<int, Transform> _objectFromId;
}
