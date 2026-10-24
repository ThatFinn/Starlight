using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Starlight.Prism.Data.Appearance;
using Starlight.Prism.Data.Native;
using Starlight.Prism.Wrappers;

namespace Starlight.Prism.Lib;

/// <summary>
/// Provides methods for merging various game-related data structures.
/// </summary>
public static class PrismLibMerging
{
    /// <summary>
    /// Merges two SlimeDiets into a new one, combining their properties without duplicates.
    /// </summary>
    /// <param name="firstDiet">The first diet to merge.</param>
    /// <param name="secondDiet">The second diet to merge.</param>
    /// <returns>A new SlimeDiet containing the merged properties.</returns>
    public static SlimeDiet MergeDiet(SlimeDiet firstDiet, SlimeDiet secondDiet)
    {
        var mergedDiet = PrismLibDiet.CreateNewDiet();

        mergedDiet.EatMap = mergedDiet.EatMap.AddRangeNoMultipleToNew(firstDiet.EatMap);
        mergedDiet.EatMap = mergedDiet.EatMap.AddRangeNoMultipleToNew(secondDiet.EatMap);

        mergedDiet.AdditionalFoodIdents = mergedDiet.AdditionalFoodIdents.AddRangeNoMultipleToNew(firstDiet.AdditionalFoodIdents);
        mergedDiet.AdditionalFoodIdents = mergedDiet.AdditionalFoodIdents.AddRangeNoMultipleToNew(secondDiet.AdditionalFoodIdents);

        mergedDiet.FavoriteIdents = mergedDiet.FavoriteIdents.AddRangeNoMultipleToNew(firstDiet.FavoriteIdents);
        mergedDiet.FavoriteIdents = mergedDiet.FavoriteIdents.AddRangeNoMultipleToNew(secondDiet.FavoriteIdents);

        mergedDiet.MajorFoodIdentifiableTypeGroups = mergedDiet.MajorFoodIdentifiableTypeGroups.AddRangeNoMultipleToNew(firstDiet.MajorFoodIdentifiableTypeGroups);
        mergedDiet.MajorFoodIdentifiableTypeGroups = mergedDiet.MajorFoodIdentifiableTypeGroups.AddRangeNoMultipleToNew(secondDiet.MajorFoodIdentifiableTypeGroups);

        mergedDiet.ProduceIdents = mergedDiet.ProduceIdents.AddRangeNoMultipleToNew(firstDiet.ProduceIdents);
        mergedDiet.ProduceIdents = mergedDiet.ProduceIdents.AddRangeNoMultipleToNew(secondDiet.ProduceIdents);

        return mergedDiet;
    }

    /// <summary>
    /// Merges the components from two GameObjects into a target GameObject.
    /// </summary>
    /// <param name="obj">The target GameObject to receive the new components.</param>
    /// <param name="oldOne">The first GameObject to get components from.</param>
    /// <param name="oldTwo">The second GameObject to get components from (optional).</param>
    public static void MergeComponentsV01(GameObject obj, GameObject oldOne, GameObject? oldTwo = null)
    {
        var components = new Dictionary<string, (MonoBehaviour, bool)>();

        foreach (var comp in obj.GetComponents<MonoBehaviour>())
            components.TryAdd(comp.GetIl2CppType().Name, (comp, false));

        foreach (var comp in oldOne.GetComponents<MonoBehaviour>())
            components.TryAdd(comp.GetIl2CppType().Name, (comp, true));

        if (oldTwo != null)
            foreach (var comp in oldTwo.GetComponents<MonoBehaviour>())
                components.TryAdd(comp.GetIl2CppType().Name, (comp, true));

        foreach (var component in components)
            if (component.Value.Item2 && !component.Key.ContainsAny("CrystalSlimeLaunch", "AweTowardsLargos", "SlimeEyeComponents", "SlimeMouthComponents", "ColliderTotemLinkerHelper"))
            {
                if (!obj.GetComponent(component.Value.Item1.GetIl2CppType()))
                {
                    var newComp = obj.AddComponent(component.Value.Item1.GetIl2CppType());
                    newComp.CopyFields(component.Value.Item1);
                    if (newComp.TryCast<Behaviour>() != null)
                        newComp.Cast<Behaviour>().enabled = true;
                }
            }
    }

