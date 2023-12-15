using GameNetcodeStuff;
using HarmonyLib;

namespace OpenShock.Integrations.LethalCompany;

public static class PlayerControllerPatches
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPostfix]
    public static void OnDeath()
    {
        LethalCompanyOpenShock.Instance.OnDeath();
    }
    
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPostfix]
    private static void DamagePatch(int ___health, int damageNumber)
    {
        LethalCompanyOpenShock.Instance.OnDamage(___health, damageNumber);
    }
    
}