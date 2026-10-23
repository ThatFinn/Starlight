/*using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Starlight.Repos;

namespace Starlight.Managers;

internal static class StarlightRepoManager
{
    internal static Dictionary<string,Repo> repos = new Dictionary<string, Repo>();
    internal static void Start()
    {
        
        Thread fileThread = new Thread(StartSeperate);
        fileThread.Start();
    }

    static void StartSeperate()
    {
        
        List<RepoSave> repoSaves = new List<RepoSave>(){new RepoSave("official","https://api.starlight.sr2.dev/repo")};
        if(UseMockRepo.HasFlag()) repoSaves.Add(new RepoSave("official_mock","https://api.starlight.sr2.dev/mockrepo"));
        repoSaves.AddRange(StarlightSaveManager.data.repos);
        foreach (RepoSave repoSave in repoSaves)
        {
            var repo = CheckRepo(repoSave);
            repos.Add(repoSave.identifier,repo);
           
        }
    }
    static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        Error = (sender, args) =>
        {
            if(DebugLogging.HasFlag()) Log($"Error: {args.ErrorContext.Error.Message}");
            //if (args.ErrorContext.Member is string memberName && args.ErrorContext.Path.Contains(nameof(StarlightSaveData.fonts))) 
            //    ((Dictionary<string, StarlightMenuFont>)args.ErrorContext.OriginalObject)[memberName] = StarlightMenuFont.Default;
            //if (args.ErrorContext.Member is string memberName2 && args.ErrorContext.Path.Contains(nameof(StarlightSaveData.themes))) 
            //    ((Dictionary<string, StarlightMenuTheme>)args.ErrorContext.OriginalObject)[memberName2] = StarlightMenuTheme.Default;
            args.ErrorContext.Handled = true;
        }
    };
    static Repo CheckRepo(RepoSave repoSave)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = client.GetStringAsync(repoSave.url).Result;

                try
                {
                    var repo = JsonConvert.DeserializeObject<Repo>(response, jsonSerializerSettings);
                    if (repo.identifier != repoSave.identifier)
                    {
                        Log("StarlightRepo identifier changed"); 
                        return null;
                    }
                    return repo;

                }
                catch (Exception e) 
                { 
                    LogError("Error fetching repo: "+repoSave.url);
                    Log("The json file is broken! Please contact the repo maintainer!"); 
                    LogError(e);
                }
            }
        }
        catch (System.Exception e)
        {
                LogError("Error fetching repo: "+repoSave.url);
                LogError(e.Message);
                LogError("This is normal if you are not connected to the internet!");
        }

        return null;
    }
}*/