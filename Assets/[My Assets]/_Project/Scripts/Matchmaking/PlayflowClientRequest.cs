using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class PlayflowClientRequest : MonoBehaviour
{
    private const string ApiUrl = "https://api.cloud.playflow.app/";
    public const string token = "dd90263d88abf74a64b8ec7f313451ff";
    private const string version = "2";

    public async Task<string> StartServer(string region, string arguments = "", string ssl = "false")
    {
        string url = $"{ApiUrl}start_game_server";

        using var client = new HttpClient(); // = UnityWebRequest counterpart
        using var formData = new MultipartFormDataContent();
        
        formData.Headers.Add("token", token);
        formData.Headers.Add("region", region);
        formData.Headers.Add("version", version);
        formData.Headers.Add("arguments", arguments);
        formData.Headers.Add("ssl", ssl);

        var response = await client.PostAsync(url, formData);

        if (!response.IsSuccessStatusCode)
        {
            Debug.Log(await response.Content.ReadAsStringAsync()); // There was an error
        }
        
        return await response.Content.ReadAsStringAsync();
    }

    public async void GetServers(Action<ServerList> callback)
    {
        string response = await GetServersAsync();
        
        print(response);
        ServerList serverList = JsonUtility.FromJson<ServerList>(response);
        
        callback?.Invoke(serverList);
    }
    
    private async Task<string> GetServersAsync()
    {
        string url = $"{ApiUrl}list_servers";

        using var client = new HttpClient(); // = UnityWebRequest counterpart
        using var formData = new MultipartFormDataContent();
        
        formData.Headers.Add("token", token);
        formData.Headers.Add("version", version);
        formData.Headers.Add("includelaunchingservers", "true");
        
        var response = await client.PostAsync(url, formData);

        if (!response.IsSuccessStatusCode)
        {
            Debug.Log(await response.Content.ReadAsStringAsync()); // There was an error
        }
        
        return await response.Content.ReadAsStringAsync();
    }
    
    [System.Serializable]
    public class ServerList
    {
        public int total_servers;
        public Server[] servers;
    }

    [System.Serializable]
    public class Server
    {
        public string match_id;
        public string status;
        public string region;
        public string instance_type;
        public int server_version;
        public string server_arguments;
        public bool ssl_enabled; // bool can be strings
        public string ip;
        public string start_time;
        public string ports;
    }
}
