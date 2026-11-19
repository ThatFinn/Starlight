using Starlight.Patches.Context;
using Starlight.Storage;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Starlight.Components.AssetBundle
{
    /// <summary>
    /// "RoundedUIImage" is a MonoBehaviour you can add every UI element
    /// It adds rounded corners to the UI element
    /// Use cornerRadius to adjust the corner size
    /// </summary>
    [InjectIntoIL]
    public class RoundedUIImage : MonoBehaviour
    {
        public float CornerRadius;

        private RectTransform rectTransform;
        private MaskableGraphic graphic;
        private Material roundedMaterial;
        private Vector4 textureUV = new Vector4(0, 0, 1, 1);

        private static readonly int ShaderRadiusID = Shader.PropertyToID("_CornerRadius");
        private static readonly int ShaderHalfSizeID = Shader.PropertyToID("_HalfSize");
        private static readonly int ShaderOuterUVID = Shader.PropertyToID("_OuterUV");

        private void Start()
        {
            var group = GetComponent<SortingGroup>();
            if (group != null)
            {
                CornerRadius = group.sortingOrder;
                group.enabled = false;
            }
        }
        private void OnEnable()
        {
            Start();
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
                roundedMaterial = new Material(SystemContextPatch.loadedShaders["UI/Starlight/Rounded"]);

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
            roundedMaterial.SetFloat(ShaderRadiusID, CornerRadius);
            roundedMaterial.SetVector(ShaderOuterUVID, textureUV);
        }
    }
}