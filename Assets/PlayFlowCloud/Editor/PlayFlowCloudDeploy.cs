﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

public class PlayFlowCloudDeploy : EditorWindow
{
    [SerializeField] private VisualTreeAsset _tree;
    private Button QuickStart;
    private Button documentationButton;
    private Button discordButton;
    private Button pricingButton;
    private Button getTokenButton;
    private Button uploadButton;
    private Button uploadStatusButton;
    private Button startButton;
    private Button refreshButton;
    private Button getStatusButton;
    private Button getLogsButton;
    private Button restartButton;
    private Button stopButton;
    private Button resetButton;
    private Button resetStatusButton;
    private Button getTagsButton;
    private Button ButtonDeleteTag;

    private TextField tokenField;
    private TextField sslValue;
    private TextField argumentsField;
    private TextField logs;
    private TextField servertag;


    private Foldout ConfigFoldout;
    private Foldout UploadFoldout;
    private Foldout LaunchServersFoldout;
    private Foldout ManageFoldout;
    private Foldout LogsFoldout;
    private Foldout TagsFoldout;

    


    private Toggle enableSSL;
    private Toggle devBuild;
    
    private DropdownField location;
    private DropdownField instanceType;
    private DropdownField activeServersField;
    private DropdownField sceneDropDown;
    private DropdownField LaunchTagDropdown;
    
    private DropdownField tagsDropDown;


    private Toggle buildSettingsToggle;

    private ProgressBar progress;
    
    private List<string> sceneList;

    


    [MenuItem("PlayFlow/PlayFlow Cloud")]
    public static void ShowEditor()
    {
        var window = GetWindow<PlayFlowCloudDeploy>();
        window.titleContent = new GUIContent("PlayFlow Cloud");
    }

    public Dictionary<string, string> productionRegionOptions = new Dictionary<string, string>
    {
        {"North America East (North Virginia)", "us-east"},
        {"North America West (California)", "us-west"},
        {"North America West (Oregon)", "us-west-2"},
        {"Europe (Stockholm)", "eu-north"},
        {"Europe (France)", "eu-west"},
        {"South Asia (Mumbai)", "ap-south"},
        {"South East Asia (Singapore)", "sea"},
        {"East Asia (Korea)", "ea"},
        {"East Asia (Japan)", "ap-north"},
        {"Australia (Sydney)", "ap-southeast"},
        {"South Africa (Cape Town)", "south-africa"},
        {"South America (Brazil)", "south-america-brazil"},
        {"South America (Chile)", "south-america-chile"}
    };

    Dictionary<string, string> instance_types = new Dictionary<string, string>
    {
        {"Small - 2 VCPU 1GB RAM", "small"},
        {"Medium - 2 VCPU 2GB RAM", "medium"},
        {"Large - 2 VCPU 4GB RAM", "large"},
    };

