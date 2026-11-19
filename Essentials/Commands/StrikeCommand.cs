using Il2CppMonomiPark.SlimeRancher.World;

namespace Starlight.Commands;

internal class StrikeCommand : StarlightCommand
{
    public override string ID => "strike";
    public override string Usage => "strike [power]";
    public override CommandType type => CommandType.Fun | CommandType.Cheat;
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0)
            return new List<string> { "1", "1.5", "2" };
        return null;
    }
    static GameObject _lightningPrefab;
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            var newPrefab = Object.Instantiate(_lightningPrefab);
            newPrefab.transform.position = hit.point;
            if (args != null && args.Length == 1)
            {
                try { newPrefab.GetComponent<LightningStrike>().BlastPower = float.Parse(args[0]) * 2750f; }
                catch { return SendNotValidInt(args[0]); }
            }
            SendMessage(Tr("cmd.strike.success"));
            return true;
        }
        return SendNotLookingAtAnything();
    }
    public override void AfterGameContext(GameContext gameContext)
    {
        _lightningPrefab = Object.Instantiate(Get<LightningStrike>("LightningStrike").gameObject);
        _lightningPrefab.MakePrefab();
        _lightningPrefab.name = "InstantLightning";
        var l = _lightningPrefab.GetComponent<LightningStrike>();
        l.WarningTime = 0.5f;
        l.SpawnOptions.RemoveAt(0);
        l.SpawnOptions.RemoveAt(0);
        l._strikeTime = 8f;
        l.BlastPower = 2750f;
        l.BlastRadius = 9f;
    }
}