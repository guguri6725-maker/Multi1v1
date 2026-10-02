using System;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class NodeDeclarationError
    {
        public string Message { get; }

        public Type Type { get; }

        public NodeDeclarationError(string message, Type type)
        {
            Message = message;
            Type = type;
        }

        public override string ToString()
        {
            return $"'{Type.Name}' is not a valid node : {Message}";
        }
    }
}
