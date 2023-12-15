using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using UnityEngine;

namespace OpenShock.Integrations.LethalCompany;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class LethalCompanyOpenShock : BaseUnityPlugin
{
    public static LethalCompanyOpenShock Instance { get; private set; } = null!;
    public static ManualLogSource ModLogger { get; private set; } = null!;
    
    private OpenShockApi.OpenShockApi _openShockApi;
    private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    private ConfigEntry<string> _openShockServer = null!;
    private ConfigEntry<string> _openShockApiToken = null!;
    
    private void Awake()
    {
        ModLogger = Logger;
        Instance = this;
        Logger.LogInfo("Starting OpenShock.Integrations.LethalCompany");
        
        _openShockServer = Config.Bind<string>("OpenShock", "Server", "https://api.shocklink.net", "The URL of the OpenShock backend");
        _openShockApiToken = Config.Bind<string>("OpenShock", "ApiToken", "", "API token for authentication, can be found under API Tokens on the OpenShock dashboard (https://shocklink.net/#/dashboard/tokens)");
        
        Logger.LogDebug("Patching PlayerController");
        harmony.PatchAll(typeof(PlayerControllerPatches));

        _openShockServer.SettingChanged += (_, _) => SetupApi();
        _openShockApiToken.SettingChanged += (_, _) => SetupApi();
        
        SetupApi();
        
        Logger.LogInfo("Started OpenShock.Integrations.LethalCompany");
    }

    private void SetupApi()
    {
        _openShockApi = new OpenShockApi.OpenShockApi(new Uri(_openShockServer.Value), _openShockApiToken.Value);
    }

    public void OnDamage(int health, int damageNumber)
    {
        Logger.LogInfo($"Received damage, health is {health}, damage is {damageNumber}");
        LucTask.Run(() => _openShockApi.Control(new Control
        {
            Id = Guid.Parse("d9267ca6-d69b-4b7a-b482-c455f75a4408"),
            Duration = 5000,
            Intensity = (byte)Mathf.Clamp(damageNumber, 25, 100),
            Type = ControlType.Shock
        }));
    }

    public void OnDeath()
    {
        LucTask.Run(() => _openShockApi.Control(new Control
        {
            Id = Guid.Parse("d9267ca6-d69b-4b7a-b482-c455f75a4408"),
            Duration = 5000,
            Intensity = 100,
            Type = ControlType.Shock
        }));
    }
}