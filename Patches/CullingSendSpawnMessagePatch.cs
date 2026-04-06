using HarmonyLib;
using Mirror;
using ProjectMER.Features;

namespace ProjectMER.Patches;

/// <summary>
/// Intercepts Mirror's <c>NetworkServer.SendSpawnMessage</c> to prevent sending
/// spawn data for cullable objects to players who are too far away.
/// This is the key optimization that prevents the initial bandwidth burst when players join.
/// </summary>
[HarmonyPatch(typeof(NetworkServer), "SendSpawnMessage")]
public static class CullingSendSpawnMessagePatch
{
	public static bool Prefix(NetworkIdentity identity, NetworkConnection conn)
	{
		if (CullingManager.BypassCulling)
			return true;

		if (!CullingManager.IsCullable(identity))
			return true;

		if (CullingManager.IsWithinCullingDistance(identity, conn))
			return true;

		CullingManager.MarkAsHidden(identity, conn);
		return false;
	}
}
