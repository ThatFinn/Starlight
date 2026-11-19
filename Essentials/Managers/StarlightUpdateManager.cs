using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Starlight.Managers;

internal static class StarlightUpdateManager
{
    internal static bool UpdatedStarlight = false;
    internal static string NewVersion = null;
    private static bool isLatestVersion => NewVersion == BuildInfo.DisplayVersion;
    private static string _branchJson = "";
    internal static IEnumerator GetBranchJson()
    {
        string checkLink = BuildInfo.PreInfo[StarlightEntryPoint.UpdateBranch].Item2;
        if (string.IsNullOrEmpty(checkLink)) yield break;
        UnityWebRequest uwr = UnityWebRequest.Get(checkLink);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError) yield break;
        string json = uwr.downloadHandler.text;
        try
        {
            var jobject = JObject.Parse(json);
            if (!CheckSignature(jobject))
            {
                Log("Starlight API's signature is invalid, is the date & time set correctly?");
                throw new Exception();
            }
        }
        catch { Log("Starlight API either changed or is broken."); yield break; }
        _branchJson = json;
        if (CheckForUpdates.HasFlag()) StartCoroutine(CheckForNewVersion());
    }
    internal static IEnumerator CheckForNewVersion()
    {
        if (string.IsNullOrWhiteSpace(_branchJson)) yield break;
        try
        {
            var jobject = JObject.Parse(_branchJson);
            if (jobject.ContainsKey("manifesterror"))
            {
                var array = jobject["manifesterror"].ToObject<string[]>();
                if (array.ToNetList().Contains(BuildInfo.DisplayVersion))
                {
                    ExecuteInTicks((() =>
                    {
                        LogError("Critical exception in manifest validation, aborting...");
                        Application.Quit();
                        Environment.Exit(1);
                    }),1);
                }
            }
            if (CheckForUpdates.HasFlag())
            {
                var latest = jobject["latest"].ToObject<string>();
                NewVersion = latest;
                if (!isLatestVersion) if (AllowAutoUpdate.HasFlag()) if (StarlightEntryPoint.autoUpdate)
                    StartCoroutine(UpdateVersion());
            }
        }
        catch { Log("Starlight API either changed or is broken."); }
    }
    internal static IEnumerator UpdateVersion()
    {
        if (string.IsNullOrWhiteSpace(_branchJson)) yield break;
        string updateLink = "";
        try
        {
            var jobject = JObject.Parse(_branchJson);
            string latest = jobject["latest"].ToObject<string>();
            var latestVersion = jobject["versions_info"][latest];
            updateLink = latestVersion["download_url"].ToObject<string>();
        }
        catch { Log("Starlight API either changed or is broken."); yield break; }
        if (string.IsNullOrEmpty(updateLink)) yield break;
        UnityWebRequest uwr = UnityWebRequest.Get(updateLink);
        yield return uwr.SendWebRequest();
        if (!uwr.isNetworkError && !uwr.isHttpError)
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Log("Downloading Starlight complete");
                string path = StarlightEntryPoint.Instance.MelonAssembly.Assembly.Location;
                if (File.Exists(path))
                {
                    if(File.Exists(path + ".old")) File.Delete(path + ".old");
                    File.Move(path, path + ".old");
                }
                File.WriteAllBytes(Path.Combine(new FileInfo(path).Directory.FullName, "Starlight.dll"), uwr.downloadHandler.data);
                UpdatedStarlight = true;
                Log("Restart needed for applying Starlight update");
            }
    }
    
    internal static bool CheckSignature(JObject jobject)
    {
        try
        {
            if (!jobject.ContainsKey("signature") || !jobject.ContainsKey("expires")) return false;
            var signatureBase64 = jobject["signature"].Value<string>();
            var expires = jobject["expires"].Value<long>();

            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > expires) return false;

            var copy = (JObject)jobject.DeepClone();
            copy.Remove("signature");
            var dict = copy.ToObject<Dictionary<string, object>>();
            var sortedDict = new SortedDictionary<string, object>(dict);
            var canonicalJson = JsonConvert.SerializeObject(sortedDict, Formatting.None);
            
            var dataBytes = Encoding.UTF8.GetBytes(canonicalJson);
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            
            var pubKeyBase64 = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvW1COeevhDiQl5G4BbLTIZi+oGZNQSyorDs/ue1YGACa/KQEysp3akgFgR86sudcAnaA+b6qaR50hD+/Wd+ZofYxv8LRI1J6RqKWNzZDK8yJePo6eVbqPz1F/JP96+pNXHEGy7yJH0VRjO+h/Jb15vNfOSwfnoIWY4XUm/+LONTCTDXOfAS9HBHy6r3zc0AI0A+101Q8LYDhMrINiirVkQRZw4W5FzNF0Ouvj0emkQfyJ7pRZcD+GwrXhN5Sad65QeWF8joKq8aCcyEN0oifieDCtGyBCGbV+Jm3zElraS1NIKWDjQnT+b4QOJtSKRHSymVqX3oFWahV3cadi/CfqQIDAQAB";
            var publicKeyBytes = Convert.FromBase64String(pubKeyBase64);

            using (var rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
        catch (Exception e)
        {
            LogError("Signature verification error: " + e.Message);
            return false;
        }
    }
}