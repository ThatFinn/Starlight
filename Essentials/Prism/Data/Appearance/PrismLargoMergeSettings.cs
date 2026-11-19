namespace Starlight.Prism.Data.Appearance;

public class PrismLargoMergeSettings
{
    public bool MergeComponents = true;
    public PrismBfMergeStrategy Body;
    public PrismBfMergeStrategy Face;
    public PrismColorMergeStrategy BaseColors;
    public PrismColorMergeStrategy TwinColors;
    public PrismColorMergeStrategy SloomberColors;

    public PrismLargoMergeSettings()
    {
        Body = PrismBfMergeStrategy.Optimal;
        Face = PrismBfMergeStrategy.Optimal;
        BaseColors = PrismColorMergeStrategy.Optimal;
        TwinColors = PrismColorMergeStrategy.Optimal;
        SloomberColors = PrismColorMergeStrategy.Optimal;
    }

    public PrismLargoMergeSettings(bool mergeComponents,PrismBfMergeStrategy body, PrismBfMergeStrategy face, PrismColorMergeStrategy baseColors, PrismColorMergeStrategy twinColors, PrismColorMergeStrategy sloomberColors)
    {
        this.MergeComponents=mergeComponents;
        this.Body = body;
        this.Face = face;
        this.BaseColors = baseColors;
        this.TwinColors = twinColors;
        this.SloomberColors = sloomberColors;
    }
}