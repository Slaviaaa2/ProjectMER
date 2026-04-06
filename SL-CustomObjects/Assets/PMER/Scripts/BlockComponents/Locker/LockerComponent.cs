using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Locker block component for ProjectMER schematics.
/// Compiles locker data including LockerType and chamber/item info
/// into the block's Properties dictionary.
/// </summary>
public class LockerComponent : SchematicBlock
{
    public LockerChamber[] Chambers = new LockerChamber[] { };

    public string[] AllowedRoleTypes =
    {
        "Scp0492",
        "Scp049",
        "Scp096",
        "Scp106",
        "Scp173",
        "Scp93953",
        "Scp93989",
        "ClassD",
        "Scientist",
        "FacilityGuard",
        "NtfPrivate",
        "NtfSergeant",
        "NtfSpecialist",
        "NtfCaptain",
        "ChaosConscript",
        "ChaosRifleman",
        "ChaosRepressor",
        "ChaosMarauder",
        "Tutorial",
    };

    public bool ShuffleChambers = true;

    public KeycardPermissions KeycardPermissions = KeycardPermissions.None;

    public ushort OpenedChambers = 0;

    [Tooltip("Locks the locker after it was interacted with.")]
    public bool InteractLock = false;

    [Tooltip("The chance for this locker to spawn.")]
    public float Chance = 100f;

    [HideInInspector]
    public LockerType LockerType;

    public override BlockType BlockType => BlockType.Locker;

    public override void Compile(SchematicBlockData block)
    {
        Dictionary<int, List<SerializableLockerItem>> chambers = new Dictionary<int, List<SerializableLockerItem>>(Chambers.Length);
        int i = 0;

        foreach (LockerChamber chamber in Chambers)
        {
            List<SerializableLockerItem> listOfItems = new List<SerializableLockerItem>(chamber.PossibleItems.Count);

            foreach (LockerItem possibleItem in chamber.PossibleItems)
            {
                listOfItems.Add(new SerializableLockerItem(possibleItem));
            }

            chambers.Add(i, listOfItems);
            i++;
        }

        block.Properties = new Dictionary<string, object>
        {
            { "LockerType", LockerType },
            { "Chambers", chambers },
            { "ShuffleChambers", ShuffleChambers },
            { "AllowedRoleTypes", AllowedRoleTypes },
            { "KeycardPermissions", KeycardPermissions },
            { "OpenedChambers", OpenedChambers },
            { "InteractLock", InteractLock },
            { "Chance", Chance },
        };

        base.Compile(block);
    }

    public override void Decompile(ref GameObject gameObject, SchematicBlockData block, Transform parent)
    {
        string lockerPrefabName = "MiscLocker";
        if (block.Properties.TryGetValue("LockerType", out object lockerTypeObj))
        {
            LockerType lt = (LockerType)System.Convert.ToInt32(lockerTypeObj);
            lockerPrefabName = GetLockerPrefabName(lt);
        }

        LockerComponent locker = Create<LockerComponent>($"Assets/Resources/Blocks/Lockers/{lockerPrefabName}.prefab");
        if (locker == null)
        {
            locker = Create<LockerComponent>("Assets/Resources/Blocks/Lockers/MiscLocker.prefab");
        }

        gameObject = locker.gameObject;

        if (block.Properties.TryGetValue("LockerType", out object lt2))
            locker.LockerType = (LockerType)System.Convert.ToInt32(lt2);

        if (block.Properties.TryGetValue("ShuffleChambers", out object sc))
            locker.ShuffleChambers = System.Convert.ToBoolean(sc);

        if (block.Properties.TryGetValue("KeycardPermissions", out object kp))
            locker.KeycardPermissions = (KeycardPermissions)System.Convert.ToInt32(kp);

        if (block.Properties.TryGetValue("OpenedChambers", out object oc))
            locker.OpenedChambers = System.Convert.ToUInt16(oc);

        if (block.Properties.TryGetValue("InteractLock", out object il))
            locker.InteractLock = System.Convert.ToBoolean(il);

        if (block.Properties.TryGetValue("Chance", out object ch))
            locker.Chance = System.Convert.ToSingle(ch);

        base.Decompile(ref gameObject, block, parent);
    }

    private static string GetLockerPrefabName(LockerType lockerType)
    {
        return lockerType switch
        {
            LockerType.PedestalScp500 => "Pedestal",
            LockerType.LargeGun => "LargeGunLocker",
            LockerType.RifleRack => "RifleRack",
            LockerType.Misc => "MiscLocker",
            LockerType.Medkit => "CabinetMedkit",
            LockerType.Adrenaline => "CabinetAdrenaline",
            LockerType.PedestalScp018 => "Pedestal",
            LockerType.PedestalScp207 => "Pedestal",
            LockerType.PedestalScp244 => "Pedestal",
            LockerType.PedestalScp268 => "Pedestal",
            LockerType.PedestalScp1853 => "Pedestal",
            LockerType.PedestalScp2176 => "Pedestal",
            LockerType.PedestalScpScp1576 => "Pedestal",
            LockerType.PedestalAntiScp207 => "Pedestal",
            LockerType.PedestalScp1344 => "Pedestal",
            LockerType.ExperimentalWeapon => "LargeGunLocker",
            _ => "MiscLocker",
        };
    }
}
