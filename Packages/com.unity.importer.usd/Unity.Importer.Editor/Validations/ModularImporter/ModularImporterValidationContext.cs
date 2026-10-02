using System.Collections.Generic;
using System.Linq;
using UnityEngine.Importer;
using UnityEngine.Importer.Validations;

namespace UnityEditor.Importer.Validation
{
    internal class ModularImporterValidationContext
    {
        public IReadOnlyList<IGraphValue> ImportSettingOverrides { get; }

        public List<BaseGraphValidationError> Errors { get; } = new();

        public IReadOnlyDictionary<string, IGraphValue> IdToImportSettings = new Dictionary<string, IGraphValue>();

        public ModularImporterValidationContext(ModularImporter importer)
        {
            ImportSettingOverrides = importer.ImportSettingOverrides;
            if (importer.Graph.asset != null)
                IdToImportSettings = importer.Graph.asset.ImportSettings.Where(i => i != null).ToDictionary(i => i.Id, i => i);
        }
    }
}
