using Il2CppMonomiPark.SlimeRancher.Player.CharacterController.Abilities;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.UnitPropertySystem;
using Starlight.Managers;

namespace Starlight.Commands;

internal class InfiniteEnergyCommand : StarlightCommand
{
    public override string ID => "infenergy";
    public override string Usage => "infenergy [should disable height limit(true/false)]";
    public override CommandType type => CommandType.Cheat;

    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return new List<string> { "true", "false" };
        return null;
    }

    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,1)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();

        bool shouldDisableThrusterHeight = false;
        if (args != null) if (!TryParseBool(args[0], out shouldDisableThrusterHeight)) return false;

        if (StarlightSaveManager.inGameData.InfiniteEnergyActive)
        {
            StarlightSaveManager.inGameData.InfiniteEnergyActive = false;
            if (!_energyMeter) _energyMeter = GetInScene<EnergyMeter>("Energy Meter");
            _energyMeter.transform.GetChild(0).gameObject.SetActive(true);

            if (_jetpackAbilityData == null) _jetpackAbilityData = Get<JetpackAbilityData>("Jetpack");
            _jetpackAbilityData._hoverHeight = _normalHoverHeight;
            _jetpackAbilityData._maxUpwardThrustForce = _normalMaxUpwardThrustForce;
            _jetpackAbilityData._upwardThrustForceIncrement = _normalUpwardThrustForceIncrement;

            _energyMeter.maxEnergy = new NullableFloatProperty(_normalEnergy);
            sceneContext.PlayerState.SetEnergy(0);
            SendMessage(Tr("cmd.infenergy.successnolonger"));
        }
        else
        {
            StarlightSaveManager.inGameData.InfiniteEnergyActive = true;
            if (!_energyMeter) _energyMeter = GetInScene<EnergyMeter>("Energy Meter");
            _energyMeter.transform.GetChild(0).gameObject.SetActive(false);

            if (_jetpackAbilityData == null) _jetpackAbilityData = Get<JetpackAbilityData>("Jetpack");
            _normalHoverHeight = _jetpackAbilityData._hoverHeight;
            _normalMaxUpwardThrustForce = _jetpackAbilityData._maxUpwardThrustForce;
            _normalUpwardThrustForceIncrement = _jetpackAbilityData._upwardThrustForceIncrement;
            if (shouldDisableThrusterHeight)
            {
                _jetpackAbilityData._hoverHeight = float.MaxValue;
                _jetpackAbilityData._maxUpwardThrustForce = 5f;
                _jetpackAbilityData._upwardThrustForceIncrement = 5f;
            }

            try { sceneContext.PlayerState.SetEnergy(int.MaxValue); }catch { }
            _normalEnergy = _energyMeter.maxEnergy;
            _energyMeter.maxEnergy = new NullableFloatProperty(2.14748365E+09f);
            SendMessage(Tr("cmd.infenergy.success"));
        }
        return true;
    }

    public override void Update()
    {
        try
        {
            if (inGame && !StarlightCounterGateManager.srleActive && StarlightSaveManager.inGameData.InfiniteEnergyActive)
                sceneContext.PlayerState.SetEnergy(int.MaxValue);
        }
        catch { }
    }

    public override void OnPlayerCoreLoad() => _jetpackAbilityData = Get<JetpackAbilityData>("Jetpack");
    public override void OnUICoreLoad()
    {
        if (StarlightCounterGateManager.srleActive) return;
        ExecuteInTicks(() =>
        {
            _energyMeter = GetInScene<EnergyMeter>("Energy Meter");
            if(inGame && !StarlightCounterGateManager.srleActive && StarlightSaveManager.inGameData != null && StarlightSaveManager.inGameData.InfiniteEnergyActive)
                _energyMeter.gameObject.active = false;
        },1);
    }
    
    private static float _normalEnergy = 100;
    private static float _normalHoverHeight = 0;
    private static float _normalMaxUpwardThrustForce = 0;
    static float _normalUpwardThrustForceIncrement = 0;
    private static EnergyMeter _energyMeter;
    private static JetpackAbilityData _jetpackAbilityData;
}