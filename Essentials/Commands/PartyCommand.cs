using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
namespace Starlight.Commands;
internal class PartyCommand : StarlightCommand
{
    public override string ID => "party";
    public override string Usage => "party";
    public override CommandType type => CommandType.Fun;
    static Volume defaultVolume = null;
    static ColorAdjustments myAdjustments = null;
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(0,0)) return SendNoArguments();
        if (defaultVolume != null)
        {
            if (myAdjustments != null)
            {
                myAdjustments.hueShift.value = 0;
                myAdjustments.hueShift.overrideState = false;
                defaultVolume = null;
                SendMessage(Tr("cmd.party.success2"));
                return true;
            }
        }

        Volume volume = Get<Volume>("Default Volume");
        if (volume != null)
            if (volume.isGlobal)
            {
                defaultVolume = volume;
                SendMessage(Tr("cmd.party.success"));
                return true;
            }

        return SendError(Tr("cmd.party.novolume"));
    }

    public override void Update()
    {
        if (defaultVolume != null)
        {
            try
            {
                if (myAdjustments == null)
                {
                    if (defaultVolume.profile.Has<ColorAdjustments>())
                        defaultVolume.profile.Remove<ColorAdjustments>();
                    myAdjustments = defaultVolume.profile.Add<ColorAdjustments>();
                }
                
                float currentValue = myAdjustments.hueShift.GetValue<float>();
                currentValue += 80f * Time.deltaTime;
                if (currentValue > 180)
                    currentValue = -180;
                myAdjustments.hueShift.overrideState = true;
                myAdjustments.hueShift.m_Value = currentValue;
                myAdjustments.hueShift.value = currentValue;
            }
            catch {}
        }
    }
}