    /// <summary>
    /// Merges the favorite toys of two slimes.
    /// </summary>
    /// <param name="slimeOne">The first slime.</param>
    /// <param name="slimeTwo">The second slime.</param>
    /// <returns>An Il2CppReferenceArray of ToyDefinition containing the merged favorite toys.</returns>
    public static Il2CppReferenceArray<ToyDefinition> MergeFavoriteToys(PrismSlime slimeOne, PrismSlime slimeTwo)
    {
        var toys = new List<ToyDefinition>();
        toys.AddRange(slimeOne.GetSlimeDefinition().FavoriteToyIdents);
        toys.AddRange(slimeTwo.GetSlimeDefinition().FavoriteToyIdents);
        return toys.ToArray();
    }

    /// <summary>
    /// Determines the optimal strategy for merging two slime definitions.
    /// </summary>
    /// <param name="one">The first slime definition.</param>
    /// <param name="two">The second slime definition.</param>
    /// <returns>The optimal merge strategy.</returns>
    public static PrismThreeMergeStrategy GetOptimalV01(SlimeDefinition one, SlimeDefinition two)
    {
        var prioritySlimes = new[] { "Saber", "Hyper", "Phosphor", "Gold", "Shadow" };
        if (prioritySlimes.Any(s => one.ReferenceId.EndsWith(s))) return PrismThreeMergeStrategy.PrioritizeFirst;
        if (prioritySlimes.Any(s => two.ReferenceId.EndsWith(s))) return PrismThreeMergeStrategy.PrioritizeSecond;

        if (one.ReferenceId.EndsWith("Flutter")) return PrismThreeMergeStrategy.PrioritizeSecond;
        if (two.ReferenceId.EndsWith("Flutter")) return PrismThreeMergeStrategy.PrioritizeFirst;

        return PrismThreeMergeStrategy.Merge;
    }

    private static readonly bool LogCombineErrors = false;

    /// <summary>
    /// Merges two appearance structures together for a largo.
    /// </summary>
    /// <param name="slime1">Base slime #1</param>
    /// <param name="slime2">Base slime #2</param>
    /// <param name="settings">Settings that determine what structures this function merges, and what colors.</param>
    /// <param name="optimalPrioritization">The optimal prioritization strategy.</param>
    /// <returns>A new array of structures.</returns>
    public static Il2CppReferenceArray<SlimeAppearanceStructure> MergeStructuresV01(SlimeAppearance slime1, SlimeAppearance slime2,
        PrismLargoMergeSettings settings, PrismThreeMergeStrategy optimalPrioritization)
    {
        var newStructures = new List<SlimeAppearanceStructure>();
        var palettes = GetPalettes(slime1, slime2);

        var useTwinShader = GetLargoHasTwinEffect(slime1) || GetLargoHasTwinEffect(slime2);
        var useBoomShader = GetLargoHasBoomEffect(slime1, slime2);
        var boomMat = useBoomShader ? PrismNativeBaseSlime.Boom.GetPrismBaseSlime().GetSlimeAppearance()._structures[0]?.DefaultMaterials[0] : null;
        var useHyperShader = GetLargoHasHyperEffect(slime1, slime2);
        var useSloomberShader = GetLargoHasSloomberEffect(slime1) || GetLargoHasSloomberEffect(slime2);
        var sloomberMat = useSloomberShader ? PrismNativeBaseSlime.Sloomber.GetPrismBaseSlime().GetSlimeAppearance()._structures[0]?.DefaultMaterials[0] : null;

        var firstBody = ShouldUseFirstStructure(settings.Body, !GetLargoHasDefaultBody(slime1));
        var firstFace = ShouldUseFirstStructure(settings.Face, !GetLargoHasDefaultFace(slime1));

        var colorMergeStrategies = DetermineColorMergeStrategies(settings, optimalPrioritization, GetLargoHasTwinEffect(slime1), GetLargoHasTwinEffect(slime2), GetLargoHasSloomberEffect(slime1), GetLargoHasSloomberEffect(slime2));

        ProcessStructures(slime1.Structures, firstBody, firstFace, newStructures, palettes, colorMergeStrategies, useBoomShader, boomMat, useHyperShader, useTwinShader, useSloomberShader, sloomberMat);
        ProcessStructures(slime2.Structures, !firstBody, !firstFace, newStructures, palettes, colorMergeStrategies, useBoomShader, boomMat, useHyperShader, useTwinShader, useSloomberShader, sloomberMat);

        return new Il2CppReferenceArray<SlimeAppearanceStructure>(newStructures.ToArray());
    }

