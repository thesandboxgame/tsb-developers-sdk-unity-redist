using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DefaultNamespace;
using JetBrains.Annotations;
using Sandbox.Developers;
using Sandbox.Developers.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour
{
    public TMP_Text status;
    public TMP_Text data;
    public Button buttonRequestAuthorization;
    public Button buttonRequestToken;
    public InputField inputFieldCode;
    public InputField inputFieldUserId;
    public InputField inputFieldLibraryId;
    public Button buttonExecute;
    public Dropdown dropDownEndpoint;
    public GameObject resultContent;
    public GameObject prefabItem;
    public GameObject textParam;
    public AssetViewer assetViewer;
    
    private Coroutine currentCoroutine;
    
    private const string OPTIONS_USERS_GETUSER = "Users.GetUser";
    private const string OPTIONS_INVENTORY_GETASSETS = "Inventory.GetAssets";
    private const string OPTIONS_INVENTORY_GETSAND = "Inventory.GetSand";
    private const string OPTIONS_INVENTORY_GETLANDS = "Inventory.GetLands";
    private const string OPTIONS_INVENTORY_GETAVATARS = "Inventory.GetAvatars";
    private const string OPTIONS_LIBRARY_GETLIBRARIES = "Library.GetLibraries";
    private const string OPTIONS_LIBRARY_GETASSETS = "Library.GetAssets";
    
    void PopulateFunctions()
    {
        dropDownEndpoint.ClearOptions();
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_USERS_GETUSER));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_INVENTORY_GETASSETS));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_INVENTORY_GETLANDS));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_INVENTORY_GETAVATARS));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_INVENTORY_GETSAND));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_LIBRARY_GETLIBRARIES));
        dropDownEndpoint.options.Add(new Dropdown.OptionData(OPTIONS_LIBRARY_GETASSETS));
    }
    
    
    // Start is called before the first frame update
    async void Start()
    {
        var settings = new OAuthSettings()
            { ClientId = "YOUR_CLIENT_ID", ClientSecret = "YOUR_CLIENT_SECRET", RedirectUri = "developertest://auth", Scope = "openid" };
        SandboxSDK.Initialize(settings);

        PopulateFunctions();
        dropDownEndpoint.onValueChanged.AddListener(OnDropDownChanged);
        dropDownEndpoint.RefreshShownValue();
        OnDropDownChanged(dropDownEndpoint.value);

        Application.deepLinkActivated += OnDeepLinkActivated;
        
        // Handle the deep link if the app was launched via a deep link
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            GetCode(Application.absoluteURL);
        }

        buttonRequestAuthorization.onClick.AddListener(() =>
        {
            SandboxSDK.RequestAuthorization();
            Application.Quit();
        });
        
        buttonRequestToken.onClick.AddListener(async () =>
        {
            status.text = "Authorizing..";
            bool loggedIn = await SandboxSDK.RequestToken(this.inputFieldCode.text);
            status.text = loggedIn ? "Authorization completed" : "Authorization Failed";
        });
        
        buttonExecute.onClick.AddListener(async () =>
        {
            status.text = "Requesting..";
            data.text = "";
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            DeleteResults();
            IEnumerable<(string, string)> images = null;
            try
            {
                switch (dropDownEndpoint.options[dropDownEndpoint.value].text)
                {
                    case OPTIONS_USERS_GETUSER:
                        UserResponse getUser = await SandboxSDK.GetUser(inputFieldUserId.text);
                        getUser.Initialize();
                        data.text = GetAllFields(getUser);
                        break;
                    case OPTIONS_INVENTORY_GETSAND:
                        SandResponse getSand = await SandboxSDK.GetSand(inputFieldUserId.text);
                        getSand.Initialize();
                        data.text = GetAllFields(getSand);
                        break;
                    case OPTIONS_INVENTORY_GETASSETS:
                        AssetsResponse getAssets = await SandboxSDK.GetInventoryAssets(inputFieldUserId.text);
                        getAssets.Initialize();
                        data.text = GetAllFields(getAssets);
                        images = getAssets.results.Select(p => (p.image_url, p.id));
                        break;
                    case OPTIONS_INVENTORY_GETAVATARS:
                        AvatarsResponse getAvatars = await SandboxSDK.GetInventoryAvatars(inputFieldUserId.text);
                        getAvatars.Initialize();
                        data.text = GetAllFields(getAvatars);
                        images = getAvatars.results.Select(p => (p.image_url, p.id));
                        break;
                    case OPTIONS_INVENTORY_GETLANDS:
                        LandsResponse getLands = await SandboxSDK.GetInventoryLands(inputFieldUserId.text);
                        getLands.Initialize();
                        data.text = GetAllFields(getLands);
                        break;
                    case OPTIONS_LIBRARY_GETLIBRARIES:
                        LibrariesResponse getLibraries = await SandboxSDK.GetLibraries();
                        getLibraries.Initialize();
                        data.text = GetAllFields(getLibraries);
                        break;
                    case OPTIONS_LIBRARY_GETASSETS:
                        getAssets = await SandboxSDK.GetLibraryAssets(inputFieldLibraryId.text);
                        getAssets.Initialize();
                        data.text = GetAllFields(getAssets);
                        images = getAssets.results.Select(p => (p.image_url, p.id));
                        break;
                }
                status.text = "Completed";
            }
            catch (Exception e)
            {
                status.text = "Error";
                data.text = e.Message;
            }
            if (images != null)
            {
                currentCoroutine = StartCoroutine(LoadResults(images));
            }
        });
    }
    
    public static string GetAllFields(object obj, int indent = 0)
    {
        if (obj == null) return "null";
        
        StringBuilder sb = new StringBuilder();
        Type type = obj.GetType();

        string indentSpace = new string(' ', indent);

        // Iterate over fields
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object value = field.GetValue(obj);

            if (value != null && field.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(field.FieldType))
            {
                sb.AppendLine($"{indentSpace}{field.Name}:");
                foreach (var child in value as IEnumerable)
                {
                    sb.Append(GetAllFields(child, indent + 2));   
                }
            }
            else if (value != null && field.FieldType.IsClass && field.FieldType != typeof(string))
            {
                sb.AppendLine($"{indentSpace}{field.Name}:");
                sb.Append(GetAllFields(value, indent + 2));
            }
            else
            {
                sb.AppendLine($"{indentSpace}{field.Name}: {value}");
            }
        }

        return sb.ToString();
    }
    
    public void DeleteResults()
    {
        foreach (Transform child in resultContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private IEnumerator LoadResults([ItemCanBeNull] IEnumerable<(string, string)> results)
    {
        foreach (var result in results)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(result.Item1);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var item = Instantiate(prefabItem);
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                item.GetComponentInChildren<RawImage>().texture = texture;
                item.transform.SetParent(resultContent.transform);

                ItemButton itemButton = item.GetComponent<ItemButton>();
                itemButton.viewer = assetViewer;
                itemButton.id = result.Item2;
            }
            else
            {
                Debug.LogError($"Error downloading image: {request.error}");
            }
        }
    }

    private void GetCode(string url)
    {
        // Parse the URL
        Uri uri = new Uri(url);
        string code = GetQueryParam(uri, "code");

        inputFieldCode.text = code;
    }
    
    private string GetQueryParam(Uri uri, string paramName)
    {
        var query = uri.Query;
        var queryParams = System.Web.HttpUtility.ParseQueryString(query);
        return queryParams.Get(paramName);
    }

    private void OnDeepLinkActivated(string url)
    {
        GetCode(url);
    }

    private void OnDropDownChanged(int index)
    {
        inputFieldLibraryId.gameObject.SetActive(dropDownEndpoint.options[index].text == OPTIONS_LIBRARY_GETASSETS);
        inputFieldUserId.gameObject.SetActive(dropDownEndpoint.options[index].text != OPTIONS_LIBRARY_GETASSETS 
                                              && dropDownEndpoint.options[index].text != OPTIONS_LIBRARY_GETLIBRARIES);
        textParam.SetActive(dropDownEndpoint.options[index].text != OPTIONS_LIBRARY_GETLIBRARIES);
        textParam.GetComponent<TMP_Text>().text = dropDownEndpoint.options[index].text == OPTIONS_LIBRARY_GETASSETS
            ? "Library Id"
            : "User Id";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