    private Dictionary<string, string> scenes = new Dictionary<string, string>();
    private void CreateGUI()
    {
        _tree.CloneTree(rootVisualElement);
        documentationButton = rootVisualElement.Q<Button>("ButtonDocumentation");
        discordButton = rootVisualElement.Q<Button>("ButtonDiscord");
        pricingButton = rootVisualElement.Q<Button>("ButtonPricing");
        getTokenButton = rootVisualElement.Q<Button>("ButtonGetToken");
        uploadButton = rootVisualElement.Q<Button>("ButtonUpload");
        uploadStatusButton = rootVisualElement.Q<Button>("ButtonUploadStatus");
        startButton = rootVisualElement.Q<Button>("ButtonStart");
        refreshButton = rootVisualElement.Q<Button>("ButtonRefresh");
        getStatusButton = rootVisualElement.Q<Button>("ButtonGetStatus");
        getLogsButton = rootVisualElement.Q<Button>("ButtonGetLogs");
        restartButton = rootVisualElement.Q<Button>("ButtonRestartServer");
        stopButton = rootVisualElement.Q<Button>("ButtonStopServer");
        resetButton =  rootVisualElement.Q<Button>("ResetInstance");
        resetStatusButton =  rootVisualElement.Q<Button>("InstanceStatus");
        QuickStart =  rootVisualElement.Q<Button>("QuickStart");
        getTagsButton = rootVisualElement.Q<Button>("ButtonGetTags");
        tagsDropDown = rootVisualElement.Q<DropdownField>("BuildTagsDropdown");
        ButtonDeleteTag = rootVisualElement.Q<Button>("ButtonDeleteTag");
        LaunchTagDropdown = rootVisualElement.Q<DropdownField>("LaunchTagDropdown");

        
        ConfigFoldout = rootVisualElement.Q<Foldout>("ConfigFoldout");
        UploadFoldout = rootVisualElement.Q<Foldout>("UploadFoldout");
        LaunchServersFoldout = rootVisualElement.Q<Foldout>("LaunchServersFoldout");
        ManageFoldout = rootVisualElement.Q<Foldout>("ManageFoldout");
        LogsFoldout = rootVisualElement.Q<Foldout>("LogsFoldout");
        TagsFoldout = rootVisualElement.Q<Foldout>("TagsFoldout");

        
        logs = rootVisualElement.Q<TextField>("logs");
        progress = rootVisualElement.Q<ProgressBar>("progress");

        sceneList = new List<string>();

        tokenField = rootVisualElement.Q<TextField>("TextToken");
        tokenField.RegisterValueChangedCallback(HandleToken);

        argumentsField = rootVisualElement.Q<TextField>("TextArgs");
        sslValue = rootVisualElement.Q<TextField>("sslValue");
        servertag = rootVisualElement.Q<TextField>("servertag");


        devBuild = rootVisualElement.Q<Toggle>("DevelopmentBuild");
        
        buildSettingsToggle = rootVisualElement.Q<Toggle>("UseBuildSettings");
        
        
        
        sceneDropDown = rootVisualElement.Q<DropdownField>("sceneDropDown");
        enableSSL = rootVisualElement.Q<Toggle>("enableSSL");
        sslValue.style.display = enableSSL.value ? DisplayStyle.Flex : DisplayStyle.None;

        
        
        location = rootVisualElement.Q<DropdownField>("locationDropdown");
        location.choices = productionRegionOptions.Keys.ToList();

        if (location.value == null || location.value.Equals(""))
        {
            location.index = 0;
        }
        
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            sceneList.Add(scene.path);
        }
        sceneDropDown.choices = sceneList;

        instanceType = rootVisualElement.Q<DropdownField>("instanceTypeDropdown");
        instanceType.choices = instance_types.Keys.ToList();

        if (instanceType.value == null  || instanceType.value.Equals(""))
        {
            instanceType.index = 0;
        }

        activeServersField = rootVisualElement.Q<DropdownField>("ActiveServersDropdown");
        activeServersField.choices = new List<string>();

        sceneDropDown.RegisterCallback<MouseDownEvent>(OnSceneDropDown);
        
        //When the user clicks the dropdown first time, we populate the list
        LaunchTagDropdown.RegisterCallback<MouseDownEvent>(OnLaunchTagDropDown);
        tagsDropDown.RegisterCallback<MouseDownEvent>(OnLaunchTagDropDown);

        
        documentationButton.clicked += OnDocumentationPressed;
        discordButton.clicked += OnDiscordPressed;
        pricingButton.clicked += OnPricingPressed;
        getTokenButton.clicked += OnGetTokenPressed;
        QuickStart.clicked += OnQuickStartPressed;

        uploadButton.clicked += OnUploadPressed;
        uploadStatusButton.clicked += OnUploadStatusPressed;
        startButton.clicked += OnStartPressed;
        enableSSL.RegisterValueChangedCallback(HandleSSL);
        buildSettingsToggle.RegisterValueChangedCallback(HandleBuildSettings);

        refreshButton.clicked += OnRefreshPressed;
        getStatusButton.clicked += OnGetStatusPressed;
        getLogsButton.clicked += OnGetLogsPressed;
        restartButton.clicked += OnRestartPressed;
        stopButton.clicked += OnStopPressed;

