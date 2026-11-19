using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.UI;
using Starlight.Managers;

namespace Starlight.Commands;

internal class InfiniteHealthCommand : StarlightCommand
{
    public override string ID => "infhealth";
    public override string Usage => "infhealth";
    public override CommandType type => CommandType.Cheat;

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();
        if (!inGame) return SendLoadASaveFirst();

        if (StarlightSaveManager.inGameData.InfiniteHealthActive)
        {
            StarlightSaveManager.inGameData.InfiniteHealthActive = false;
            if (!_healthMeter) _healthMeter = GetInScene<HealthMeter>("Health Meter");
            _healthMeter.gameObject.active = true;
            SendMessage(Tr("cmd.infhealth.successnolonger"));
        }
        else
        {
            StarlightSaveManager.inGameData.InfiniteHealthActive = true;
            if (!_healthMeter) _healthMeter = GetInScene<HealthMeter>("Health Meter");
            _healthMeter.gameObject.active = false;
            SendMessage(Tr("cmd.infhealth.success"));
        }

        return true;
    }
    private static HealthMeter _healthMeter;
    public override void OnUICoreLoad()
    {
        ExecuteInTicks(() =>
        {
            _healthMeter = GetInScene<HealthMeter>("Health Meter");
            if (inGame && !StarlightCounterGateManager.srleActive && StarlightSaveManager.inGameData != null && StarlightSaveManager.inGameData.InfiniteHealthActive)
                _healthMeter.gameObject.active = false;
        },1);
    }
    [HarmonyPatch(typeof(PlayerModel), nameof(PlayerModel.LoseHealth))]
    internal class PlayerModelLoseHealthPatch
    {
        public static bool Prefix(PlayerState __instance, float health) {
            try { return !StarlightSaveManager.inGameData.InfiniteHealthActive; } catch { }
            return true;
        }
    }
}