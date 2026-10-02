using System.Collections.Generic;
using System.Text.RegularExpressions;
using pxr;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will filter a list of prims based on their path and a provided regex.
    /// </summary>
    [NodeMetadata("PrimsRegexFilterNode", 0)]
    public class PrimsRegexFilterNode : Node<PrimsRegexFilterNode.InputPort, PrimsRegexFilterNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="PrimsRegexFilterNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Usd prims to parse.
            /// </summary>
            public List<UsdPrim> prims;

            /// <summary>
            /// The regex to use for the parsing.
            /// </summary>
            public string regex;
        }

        /// <summary>
        /// Output ports of the <see cref="PrimsRegexFilterNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The prims that matched the regex.
            /// </summary>
            public List<UsdPrim> matchingPrims = new();

            /// <summary>
            /// The prims that did not match the regex.
            /// </summary>
            public List<UsdPrim> nonMatchingPrims = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var regex = new Regex(Input.regex);
            foreach (var prim in Input.prims)
            {
                if (regex.Match(prim.GetPath()).Success)
                {
                    Output.matchingPrims.Add(prim);
                }
                else
                {
                    Output.nonMatchingPrims.Add(prim);
                }
            }
        }
    }
}
