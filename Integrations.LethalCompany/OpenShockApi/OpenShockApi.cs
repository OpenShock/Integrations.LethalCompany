using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;

namespace OpenShock.Integrations.LethalCompany.OpenShockApi;

public class OpenShockApi
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("OpenShockApi");
    private readonly HttpClient _httpClient;
    
    public OpenShockApi(Uri server, string apiToken)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = server
        };
        _httpClient.DefaultRequestHeaders.Add("OpenShockToken", apiToken);
    }

    public async Task Control(IEnumerable<Control> shocks)
    {
        Logger.LogInfo("Sending control request to OpenShock API");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/2/shockers/control")
        {
            Content = new StringContent(JsonConvert.SerializeObject(new ControlRequest
            {
                Shocks = shocks,
                CustomName = "Integrations.LethalCompany"
            }), Encoding.UTF8, "application/json")
        };
        var response = await _httpClient.SendAsync(requestMessage);
        
        if (!response.IsSuccessStatusCode) Logger.LogError($"Failed to send control request to OpenShock API [{response.StatusCode}]");
    }
}