    private static (SlimeAppearance.Palette, SlimeAppearance.Palette, SlimeAppearance.Palette, SlimeAppearance.Palette, SlimeAppearance.Palette, SlimeAppearance.Palette) GetPalettes(SlimeAppearance slime1, SlimeAppearance slime2)
    {
        return (
            slime1.GetMainPalette(), slime1.GetTwinPalette(), slime1.GetSloomberPalette(),
            slime2.GetMainPalette(), slime2.GetTwinPalette(), slime2.GetSloomberPalette()
        );
    }

    internal static bool ShouldUseFirstStructure(PrismBfMergeStrategy strategy, bool optimalCondition)
    {
        switch (strategy)
        {
            case PrismBfMergeStrategy.KeepFirst: return true;
            case PrismBfMergeStrategy.KeepSecond: return false;
            case PrismBfMergeStrategy.Optimal: return optimalCondition;
            default: return false;
        }
    }

    private static Dictionary<string, bool> DetermineColorMergeStrategies(PrismLargoMergeSettings settings, PrismThreeMergeStrategy optimalPrioritization, bool oneHasTwin, bool twoHasTwin, bool oneHasSloomber, bool twoHasSloomber)
    {
        var strategies = new Dictionary<string, bool>();
        strategies["pFirst"] = settings.BaseColors == PrismColorMergeStrategy.PrioritizeFirst || (settings.BaseColors == PrismColorMergeStrategy.Optimal && optimalPrioritization == PrismThreeMergeStrategy.PrioritizeFirst);
        strategies["pSecond"] = settings.BaseColors == PrismColorMergeStrategy.PrioritizeSecond || (settings.BaseColors == PrismColorMergeStrategy.Optimal && optimalPrioritization == PrismThreeMergeStrategy.PrioritizeSecond);
        strategies["pMerge"] = settings.BaseColors == PrismColorMergeStrategy.Merge || (settings.BaseColors == PrismColorMergeStrategy.Optimal && optimalPrioritization == PrismThreeMergeStrategy.Merge);

        strategies["pTwinFirst"] = settings.TwinColors == PrismColorMergeStrategy.PrioritizeFirst || (settings.TwinColors == PrismColorMergeStrategy.Optimal && ((oneHasTwin && !twoHasTwin) || optimalPrioritization == PrismThreeMergeStrategy.PrioritizeFirst));
        strategies["pTwinSecond"] = settings.TwinColors == PrismColorMergeStrategy.PrioritizeSecond || (settings.TwinColors == PrismColorMergeStrategy.Optimal && ((!oneHasTwin && twoHasTwin) || optimalPrioritization == PrismThreeMergeStrategy.PrioritizeSecond));
        strategies["pTwinMerge"] = settings.TwinColors == PrismColorMergeStrategy.Merge || (settings.TwinColors == PrismColorMergeStrategy.Optimal && optimalPrioritization == PrismThreeMergeStrategy.Merge);

        strategies["pSloomberFirst"] = settings.SloomberColors == PrismColorMergeStrategy.PrioritizeFirst || (settings.SloomberColors == PrismColorMergeStrategy.Optimal && ((oneHasSloomber && !twoHasSloomber) || optimalPrioritization == PrismThreeMergeStrategy.PrioritizeFirst));
        strategies["pSloomberSecond"] = settings.SloomberColors == PrismColorMergeStrategy.PrioritizeSecond || (settings.SloomberColors == PrismColorMergeStrategy.Optimal && ((!oneHasSloomber && twoHasSloomber) || optimalPrioritization == PrismThreeMergeStrategy.PrioritizeSecond));
        strategies["pSloomberMerge"] = settings.SloomberColors == PrismColorMergeStrategy.Merge || (settings.SloomberColors == PrismColorMergeStrategy.Optimal && optimalPrioritization == PrismThreeMergeStrategy.Merge);

        return strategies;
    }

