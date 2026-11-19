using Il2CppMonomiPark.SlimeRancher.Slime;

namespace Starlight.Commands;

internal class EmotionsCommand : StarlightCommand
{
    public override string ID => "emotions";
    public override string Usage => "emotions <emotion> [value]";
    public override CommandType type => CommandType.Cheat | CommandType.Fun;

    List<string> arg0List = new List<string> { "hunger", "agitation", "fear", "sleepiness"};
    public override List<string> GetAutoComplete(int argIndex, string[] args)
    {
        if (argIndex == 0) return arg0List;
        if (argIndex == 1) return new List<string> { "0", "0.25", "0.5", "1" };
        return null;
    }
    public override bool Execute(string[] args)
    {
        if (!args.IsBetween(1,2)) return SendUsage();
        if (!inGame) return SendLoadASaveFirst();
        if (!arg0List.Contains(args[0])) return SendNotValidOption(args[0]);
        SlimeEmotions.Emotion emotion;
        switch (args[0])
        {
            case "hunger": emotion = SlimeEmotions.Emotion.HUNGER; break;
            case "agitation": emotion = SlimeEmotions.Emotion.AGITATION; break;
            case "fear": emotion = SlimeEmotions.Emotion.FEAR; break;
            case "sleepiness": emotion = SlimeEmotions.Emotion.SLEEPINESS; break;
            default: return SendUnknown();
        }
        Camera cam = MiscEUtil.GetActiveCamera(); if (!cam) return SendNoCamera();
        if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,MiscEUtil.defaultMask))
        {
            var slime = hit.collider.gameObject.GetComponent<SlimeEmotions>();
            if (slime != null)
            {
                if (args.Length == 1)
                {
                    SendMessage(Tr($"cmd.emotions.{args[0]}.show", slime.gameObject.GetComponent<Identifiable>().identType.GetName(), slime.Get(emotion)));
                    return true;
                }
                if (args.Length == 2)
                {
                    if (!TryParseFloat(args[1], out float newValue, 0,true)) return false;
                    if (newValue > 1) newValue = 1;
                    slime.Set(emotion, newValue);
                    SendMessage(Tr($"cmd.emotions.{args[0]}.edit", slime.gameObject.GetComponent<Identifiable>().identType.GetName(), newValue));
                    return true;
                }

                return SendErrorToManyArgs(ID);
            }
            return SendNotLookingAtValidObject();
        }
        return SendNotLookingAtAnything();
    }
}