        resetButton.clicked += OnResetPressed;
        resetStatusButton.clicked += OnResetStatusPressed;
        getTagsButton.clicked += OnGetTagsPressed;
        ButtonDeleteTag.clicked += OnDeleteTagPressed;


    }
    


    private async void OnDeleteTagPressed()
    {
        if (tagsDropDown.value == null || tagsDropDown.value.Equals("") || tagsDropDown.value.Equals("default"))
        {
            outputLogs("Please select a tag to delete. Default tag cannot be deleted");
            return;
        }
        validateToken();
        showProgress();
        string response = await PlayFlowAPI.Delete_Tag(tokenField.value, tagsDropDown.value);
        outputLogs(response);
        hideProgress();
        
        //Change dropdown to index 0
        tagsDropDown.index = 0;
        
        OnGetTagsPressed();
    }
    
    private async void OnLaunchTagDropDown(MouseDownEvent evt)
    {
        OnGetTagsPressed();
    }
    private async void OnGetTagsPressed()
    {
        
        validateToken();
        showProgress();
        string response = await PlayFlowAPI.Get_Tags(tokenField.value);
        string currentTag = tagsDropDown.value;
        string currentLaunchTag = LaunchTagDropdown.value;
        outputLogs(response);
        //response to json object
        TagsResponse tagsResponse = JsonUtility.FromJson<TagsResponse>(response);
        tagsDropDown.choices = tagsResponse.tags.ToList();
        LaunchTagDropdown.choices = tagsResponse.tags.ToList();
        tagsDropDown.value = currentTag;
        LaunchTagDropdown.value = currentLaunchTag;
        hideProgress();
    }
    

    private void OnSceneDropDown(MouseDownEvent clickEvent)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            sceneList.Add(scene.path);
        }
        sceneDropDown.choices = sceneList;
    }
    
    private void HandleBuildSettings(ChangeEvent<bool> value)
    {
        if (value.newValue)
        {
            sceneDropDown.style.display = DisplayStyle.None;
        }
        else
        {
            sceneDropDown.style.display = DisplayStyle.Flex;
        }
    }

    private async void OnResetPressed()
    {
        validateToken();
        showProgress();
        string response = await PlayFlowAPI.ResetInstance(tokenField.value);
        outputLogs(response);
        hideProgress();
    }

    private async void OnResetStatusPressed()
    {
        validateToken();
        showProgress();
        string response = await PlayFlowAPI.ResetStatus(tokenField.value);
        outputLogs(response);
        hideProgress();
    }

    private void HandleToken(ChangeEvent<string> value)
    {
        //Check if token is valid and has > or = 32 characters before we validate
        validate(value.newValue);
        
    }

    private int api_version = 8;

    private async Task validate(string value)
    {
        string response = await PlayFlowAPI.Validate_Token(value);
        //response json = {"success":true,"api_version":"9"}
        //Check API version from the response json
        //Validation_Response
        Validation_Response validationResponse = JsonUtility.FromJson<Validation_Response>(response);

        if (validationResponse.success == "true")
        {
            if (validationResponse.api_version == "9")
            {
                enableSSL.style.display = DisplayStyle.None;
                sslValue.style.display = DisplayStyle.None;
                
                //Build Tag Enable
                LaunchTagDropdown.style.display = DisplayStyle.Flex;
                tagsDropDown.style.display = DisplayStyle.Flex;
                ButtonDeleteTag.style.display = DisplayStyle.Flex;
                getTagsButton.style.display = DisplayStyle.Flex;
                servertag.style.display = DisplayStyle.Flex;
                TagsFoldout.style.display = DisplayStyle.Flex;
                
                api_version = 9;

            }
            else
            {
                enableSSL.style.display = DisplayStyle.Flex;
                sslValue.style.display = DisplayStyle.Flex;
                
                //Build Tag Disable
                LaunchTagDropdown.style.display = DisplayStyle.None;
                tagsDropDown.style.display = DisplayStyle.None;
                ButtonDeleteTag.style.display = DisplayStyle.None;
                getTagsButton.style.display = DisplayStyle.None;
                
                //Clear the dropdowns
                LaunchTagDropdown.index = 0;
                tagsDropDown.index = 0;
                
                TagsFoldout.style.display = DisplayStyle.None;
                servertag.style.display = DisplayStyle.None;
                servertag.value = "default";
                
                api_version = 8;
                
                
            }
        }
    }

    private bool isProductionToken(string value)
    {
        return true;
    }

    private void HandleSSL(ChangeEvent<bool> value)
    {
        if (value.newValue && isProductionToken(tokenField.value))
        {
            sslValue.style.display = DisplayStyle.Flex;
        }
        else
        {
            sslValue.style.display = DisplayStyle.None;
        }
    }

    private void OnDocumentationPressed()
    {
        System.Diagnostics.Process.Start("https://docs.playflowcloud.com");
    }
    
    private void OnQuickStartPressed()
    {
        System.Diagnostics.Process.Start("https://docs.playflowcloud.com/guides/creating-your-first-server-deployment");
    }

    private void OnDiscordPressed()
    {
        System.Diagnostics.Process.Start("https://discord.gg/P5w45Vx5Q8");
    }

    private void OnPricingPressed()
    {
        System.Diagnostics.Process.Start("https://www.playflowcloud.com/pricing");
    }

    private void OnGetTokenPressed()
    {
        System.Diagnostics.Process.Start("https://app.playflowcloud.com");
    }

    private void validateToken()
    {
        if (tokenField.value == null || tokenField.value.Equals(""))
        {
            outputLogs("PlayFlow Token is empty. Please provide a PlayFlow token.");
            throw new Exception("PlayFlow Token is empty. Please provide a PlayFlow token.");
        }
    }

    private async void setCurrentServer(MatchInfo matchInfo)
    {
        await get_server_list(true);

        if (matchInfo != null)
        {
            string match = matchInfo.match_id;

            if (matchInfo.ssl_port != null)
            {
                match = matchInfo.match_id + " -> (SSL) " + matchInfo.ssl_port;
            }
            activeServersField.index = (activeServersField.choices.IndexOf(match));
        }

    }

    private async Task get_server_list(bool printOutput)
    {
        validateToken();
        string response = await PlayFlowAPI.GetActiveServers(tokenField.value, productionRegionOptions[location.value], true);
        Server[] servers = JsonHelper.FromJson<Server>(response);
        List<string> active_servers = new List<string>();
        foreach (Server server in servers)
        {
            string serverInfo = server.match_id;
            outputLogs(serverInfo);
            if (server.ssl_enabled)
            {
                serverInfo = server.match_id + " -> (SSL) " + server.ssl_port;
            }
            active_servers.Add(serverInfo);
        }
        active_servers.Sort();
        activeServersField.choices = active_servers;

        if (active_servers == null || active_servers.Count.Equals(0))
        {
            activeServersField.value = "";
            activeServersField.index = 0;
        }
        
        if (activeServersField.value == null || activeServersField.value.Equals(""))
        {
            activeServersField.index = 0;
        }

        if (printOutput)
        {
            outputLogs(response);
        }
    }

    private async Task get_status()
    {
        validateToken();
        if (activeServersField.value == null || activeServersField.value.Equals(""))
        {
            outputLogs("No server selected");
            return;
        }
        string response = await PlayFlowAPI.GetServerStatus(tokenField.value, activeServersField.value);
        outputLogs(response);
    }

    private async Task get_logs()
    {
        if (activeServersField.value == null || activeServersField.value.Equals(""))
        {
            outputLogs("No server selected");
            return;
        }
        string playflow_logs = await PlayFlowAPI.GetServerLogs(tokenField.value, productionRegionOptions[location.value], activeServersField.value);
        string[] split = playflow_logs.Split(new[] {"\\n"}, StringSplitOptions.None);
        playflow_logs = "";
        foreach (string s in split)
            playflow_logs += s + "\n";
        
        outputLogs(playflow_logs);
    }
    
    private async Task restart_server()
    {
        if (activeServersField.value == null || activeServersField.value.Equals(""))
        {
            outputLogs("No server selected");
            return;
        }
        string response =
            await PlayFlowAPI.RestartServer(tokenField.value, productionRegionOptions[location.value],  argumentsField.value, enableSSL.value.ToString(), activeServersField.value);
        outputLogs(response);
        
    }
    
    private async Task stop_server()
    {
        if (activeServersField.value == null || activeServersField.value.Equals(""))
        {
            outputLogs("No server selected");
            return;
        }
        string response =
            await PlayFlowAPI.StopServer(tokenField.value, productionRegionOptions[location.value],  activeServersField.value);
        outputLogs(response);
        await get_server_list(true);
        activeServersField.index = 0;
    }


    private void outputLogs(string s)
    {
        Debug.Log( DateTime.Now.ToString() + " PlayFlow Logs: " +  s);
        logs.value = s;
    }

    private async void OnRefreshPressed()
    {
        //
        try
        {
            validateToken();
            showProgress();
            refreshButton.SetEnabled(false);
            await get_server_list(true);
        }
        finally
        {
            hideProgress();
            refreshButton.SetEnabled(true);
        }
        
    }

    private async void OnGetStatusPressed()
    {
        //

        try
        {
            validateToken();
            showProgress();
            getStatusButton.SetEnabled(false);
            await get_status();

        }
        finally
        {
            hideProgress();
            getStatusButton.SetEnabled(true);
        }

    }

    private async void OnGetLogsPressed()
    {
        //

        try
        {
            validateToken();
            showProgress();
            getLogsButton.SetEnabled(false);
            await get_logs();
        }
        finally
        {
            hideProgress();
            getLogsButton.SetEnabled(true);
        }

    }

    private async void OnRestartPressed()
    {
        //
        try
        {
            validateToken();
            showProgress();
            restartButton.SetEnabled(false);
            await restart_server();
        }
        finally
        {
            hideProgress();
            restartButton.SetEnabled(true);
        }
    }

    private async void OnStopPressed()
    {
        //
        try
        {
            validateToken();
            showProgress();
            stopButton.SetEnabled(false);
            await stop_server();
        }
        finally
        {
            hideProgress();
            stopButton.SetEnabled(true);
        }

    }


    public static bool CheckLinuxServerModule()
    {
        // Check if the Linux Standalone target is supported
        bool isLinuxTargetSupported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        if (!isLinuxTargetSupported)
        {
            Debug.LogError("Linux Standalone target is not installed.");
            return false;
        }

        try
        {
            // Attempt to switch to the Linux Standalone target and set the server subtarget
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to set Linux Server build subtarget: " + e.Message);
            return false;
        }

        return true;
    }


    private void OnUploadPressed()
    {
                
        if (servertag.value == null || servertag.value.Equals(""))
        {
            Debug.LogError("Server tag cannot be empty. Please enter a server tag or use `default` as the server tag");
            return;
        }
        
        BuildTarget standaloneTarget = EditorUserBuildSettings.selectedStandaloneTarget;
        BuildTargetGroup currentBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(standaloneTarget);
#if UNITY_2021_2_OR_NEWER
        StandaloneBuildSubtarget currentSubTarget = EditorUserBuildSettings.standaloneBuildSubtarget;
#endif
        
#if UNITY_2021_2_OR_NEWER
        if (CheckLinuxServerModule() == false)
        {
            return;
        }
#endif
        
        validateToken();
        showProgress(25);

        

        //Check if build target is installed in the editor
        if (!BuildPipeline.IsBuildTargetSupported(currentBuildTargetGroup, standaloneTarget))
        {
            Debug.LogError("Build target " + standaloneTarget + " is not installed in the editor");
            return;
        }
        try
        {
            uploadButton.SetEnabled(false);
            List<string> scenesToUpload = new List<string>();
            if (buildSettingsToggle.value)
            {
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                    {
                        scenesToUpload.Add(scene.path);
                    }
                }
            }
            else
            {
                if (sceneDropDown.value == null || sceneDropDown.value.Equals(""))
                {
                    outputLogs("Select a scene first before uploading");
                    throw new Exception("Select a scene first before uploading");
                }
                scenesToUpload.Add(sceneDropDown.value);
            }
            

            bool success = PlayFlowBuilder.BuildServer(devBuild.value, scenesToUpload);
            if (!success)
            {
                outputLogs("Build failed");
                throw new Exception("Build failed");
            }
            string zipFile = PlayFlowBuilder.ZipServerBuild();
            string directoryToZip = Path.GetDirectoryName(PlayFlowBuilder.defaultPath);
            showProgress(50);
            string targetfile = Path.Combine(directoryToZip, @".." + Path.DirectorySeparatorChar + "Server.zip");
            showProgress(75);
            string playflow_logs = PlayFlowAPI.Upload(targetfile, tokenField.value, productionRegionOptions[location.value], servertag.value);
            outputLogs(playflow_logs);
            
        }
        finally
        {
            uploadButton.SetEnabled(true);

            EditorUserBuildSettings.SwitchActiveBuildTarget(currentBuildTargetGroup, standaloneTarget);
#if UNITY_2021_2_OR_NEWER
            EditorUserBuildSettings.standaloneBuildSubtarget = currentSubTarget;
#endif
            hideProgress();

            EditorUtility.ClearProgressBar();

        }
        //
    }

    private async void OnUploadStatusPressed()
    {
        validateToken();
        showProgress();
        string response = await PlayFlowAPI.Get_Upload_Version(tokenField.value);
        outputLogs(response);
        hideProgress();
        //
    }

    private async void OnStartPressed()
    {
        string response = "";
        if (api_version == 9)
        {
            if (LaunchTagDropdown.value == null || LaunchTagDropdown.value.Equals(""))
            {
                outputLogs("Select a launch tag first before starting or use `default` as the launch tag");
                return;
            }
        }

        try
        {
            validateToken();
            showProgress();
            if (enableSSL.value && !(sslValue.value == null || sslValue.value.Equals("")))
            {
                try
                {
                    int.Parse(sslValue.value);
                }
                catch
                {
                    outputLogs("SSL Port must be a valid integer.");
                    throw new Exception("SSL Port must be a valid integer.");
                }
            }

            startButton.SetEnabled(false);
            response = await PlayFlowAPI.StartServer(tokenField.value, productionRegionOptions[location.value],
                argumentsField.value, enableSSL.value.ToString(), sslValue.value.ToString(),
                instance_types[instanceType.value], isProductionToken(tokenField.value), LaunchTagDropdown.value);
            MatchInfo matchInfo = JsonUtility.FromJson<MatchInfo>(response);
            setCurrentServer(matchInfo);

        }
        finally{
            outputLogs(response);
            hideProgress();
            startButton.SetEnabled(true);

        }

      
    }
