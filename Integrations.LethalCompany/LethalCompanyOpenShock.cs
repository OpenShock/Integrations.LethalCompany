using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    private OpenShockApi.OpenShockApi _openShockApi = null!;
    private readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

    // Config
    
    // OpenShock Server
    private ConfigEntry<string> _openShockServer = null!;
    private ConfigEntry<string> _openShockApiToken = null!;
    private ConfigEntry<string> _shockers = null!;
    
    // Settings
    
    // OnDeath
    private ConfigEntry<byte> _settingOnDeathIntensity = null!;
    private ConfigEntry<ushort> _settingOnDeathDuration = null!;

    // OnDamage
    private ConfigEntry<ushort> _settingOnDamageDuration = null!;
    
    private IList<Guid> _shockersList = null!;

    private void Awake()
    {
        ModLogger = Logger;
        Instance = this;
        Logger.LogInfo("Starting OpenShock.Integrations.LethalCompany");

        // Config
        
        // OpenShock Server
        _openShockServer = Config.Bind<string>("OpenShock", "Server", "https://api.shocklink.net",
            "The URL of the OpenShock backend");
        _openShockApiToken = Config.Bind<string>("OpenShock", "ApiToken", "",
            "API token for authentication, can be found under API Tokens on the OpenShock dashboard (https://shocklink.net/#/dashboard/tokens)");
        _shockers = Config.Bind<string>("Shockers", "Shockers", "comma,seperated,list,of,shocker,ids",
            "A list of shocker IDs to use within the mod. Comma seperated.");
        
        
        // Settings
        
        // OnDeath
        _settingOnDeathIntensity = Config.Bind<byte>("OnDeath", "Intensity", 100,
            "The intensity of the shocker when the player dies");
        _settingOnDeathDuration = Config.Bind<ushort>("OnDeath", "Duration", 5000,
            "The duration of the shocker when the player dies");
        
        // OnDamage
        _settingOnDamageDuration = Config.Bind<ushort>("OnDamage", "Duration", 5000,
            "The duration of the shocker when the player takes damage");

        Logger.LogDebug("Patching PlayerController");
        _harmony.PatchAll(typeof(PlayerControllerPatches));

        _openShockServer.SettingChanged += (_, _) => SetupApi();
        _openShockApiToken.SettingChanged += (_, _) => SetupApi();
        _shockers.SettingChanged += (_, _) => ShockersSetup();

        SetupApi();
        ShockersSetup();

        Logger.LogInfo("Started OpenShock.Integrations.LethalCompany");
    }

    private void ShockersSetup()
    {
        var newList = new List<Guid>();
        foreach (var shocker in _shockers.Value.Split(','))
        {
            if (Guid.TryParse(shocker, out var shockerGuid))
            {
                Logger.LogInfo("Found shocker ID " + shockerGuid);
                newList.Add(shockerGuid);
            }
            else Logger.LogError($"Failed to parse shocker ID {shocker}");
        }

        _shockersList = newList;
    }

    private void SetupApi()
    {
        _openShockApi = new OpenShockApi.OpenShockApi(new Uri(_openShockServer.Value), _openShockApiToken.Value);
    }

    public void OnDamage(int health, int damageNumber)
    {
        Logger.LogInfo($"Received damage, health is {health}, damage is {damageNumber}");
        
        FireAndForgetControl(_settingOnDamageDuration.Value, (byte)Mathf.Clamp(damageNumber, 1, 100), ControlType.Shock);
    }

    private void FireAndForgetControl(ushort duration, byte intensity, ControlType type) =>
        LucTask.Run(() => SendControlToAll(duration, intensity, type));


    private Task SendControlToAll(ushort duration, byte intensity, ControlType type)
    {
        var controls = _shockersList.Select(shocker =>
            new Control
            {
                Id = shocker,
                Duration = duration,
                Intensity = intensity,
                Type = type
            });
        return _openShockApi.Control(controls);
    }

    public void OnDeath()
    {
        FireAndForgetControl(_settingOnDeathDuration.Value, _settingOnDeathIntensity.Value, ControlType.Shock);
    }
}