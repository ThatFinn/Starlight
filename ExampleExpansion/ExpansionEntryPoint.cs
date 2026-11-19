using Starlight.Enums;
using Starlight.Expansion;
using Starlight.Storage;

namespace StarlightExampleExpansion;

// This is required to load the expansion
// One mod.dll can have multiple Starlight expansions
[StarlightLoadExpansion()]
public class ExpansionEntryPoint : StarlightExpansionV01
{
    protected override StarlightPackageInfo info => new ()
    {
        ID = "com.devname.modname",
        Name = "Example Expansion",
        Author = "YourName",
        Description = "Example description",
        Version = "1.0.0",
        /// Optionally use these arguments
        
        //CoAuthors = new []{"coauthor1","coauthor2"},
        //Contributors = new []{"contributor1","contributor2"},
        //SourceCode = "https://gitsite.com/SomeUserName/SomeRepo",
        //Nexus = "https://www.nexusmods.com/slimerancher2/mods/0",
        //Discord = "https://discord.gg/someserver",
        //UsePrism = true,
        //IconPath = "Assets/otherpath.png", // By default, the path is "Assets/icon.png"
        
        /// Changing these to the necessity, may allow the expansion to be loaded, unloaded and updated without restarting SR2
        
        LoadTime = ExpansionLoadTime.Startup,
            //Startup (default) means the expansion has to be loaded at startup
            //BeforeGameContext means the expansion has to be loaded before GameContext.Start
            //InMainMenu means the expansion can be loaded anytime, except while a save is open
            //Anytime means the expansion can be loaded anytime
        UnloadTime = ExpansionUnloadTime.Never,
            //Never (default) means the expansion cannot be unloaded
            //InMainMenu means the expansion can be unloaded anytime, except while a save is open
            //Anytime means the expansion can be unloaded anytime
        
        /// This is used for the multiplayer mod
        MultiplayerRequirement = MultiplayerRequirement.ServerAndClient,
            //ServerAndClient (default) means the expansion needs to be installed on both the server and the client
            //Server means the expansion only needs to be installed on the server
            //Client means the expansion only needs to be installed on the client
            //Stop means you cannot host nor join while this mod is installed
    };

    public override void OnInitialize()
    {
        AddLanguages(EmbeddedResourceEUtil.LoadString("Assets.translations.csv"));
    }
}