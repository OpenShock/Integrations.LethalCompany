using GameNetcodeStuff;
using HarmonyLib;

namespace OpenShock.Integrations.LethalCompany;

public static class PlayerControllerPatches
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPostfix]
    public static void OnDeath(PlayerControllerB __instance)
    {
        if (!__instance.IsOwner || !__instance.AllowPlayerDeath()) return;
        LethalCompanyOpenShock.Instance.OnDeath();
    }
    
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPostfix]
    private static void DamagePatch(PlayerControllerB __instance, int damageNumber)
    {
        if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath()) return;
        LethalCompanyOpenShock.Instance.OnDamage(__instance.health, damageNumber);
    }
    
}