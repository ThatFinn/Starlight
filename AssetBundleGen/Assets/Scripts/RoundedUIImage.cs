using UnityEngine;
using UnityEngine.UI;

namespace Starlight.Components.AssetBundle
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform), typeof(MaskableGraphic))]
    [DisallowMultipleComponent]
    public class RoundedUIImage : MonoBehaviour
    {
        [Tooltip("Radius for all corners")]
        [UnityEngine.SerializeField] [Min(0)] public float cornerRadius = 10f;

        private RectTransform rectTransform;
        private MaskableGraphic graphic;
        private Material roundedMaterial;
        private Vector4 textureUV = new Vector4(0, 0, 1, 1);

        private static readonly int ShaderRadiusID = Shader.PropertyToID("_CornerRadius");
        private static readonly int ShaderHalfSizeID = Shader.PropertyToID("_HalfSize");
        private static readonly int ShaderOuterUVID = Shader.PropertyToID("_OuterUV");

        private void OnEnable()
        {
            Initialize();
            UpdateMaterial();
        }

        private void OnValidate()
        {
            Initialize();
            UpdateMaterial();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateMaterial();
        }

        private void OnDestroy()
        {
            if (graphic != null) graphic.material = null;
            if (roundedMaterial != null) DestroyImmediate(roundedMaterial);
        }

        private void Initialize()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (graphic == null) graphic = GetComponent<MaskableGraphic>();

            if (roundedMaterial == null)
                roundedMaterial = new Material(Shader.Find("UI/Starlight/Rounded"));

            if (graphic != null)
                graphic.material = roundedMaterial;

            if (graphic is Image img && img.sprite != null)
                textureUV = UnityEngine.Sprites.DataUtility.GetOuterUV(img.sprite);
        }

        private void UpdateMaterial()
        {
            if (roundedMaterial == null || rectTransform == null) return;

            Vector2 halfSize = rectTransform.rect.size * 0.5f;
            roundedMaterial.SetVector(ShaderHalfSizeID, halfSize);
            roundedMaterial.SetFloat(ShaderRadiusID, cornerRadius);
            roundedMaterial.SetVector(ShaderOuterUVID, textureUV);
        }
    }
}