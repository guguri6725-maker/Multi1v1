using pxr;

namespace Unity.Importer.USD
{
    internal static class UsdPreviewSurfaceTokens
    {
        public static readonly TfToken UsdPreviewSurface = new TfToken("UsdPreviewSurface");
        public static readonly TfToken DiffuseColor = new("diffuseColor");
        public static readonly TfToken EmissiveColor = new("emissiveColor");
        public static readonly TfToken SpecularColor = new("specularColor");
        public static readonly TfToken UseSpecularWorkflow = new("useSpecularWorkflow");
        public static readonly TfToken Metallic = new("metallic");
        public static readonly TfToken Roughness = new("roughness");
        public static readonly TfToken ClearCoat = new("clearcoat");
        public static readonly TfToken ClearCoatRoughness = new("clearcoatRoughness");
        public static readonly TfToken Opacity = new("opacity");
        public static readonly TfToken OpacityThreshold = new("opacityThreshold");
        public static readonly TfToken Normal = new("normal");
        public static readonly TfToken Displacement = new("displacement");
        public static readonly TfToken Occlusion = new("occlusion");
        public static readonly TfToken repeat = new("repeat");
        public static readonly TfToken mirror = new("mirror");
        public static readonly TfToken file = new("file");
        public static readonly TfToken varname = new("varname");
        public static readonly TfToken wrapS = new("wrapS");
        public static readonly TfToken wrapT = new("wrapT");
        public static readonly TfToken rotation = new("rotation");
        public static readonly TfToken scale = new("scale");
        public static readonly TfToken bias = new("bias");
        public static readonly TfToken translation = new("translation");
        public static readonly TfToken inputIn = new("in");
        public static readonly TfToken st = new("st");
        public static readonly TfToken UsdUVTexture = new("UsdUVTexture");
        public static readonly TfToken UsdTransform2d = new("UsdTransform2d");
    }
}
