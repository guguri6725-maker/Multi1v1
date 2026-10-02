using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class NodeValidator : BaseGraphComponentValidator<INode<InputPorts, OutputPorts>>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<INode<InputPorts, OutputPorts>>> AddValidations => new List<INewGraphComponentValidation<INode<InputPorts, OutputPorts>>>
        {
            new NodeIsNotNullValidation(),
            new NodeIsUniqueValidation(),
            new NodeIsOfValidTypeValidation()
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<INode<InputPorts, OutputPorts>>> RemoveValidations => new List<IRemoveGraphComponentValidation<INode<InputPorts, OutputPorts>>>
        {
            new NodeIsNotNullValidation(),
            new NodePresentInGraphValidation()
        };

        protected override string GetAdditionErrorHeader(INode<InputPorts, OutputPorts> node) =>
            $"Could not add Node '{(node != null ? node : "null")}' to graph due to the following errors :";

        protected override string GetRemovalErrorHeader(INode<InputPorts, OutputPorts> node) =>
            $"Could not remove Node '{(node != null ? node : "null")}' due to the following errors :";
    }
}
