using UnityEditor;
using UnityEngine;

/// <summary>
/// Right-click context menu for creating PMER blocks in Unity hierarchy.
/// All block types including Teleporter and Locker are enabled.
/// </summary>
public class RightClickMenuExtended
{
    [MenuItem("GameObject/PMER Blocks/Empty (Schematic)", false, -2)]
    private static void CreateEmptyOrSchematic(MenuCommand menuCommand)
    {
        GameObject parent = Selection.activeGameObject;

        if (menuCommand.context as GameObject != null)
            parent = menuCommand.context as GameObject;

        if (parent == null)
        {
            CreateBlock(menuCommand, "Assets/Resources/Blocks/Schematic.prefab");
        }
        else
        {
            CreateBlock(menuCommand, "Assets/Resources/Blocks/Empty.prefab");
        }
    }

    #region Primitives
    [MenuItem("GameObject/PMER Blocks/Primitives/Cube", false, -1)]
    private static void CreateCube(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Cube);

    [MenuItem("GameObject/PMER Blocks/Primitives/Sphere", false, -1)]
    private static void CreateSphere(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Sphere);

    [MenuItem("GameObject/PMER Blocks/Primitives/Capsule", false, -1)]
    private static void CreateCapsule(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Capsule);

    [MenuItem("GameObject/PMER Blocks/Primitives/Cylinder", false, -1)]
    private static void CreateCylinder(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Cylinder);

    [MenuItem("GameObject/PMER Blocks/Primitives/Plane", false, -1)]
    private static void CreatePlane(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Plane);

    [MenuItem("GameObject/PMER Blocks/Primitives/Quad", false, -1)]
    private static void CreateQuad(MenuCommand menuCommand) => CreatePrimitive(menuCommand, PrimitiveType.Quad);

    private static void CreatePrimitive(MenuCommand menuCommand, PrimitiveType primitiveType) => CreateBlock(menuCommand, $"Assets/Resources/Blocks/Primitives/{primitiveType}.prefab");
    #endregion

    #region Lights
    [MenuItem("GameObject/PMER Blocks/Lights/Directional", false, -1)]
    private static void CreateDirectionalLight(MenuCommand menuCommand) => CreateLight(menuCommand, LightType.Directional);

    [MenuItem("GameObject/PMER Blocks/Lights/Point", false, -1)]
    private static void CreatePointLight(MenuCommand menuCommand) => CreateLight(menuCommand, LightType.Point);

    [MenuItem("GameObject/PMER Blocks/Lights/Spot", false, -1)]
    private static void CreateSpotLight(MenuCommand menuCommand) => CreateLight(menuCommand, LightType.Spot);

    [MenuItem("GameObject/PMER Blocks/Lights/Area", false, -1)]
    private static void CreateAreaLight(MenuCommand menuCommand) => CreateLight(menuCommand, LightType.Area);

    private static void CreateLight(MenuCommand menuCommand, LightType lightType) => CreateBlock(menuCommand, $"Assets/Resources/Blocks/Lights/{lightType} Light.prefab");
    #endregion

    [MenuItem("GameObject/PMER Blocks/Pickup", false, -1)]
    private static void CreatePickup(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Pickup.prefab");

    [MenuItem("GameObject/PMER Blocks/Workstation", false, -1)]
    private static void CreateWorkstation(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Workstation.prefab");

    [MenuItem("GameObject/PMER Blocks/Teleporter", false, -1)]
    private static void CreateTeleport(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Teleporter.prefab");

    #region Lockers
    [MenuItem("GameObject/PMER Blocks/Lockers/Pedestal", false, -1)]
    private static void CreatePedestal(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.PedestalScp500);

    [MenuItem("GameObject/PMER Blocks/Lockers/Large Gun Locker", false, -1)]
    private static void CreateLargeGunLocker(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.LargeGun);

    [MenuItem("GameObject/PMER Blocks/Lockers/Rifle Rack", false, -1)]
    private static void CreateRifleRack(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.RifleRack);

    [MenuItem("GameObject/PMER Blocks/Lockers/Misc Locker", false, -1)]
    private static void CreateMiscLocker(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.Misc);

    [MenuItem("GameObject/PMER Blocks/Lockers/Medkit Cabinet", false, -1)]
    private static void CreateMedkitCabinet(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.Medkit);

    [MenuItem("GameObject/PMER Blocks/Lockers/Adrenaline Cabinet", false, -1)]
    private static void CreateAdrenalineCabinet(MenuCommand menuCommand) => CreateLocker(menuCommand, LockerType.Adrenaline);

    private static void CreateLocker(MenuCommand menuCommand, LockerType lockerType)
    {
        string prefabName = lockerType switch
        {
            LockerType.PedestalScp500 => "Pedestal",
            LockerType.LargeGun => "LargeGunLocker",
            LockerType.RifleRack => "RifleRack",
            LockerType.Misc => "MiscLocker",
            LockerType.Medkit => "CabinetMedkit",
            LockerType.Adrenaline => "CabinetAdrenaline",
            _ => "MiscLocker",
        };

        GameObject instance = SchematicBlock.Create<GameObject>($"Assets/Resources/Blocks/Lockers/{prefabName}.prefab");
        if (instance == null)
            return;

        LockerComponent locker = instance.GetComponent<LockerComponent>();
        if (locker == null)
            locker = instance.AddComponent<LockerComponent>();

        locker.LockerType = lockerType;

        GameObject parent = Selection.activeGameObject;
        if (menuCommand.context as GameObject != null)
            parent = menuCommand.context as GameObject;

        GameObjectUtility.SetParentAndAlign(instance, parent);
        Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
        Selection.activeGameObject = instance;
    }
    #endregion

    [MenuItem("GameObject/PMER Blocks/Text", false, -1)]
    private static void CreateText(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Text.prefab");

    [MenuItem("GameObject/PMER Blocks/Interactable", false, -1)]
    private static void CreateInteractable(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Interactable.prefab");

    [MenuItem("GameObject/PMER Blocks/Waypoint", false, -1)]
    private static void CreateWaypoint(MenuCommand menuCommand) => CreateBlock(menuCommand, "Assets/Resources/Blocks/Waypoint.prefab");

    private static void CreateBlock(MenuCommand menuCommand, string prefabPath)
    {
        GameObject instance = SchematicBlock.Create<GameObject>(prefabPath);
        if (instance == null)
            return;

        GameObject parent = Selection.activeGameObject;

        if (menuCommand.context as GameObject != null)
            parent = menuCommand.context as GameObject;

        GameObjectUtility.SetParentAndAlign(instance, parent);

        Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");

        Selection.activeGameObject = instance;
    }
}
