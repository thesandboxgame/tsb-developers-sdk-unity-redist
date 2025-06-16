using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sandbox.Developers;
using Sandbox.Developers.Importers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public CameraManager cameraManager;
    public GameObject loadingCanvas;
    public Image progressBar;
    public Text loadingText;

    private const string TREE_ID = "91a45705-f841-4b40-b730-1d2284dfa173";

    private string loggedUserId;
    private string playerAssetId;
    private List<string> npcAssetIds;

    // Start is called before the first frame update
    public async void Start()
    {
        loadingCanvas.SetActive(true);
        loadingText.text = "Initializing..";
        progressBar.transform.localScale = new Vector3(0, 1, 1);
        
        var settings = new Sandbox.Developers.Models.OAuthSettings()
            { ClientId = "sandbox-partner", ClientSecret = "", RedirectUri = "shooterdemo://auth", Scope = "openid" };
        SandboxSDK.Initialize(settings);
        
#if !UNITY_EDITOR
        // Deep link support
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            GetCode(args[1]);
            return;
        }
#endif
        
        if (!await SandboxSDK.RequestAuthorization())
        {
            Application.Quit();
        }
        else
        {
            await SetupScene();
        }
    }

    private async void GetCode(string url)
    {
        // Parse the URL
        Uri uri = new Uri(url);
        string code = GetQueryParam(uri, "code");

        if (await SandboxSDK.RequestToken(code))
        {
            await SetupScene();
        }
    }
    
    private string GetQueryParam(Uri uri, string paramName)
    {
        var query = uri.Query;
        var queryParams = System.Web.HttpUtility.ParseQueryString(query);
        return queryParams.Get(paramName);
    }

    /// <summary>
    /// Set up scene
    /// </summary>
    private async UniTask SetupScene()
    {
        loggedUserId = SandboxSDK.GetLoggedUserId();
        
        // Preload assets
        await PreloadAssets();
        
        // Add trees
        for (int i = 0; i <= 10; i++)
        {
            SandboxAssetImporter asset = await SandboxSDK.InstantiateAsset(TREE_ID);
            asset.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            asset.transform.position = new Vector3(-200 + i * 40f, 0, i % 2 == 0 ? 30f : 100f);
        }

        // Add player
        SandboxAssetImporter player = await SandboxSDK.InstantiateAsset(playerAssetId);
        player.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        cameraManager.SetTarget(player.gameObject);
        player.gameObject.AddComponent<PlayerLogic>();
        
        // Add NPCs
        foreach (var id in npcAssetIds)
        {
            await SpawnNPC(id);
            await SpawnNPC(id);
            await SpawnNPC(id);
        }
    }

    public async void Update()
    {
        // Spawn NPC
        if (Input.GetKeyDown(KeyCode.S))
        {
            await SpawnNPC(npcAssetIds[Random.Range(0, npcAssetIds.Count)]);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            await SandboxSDK.Logout();
            Application.Quit();
        }
    }

    /// <summary>
    /// Spawn NPC
    /// </summary>
    /// <param name="assetId">asset id to spawn</param>
    private async UniTask SpawnNPC(string assetId)
    {
        SandboxAssetImporter enemy = await SandboxSDK.InstantiateAsset(assetId);
        enemy.gameObject.AddComponent<NPCLogic>();
        enemy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        enemy.transform.position =
            new Vector3(Random.Range(-150f, 150f), 0, Random.Range(20f, 100f));
    }

    /// <summary>
    /// Preload assets
    /// </summary>
    private async UniTask PreloadAssets()
    {
        List<string> assetsToLoad = new List<string>();
        assetsToLoad.Add(TREE_ID);
        
        Sandbox.Developers.Models.AvatarData avatarData = await SandboxSDK.GetUserAvatar(loggedUserId);
        playerAssetId = avatarData.id;
        assetsToLoad.Add(playerAssetId);
        
        Sandbox.Developers.Models.AvatarsResponse avatars = await SandboxSDK.GetInventoryAvatars(loggedUserId);
        npcAssetIds = new List<string>();
        int types = Math.Min(avatars.results.Count, 3);
        for (int i = 0; i < types; i++)
        {
            npcAssetIds.Add(avatars.results[i].id);
        }
        assetsToLoad.AddRange(npcAssetIds);
            
        await SandboxSDK.PreloadAssets(assetsToLoad,
            (number, total) =>
            {
                loadingText.text = $"Loading asset: {number}/{total}";
                progressBar.transform.localScale = new Vector3((float)number / total, 1, 1);
            });

        await Task.Delay(400);
        loadingCanvas.SetActive(false);
    }
}
