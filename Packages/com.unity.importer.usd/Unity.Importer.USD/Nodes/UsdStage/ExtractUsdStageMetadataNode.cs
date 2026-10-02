using pxr;
using Unity.USD.Core;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will extract metadata from the imported file USD stage.
    /// </summary>
    [NodeMetadata("ExtractUsdStageMetadataNode", 2)]
    public class ExtractUsdStageMetadataNode : Node<ExtractUsdStageMetadataNode.InputPort, ExtractUsdStageMetadataNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ExtractUsdStageMetadataNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The USD stage of the imported USD file
            /// </summary>
            public UsdStage stage;
        }

        /// <summary>
        /// Output ports of the <see cref="ExtractUsdStageMetadataNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The extracted metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            UsdTimeCode timeCodesPerSecond = Input.stage.GetTimeCodesPerSecond();
            if (timeCodesPerSecond.GetValue() <= 0)
            {
                Input.GraphLogger.LogImportWarning(
                    $"The USD stage timeCodesPerSecond value is invalid : {timeCodesPerSecond.GetValue()}. Using 24 instead.",
                    null, NodeWarnings.InvalidTimeCodesPerSecond);
                timeCodesPerSecond = new UsdTimeCode(24.0f);
            }

            Output.usdMetadata = new UsdStageMetadata
            {
                isStageZup = IsStageZup(),
                metersPerUnit = GetStageMetersPerUnit(),
                startTimeCode = Input.stage.GetStartTimeCode(),
                timeCodesPerSecond = timeCodesPerSecond
            };
        }

        bool IsStageZup()
        {
            VtValue val = new VtValue();
            if (Input.stage.GetMetadata(UsdGeomTokens.upAxis, val) && !Vt.VtValueToTfToken(val).Equals(UsdGeomTokens.y))
            {
                return true;
            }
            return false;
        }

        static VtValue s_FloatVtValue = new VtValue(0.0f);
        float GetStageMetersPerUnit()
        {
            var val = new VtValue();
            if (Input.stage.GetMetadata(UsdGeomTokens.metersPerUnit, val))
                return VtValue.CastToTypeOf(val, s_FloatVtValue);

            return 0.01f;
        }
    }
}