    private static void ProcessStructures(Il2CppReferenceArray<SlimeAppearanceStructure> structures, bool processBody, bool processFace, List<SlimeAppearanceStructure> newStructures,
        (SlimeAppearance.Palette firstColor, SlimeAppearance.Palette firstColorTwin, SlimeAppearance.Palette firstColorSloomber, SlimeAppearance.Palette secondColor, SlimeAppearance.Palette secondColorTwin, SlimeAppearance.Palette secondColorSloomber) palettes,
        Dictionary<string, bool> colorMergeStrategies, bool useBoomShader, Material boomMat, bool useHyperShader, bool useTwinShader, bool useSloomberShader, Material sloomberMat)
    {
        foreach (var structure in structures)
        {
            bool isFace = structure.Element.Type == SlimeAppearanceElement.ElementType.FACE || structure.Element.Type == SlimeAppearanceElement.ElementType.FACE_ATTACH;
            bool isBody = structure.Element.Type == SlimeAppearanceElement.ElementType.BODY;

            if ((isFace && processFace) || (isBody && processBody) || (!isFace && !isBody))
            {
                if (structure != null && !newStructures.Any(s => s.Element == structure.Element) && structure.DefaultMaterials.Length != 0)
                {
                    var newStructure = new SlimeAppearanceStructure(structure);
                    newStructures.Add(newStructure);
                    var mat = Object.Instantiate(structure.DefaultMaterials[0]);
                    newStructure.DefaultMaterials = new List<Material>() { mat }.ToArray();

                    if (structure.Element.Type == SlimeAppearanceElement.ElementType.FOREHEAD) continue;

                    try
                    {
                        ApplyColors(mat, palettes, colorMergeStrategies);
                        ApplySpecialEffects(mat, useBoomShader, boomMat, useHyperShader, useTwinShader, useSloomberShader, sloomberMat, palettes, colorMergeStrategies);
                    }
                    catch (Exception e) { if (LogCombineErrors) LogError(e); }
                }
            }
        }
    }

    private static void ApplyColors(Material mat, (SlimeAppearance.Palette first, SlimeAppearance.Palette firstTwin, SlimeAppearance.Palette firstSloomber, SlimeAppearance.Palette second, SlimeAppearance.Palette secondTwin, SlimeAppearance.Palette secondSloomber) palettes, Dictionary<string, bool> strategies)
    {
        if (strategies["pFirst"])
            SetBaseColors(mat, palettes.first.Top, palettes.first.Middle, palettes.first.Bottom);
        else if (strategies["pSecond"])
            SetBaseColors(mat, palettes.second.Top, palettes.second.Middle, palettes.second.Bottom);
        else if (strategies["pMerge"])
            SetBaseColors(mat, Color.Lerp(palettes.first.Top, palettes.second.Top, 0.5f), Color.Lerp(palettes.first.Middle, palettes.second.Middle, 0.5f), Color.Lerp(palettes.first.Bottom, palettes.second.Bottom, 0.5f));
    }

    private static void SetBaseColors(Material mat, Color top, Color middle, Color bottom)
    {
        mat.SetColor("_TopColor", top);
        mat.SetColor("_MiddleColor", middle);
        mat.SetColor("_BottomColor", bottom);
        mat.SetColor("_SpecColor", middle);
    }

    private static void ApplySpecialEffects(Material mat, bool useBoom, Material boomMat, bool useHyper, bool useTwin, bool useSloomber, Material sloomberMat,
        (SlimeAppearance.Palette first, SlimeAppearance.Palette firstTwin, SlimeAppearance.Palette firstSloomber, SlimeAppearance.Palette second, SlimeAppearance.Palette secondTwin, SlimeAppearance.Palette secondSloomber) palettes, Dictionary<string, bool> strategies)
    {
        if (useBoom)
        {
            mat.SetTexture("_Cracks", boomMat.GetTexture("_Cracks"));
            mat.SetTexture("_CrackNoise", boomMat.GetTexture("_CrackNoise"));
            mat.EnableKeyword("_ENABLEBOOMCRACKS_ON");
        }
        if (useHyper) mat.EnableKeyword("_ENABLEHYPEREFFECT_ON");

        if (useTwin)
        {
            mat.EnableKeyword("_ENABLETWINEFFECT_ON");
            if (strategies["pTwinFirst"])
                SetTwinColors(mat, palettes.firstTwin.Top, palettes.firstTwin.Middle, palettes.firstTwin.Bottom);
            else if (strategies["pTwinSecond"])
                SetTwinColors(mat, palettes.secondTwin.Top, palettes.secondTwin.Middle, palettes.secondTwin.Bottom);
            else if (strategies["pTwinMerge"])
                SetTwinColors(mat, Color.Lerp(palettes.firstTwin.Top, palettes.secondTwin.Top, 0.5f), Color.Lerp(palettes.firstTwin.Middle, palettes.secondTwin.Middle, 0.5f), Color.Lerp(palettes.firstTwin.Bottom, palettes.secondTwin.Bottom, 0.5f));
        }

        if (useSloomber)
        {
            mat.SetTexture("_SloomberColorOverlay", sloomberMat.GetTexture("_SloomberColorOverlay"));
            mat.SetTexture("_SloomberStarMask", sloomberMat.GetTexture("_SloomberStarMask"));
            mat.EnableKeyword("_BODYCOLORING_SLOOMBER");
            mat.DisableKeyword("_BODYCOLORING_DEFAULT");

            if (strategies["pSloomberFirst"])
                SetSloomberColors(mat, palettes.firstSloomber.Top, palettes.firstSloomber.Bottom);
            else if (strategies["pSloomberSecond"])
                SetSloomberColors(mat, palettes.secondSloomber.Top, palettes.secondSloomber.Bottom);
            else if (strategies["pSloomberMerge"])
                SetSloomberColors(mat, Color.Lerp(palettes.firstSloomber.Top, palettes.secondSloomber.Top, 0.5f), Color.Lerp(palettes.firstSloomber.Bottom, palettes.secondSloomber.Bottom, 0.5f));
        }
    }