#pragma warning disable 0168

    private void showProgress()
    {
        try {
        progress.value = 50;
        progress.title = "Loading...";
        progress.style.display = DisplayStyle.Flex;
        }
        catch (Exception e)
        {
            Debug.Log("Progress Bar UI Not found. ProgressBar will be hidden. Loading in progress");
        }
    }
    
    private void showProgress(float value)
    {
        try
        {
            progress.value = value;
            progress.title = "Loading...";
            progress.style.display = DisplayStyle.Flex;
        }
        catch (Exception e)
        {
            Debug.Log("Progress Bar UI Not found. ProgressBar will be hidden. Loading in progress");
        }

    }
    
    private void hideProgress()
    {
        try
        {
            progress.value = 0;
            progress.style.display = DisplayStyle.None;
        }
        catch (Exception e)
        {
            Debug.Log("Progress Bar UI Not found. ProgressBar will be hidden");
        }
    }
#pragma warning restore 0168

}


//{"tags":["default","serverversion1","randombugtest","testtttttt"]}
[Serializable]
public class TagsResponse
{
    public string[] tags;
}


[Serializable]
public class Server
{
    public string ssl_port;
    public bool ssl_enabled;
    public string server_arguments;
    public string status;
    public string port;
    public string match_id;
    public string ssl_url;

}

[Serializable]
public class Validation_Response
{
    public string success;
    public string api_version;
}


[Serializable]
public class MatchInfo
{
    public string match_id;
    public string server_url;
    public string ssl_port;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.servers;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.servers = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.servers = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] servers;
    }
}

#endif