    private static void SetTwinColors(Material mat, Color top, Color middle, Color bottom)
    {
        mat.SetColor("_TwinTopColor", top);
        mat.SetColor("_TwinMiddleColor", middle);
        mat.SetColor("_TwinBottomColor", bottom);
    }

    private static void SetSloomberColors(Material mat, Color top, Color bottom)
    {
        mat.SetColor("_SloomberTopColor", top);
        mat.SetColor("_SloomberBottomColor", bottom);
    }

    private static SlimeAppearance.Palette GetTwinPalette(this SlimeAppearance app)
    {
        Material mat = app._structures.FirstOrDefault(s => s.Element.Type == SlimeAppearanceElement.ElementType.BODY)?.DefaultMaterials[0];
        if (mat == null) return new SlimeAppearance.Palette();
        return new SlimeAppearance.Palette { Ammo = Color.white, Top = mat.GetColor("_TwinTopColor"), Middle = mat.GetColor("_TwinMiddleColor"), Bottom = mat.GetColor("_TwinBottomColor") };
    }

    private static SlimeAppearance.Palette GetSloomberPalette(this SlimeAppearance app)
    {
        Material mat = app._structures.FirstOrDefault(s => s.Element.Type == SlimeAppearanceElement.ElementType.BODY)?.DefaultMaterials[0];
        if (mat == null) return new SlimeAppearance.Palette();
        return new SlimeAppearance.Palette { Ammo = Color.white, Top = mat.GetColor("_SloomberTopColor"), Bottom = mat.GetColor("_SloomberBottomColor") };
    }

    private static SlimeAppearance.Palette GetMainPalette(this SlimeAppearance app)
    {
        Material mat = app._structures.FirstOrDefault(s => s.Element.Type == SlimeAppearanceElement.ElementType.BODY)?.DefaultMaterials[0];
        if (mat == null) return new SlimeAppearance.Palette();
        return new SlimeAppearance.Palette { Ammo = Color.white, Top = mat.GetColor("_TopColor"), Middle = mat.GetColor("_MiddleColor"), Bottom = mat.GetColor("_BottomColor") };
    }

    private static bool GetLargoHasHyperEffect(SlimeAppearance slime1, SlimeAppearance slime2) =>
        HasKeyword(slime1, "_ENABLEHYPEREFFECT_ON") || HasKeyword(slime2, "_ENABLEHYPEREFFECT_ON");

    private static bool GetLargoHasDefaultBody(SlimeAppearance slime) =>
        slime._structures.Any(s => s.Element.Type == SlimeAppearanceElement.ElementType.BODY && s.Element.name.Contains("DefaultBody"));

    internal static bool GetLargoHasDefaultFace(SlimeAppearance slime) =>
        slime._structures.Any(s => s.Element.Type == SlimeAppearanceElement.ElementType.FACE && s.Element.name.Replace("(Clone)", "") == "SlimeFace");

    private static bool GetLargoHasBoomEffect(SlimeAppearance slime1, SlimeAppearance slime2) =>
        HasKeyword(slime1, "_ENABLEBOOMCRACKS_ON") || HasKeyword(slime2, "_ENABLEBOOMCRACKS_ON");

    private static bool GetLargoHasTwinEffect(SlimeAppearance slime) => HasKeyword(slime, "_ENABLETWINEFFECT_ON");

    private static bool GetLargoHasSloomberEffect(SlimeAppearance slime) => HasKeyword(slime, "_BODYCOLORING_SLOOMBER");

    private static bool HasKeyword(SlimeAppearance slime, string keyword)
    {
        try
        {
            return slime._structures.Any(s => s.Element.Type == SlimeAppearanceElement.ElementType.BODY && s.DefaultMaterials[0].IsKeywordEnabled(keyword));
        }
        catch { return false; }
    }
}