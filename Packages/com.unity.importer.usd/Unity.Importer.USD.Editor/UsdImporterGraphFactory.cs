using System.Collections.Generic;
using pxr;
using Unity.Importer.USD;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// A factory for creating in-memory importer graph for usd files.
    /// </summary>
    public class UsdImporterGraphFactory
    {
        /// <summary>
        /// Create a default in-memory <see cref="ImporterGraph"/> for importing usd files.
        /// </summary>
        /// <returns></returns>
        public static ImporterGraph CreateDefaultGraph()
        {
            var graph = ScriptableObject.CreateInstance<ImporterGraph>();

            //Node declaration :

            //Stage
            var usdStageOpenNode_stage = new UsdStageOpenNode();
            var extractUsdStageMetadataNode_stage = new ExtractUsdStageMetadataNode();

            //Xforms
            var getTfTypeByNameNode_xform = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_xform = new FilterStageByTfTypeNode();
            var readXformNode_xform = new ReadXFormNode();
            var createXformNode_xform = new CreateXFormNode();

            //Skeleton
            var getTfTypeByNameNode_blendShape = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_blendShape = new FilterStageByTfTypeNode();
            var getTfTypeByNameNode_skeleton = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_skeleton = new FilterStageByTfTypeNode();
            var readSkeletonRootNode_skeleton = new ReadSkeletonRootNode();
            var readXformNode_skeletonRoot = new ReadXFormNode();
            var createXformNode_skeletonRoot = new CreateXFormNode();
            var convertSkeletonRigToXFormableNode_skeleton = new ConvertSkeletonRigToXFormableNode();
            var createXformNode_skeleton = new CreateXFormNode();
            var aggregator_skeleton = new IDictionaryAggregatorNode<Dictionary<string, GameObject>>();

            //Mesh filtering
            var getTfTypeByNameNode_meshFiltering = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_meshFiltering = new FilterStageByTfTypeNode();
            var filterMeshTypeNode_meshFiltering = new FilterMeshTypeNode();
            var readMeshMaterialDescriptionNode_mesh = new ReadMeshMaterialDescriptionNode();
            var createMaterialFromMeshColorNode_material = new CreateMaterialFromMeshColorNode();
            var readVisibilityNode_mesh = new ReadVisibilityNode();

            //Meshes
            var readReferencesNode_mesh = new ReadReferencesNode();
            var readMeshNode_mesh = new ReadMeshNode();
            var duplicateVerticesRemovalNode_mesh = new DuplicateVerticesRemovalNode();
            var writeMeshNode_mesh = new WriteMeshNode();
            var computeNormalsAndTangentsNode_mesh = new ComputeNormalsAndTangentsNode();
            var readXformNode_mesh = new ReadXFormNode();
            var createXformNode_mesh = new CreateXFormNode();
            var addMeshFilterToGameObjectsNode_mesh = new AddMeshFilterToGameObjectsNode();

            //SkinnedMeshes
            var readReferencesNode_skinnedMesh = new ReadReferencesNode();
            var readSkinnedMeshNode_skinnedMesh = new ReadSkinnedMeshNode();
            var duplicateVerticesRemovalNode_skinnedMesh = new DuplicateVerticesRemovalNode();
            var writeMeshNode_skinnedMesh = new WriteMeshNode();
            var computeNormalsAndTangentsNode_skinnedMesh = new ComputeNormalsAndTangentsNode();
            var readXformNode_skinnedMesh = new ReadXFormNode();
            var createXformNode_skinnedMesh = new CreateXFormNode();
            var addSkinnedMeshRendererToGameObjectsNode_skinnedMesh = new AddSkinnedMeshRendererToGameObjectsNode();

            //Textures settings
            var createDiffuseImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createNormalImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createMetallicImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createRoughnessImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createEmissiveImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createOpacityImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createDisplacementImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createSpecularColorImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createOcclusionImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createClearcoatImportSettings_texture = new CreateTextureGenerationSettingsNode();
            var createClearcoatRoughnessImportSettings_texture = new CreateTextureGenerationSettingsNode();

            //Textures
            var createDiffuseColorTextureNode_texture = new CreateTextureNode();
            var createNormalTextureNode_texture = new CreateTextureNode();
            var createMetallicTextureNode_texture = new CreateTextureNode();
            var createRoughnessTextureNode_texture = new CreateTextureNode();
            var createEmissiveColorTextureNode_texture = new CreateTextureNode();
            var createOpacityTextureNode_texture = new CreateTextureNode();
            var createDisplacementTextureNode_texture = new CreateTextureNode();
            var createSpecularColorTextureNode_texture = new CreateTextureNode();
            var createOcclusionTextureNode_texture = new CreateTextureNode();
            var createClearcoatTextureNode_texture = new CreateTextureNode();
            var createClearcoatRoughnessTextureNode_texture = new CreateTextureNode();

            //Materials
            var getTfTypeByNameNode_material = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_material = new FilterStageByTfTypeNode();
            var readMaterialNode_material = new ReadMaterialNode();
            var createMaterialNode_material = new CreateMaterialNode();
            var mapTextureToMaterialNode_material = new MapTextureToMaterialNode();

            //Cameras
            var getTfTypeByNameNode_camera = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_camera = new FilterStageByTfTypeNode();
            var readCameraNode_camera = new ReadCameraNode();
            var readXformNode_camera = new ReadXFormNode();
            var createXformNode_camera = new CreateXFormNode();
            var createCameraNode_camera = new CreateCameraNode();

            //Distant Lights
            var getTfTypeByNameNode_distantLight = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_distantLight = new FilterStageByTfTypeNode();
            var readDistantLightNode_distantLight = new ReadDistantLightNode();
            var readXformNode_distantLight = new ReadXFormNode();
            var createXformNode_distantLight = new CreateXFormNode();
            var createDistantLightNode_distantLight = new CreateDistantLightNode();

            //Sphere Lights
            var getTfTypeByNameNode_sphereLight = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_sphereLight = new FilterStageByTfTypeNode();
            var readSphereLightNode_sphereLight = new ReadSphereLightNode();
            var readXformNode_sphereLight = new ReadXFormNode();
            var createXformNode_sphereLight = new CreateXFormNode();
            var createSphereLightNode_sphereLight = new CreateSphereLightNode();

            //Rect Lights
            var getTfTypeByNameNode_rectLight = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_rectLight = new FilterStageByTfTypeNode();
            var readRectLightNode_rectLight = new ReadRectLightNode();
            var readXformNode_rectLight = new ReadXFormNode();
            var createXformNode_rectLight = new CreateXFormNode();
            var createRectLightNode_rectLight = new CreateRectLightNode();

            //Disk Lights
            var getTfTypeByNameNode_diskLight = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_diskLight = new FilterStageByTfTypeNode();
            var readDiskLightNode_diskLight = new ReadDiskLightNode();
            var readXformNode_diskLight = new ReadXFormNode();
            var createXformNode_diskLight = new CreateXFormNode();
            var createDiskLightNode_diskLight = new CreateDiskLightNode();

            //Post Mesh
            var aggregator_postMesh = new IDictionaryAggregatorNode<Dictionary<string, GameObject>>();
            var materialsAggregator_postMesh = new IDictionaryAggregatorNode<Dictionary<string, Material>>();
            var mapMaterialToMeshNode_postMesh = new MapMaterialToMeshNode();

            //Hierarchy
            var aggregator_entry_hierarchy = new IDictionaryAggregatorNode<Dictionary<string, GameObject>>();
            var getTfTypeByNameNode_hierarchy = new GetTfTypeByNameNode();
            var filterMissingHierarchyNode_hierarchy = new FilterMissingHierarchyNode();
            var readXformNode_hierarchy = new ReadXFormNode();
            var createXformNode_hierarchy = new CreateXFormNode();
            var createEmptyObjectsNode_hierarchy = new CreateEmptyObjectsNode();
            var aggregator_final_hierarchy = new IDictionaryAggregatorNode<Dictionary<string, GameObject>>();
            var buildHierarchyNode_hierarchy = new BuildHierarchyNode();
            var disposeUsdImporter_hierarchy = new DisposeUsdImporterNode();

            //Xform animation
            var getTfTypeByNameNode_xformAnimation = new GetTfTypeByNameNode();
            var filterStageByTfTypeNode_xformAnimation = new FilterStageByTfTypeNode();
            var readXformAnimationNode_xformAnimation = new ReadXFormAnimationNode();

            //Animated properties
            var createAnimationClipNode_xformAnimation = new CreateAnimationClipNode();
            var animatedPropertiesAggregator = new IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>();
            var trimTransformPath = new TrimTransformPathNode();

            //Skel animation
            var filterStageByAppliedSchemaNode_skelAnimation = new FilterStageByAppliedSchemaNode();
            var resolveSkelBindingsNode_skelAnimation = new ResolveSkelBindingsNode();
            var readJointXformAnimationNode_skelAnimation = new ReadJointXformAnimationNode();
            var readBlendShapeWeightAnimationNode_skelAnimation = new ReadBlendShapeWeightAnimationNode();

            //Camera animation
            var readCameraAnimationNode_cameraAnimation = new ReadCameraAnimationNode();

            //Stage
            graph.AddNode(usdStageOpenNode_stage);
            graph.AddNode(extractUsdStageMetadataNode_stage);

            //Xforms
            graph.AddNode(getTfTypeByNameNode_xform);
            graph.AddNode(filterStageByTfTypeNode_xform);
            graph.AddNode(readXformNode_xform);
            graph.AddNode(createXformNode_xform);

            //Skeleton
            graph.AddNode(getTfTypeByNameNode_blendShape);
            graph.AddNode(filterStageByTfTypeNode_blendShape);
            graph.AddNode(getTfTypeByNameNode_skeleton);
            graph.AddNode(filterStageByTfTypeNode_skeleton);
            graph.AddNode(readSkeletonRootNode_skeleton);
            graph.AddNode(readXformNode_skeletonRoot);
            graph.AddNode(createXformNode_skeletonRoot);
            graph.AddNode(convertSkeletonRigToXFormableNode_skeleton);
            graph.AddNode(createXformNode_skeleton);
            graph.AddNode(aggregator_skeleton);

            //Mesh filtering
            graph.AddNode(getTfTypeByNameNode_meshFiltering);
            graph.AddNode(filterStageByTfTypeNode_meshFiltering);
            graph.AddNode(filterMeshTypeNode_meshFiltering);
            graph.AddNode(readMeshMaterialDescriptionNode_mesh);
            graph.AddNode(createMaterialFromMeshColorNode_material);
            graph.AddNode(readVisibilityNode_mesh);

            //Meshes
            graph.AddNode(readReferencesNode_mesh);
            graph.AddNode(readMeshNode_mesh);
            graph.AddNode(duplicateVerticesRemovalNode_mesh);
            graph.AddNode(writeMeshNode_mesh);
            graph.AddNode(computeNormalsAndTangentsNode_mesh);
            graph.AddNode(readXformNode_mesh);
            graph.AddNode(createXformNode_mesh);
            graph.AddNode(addMeshFilterToGameObjectsNode_mesh);

            //SkinnedMeshes
            graph.AddNode(readReferencesNode_skinnedMesh);
            graph.AddNode(readSkinnedMeshNode_skinnedMesh);
            graph.AddNode(duplicateVerticesRemovalNode_skinnedMesh);
            graph.AddNode(writeMeshNode_skinnedMesh);
            graph.AddNode(computeNormalsAndTangentsNode_skinnedMesh);
            graph.AddNode(readXformNode_skinnedMesh);
            graph.AddNode(createXformNode_skinnedMesh);
            graph.AddNode(addSkinnedMeshRendererToGameObjectsNode_skinnedMesh);

            //Textures
            graph.AddNode(createDiffuseImportSettings_texture);
            graph.AddNode(createNormalImportSettings_texture);
            graph.AddNode(createMetallicImportSettings_texture);
            graph.AddNode(createRoughnessImportSettings_texture);
            graph.AddNode(createEmissiveImportSettings_texture);
            graph.AddNode(createOpacityImportSettings_texture);
            graph.AddNode(createDisplacementImportSettings_texture);
            graph.AddNode(createSpecularColorImportSettings_texture);
            graph.AddNode(createOcclusionImportSettings_texture);
            graph.AddNode(createClearcoatImportSettings_texture);
            graph.AddNode(createClearcoatRoughnessImportSettings_texture);

            graph.AddNode(createDiffuseColorTextureNode_texture);
            graph.AddNode(createNormalTextureNode_texture);
            graph.AddNode(createMetallicTextureNode_texture);
            graph.AddNode(createRoughnessTextureNode_texture);
            graph.AddNode(createEmissiveColorTextureNode_texture);
            graph.AddNode(createOpacityTextureNode_texture);
            graph.AddNode(createDisplacementTextureNode_texture);
            graph.AddNode(createSpecularColorTextureNode_texture);
            graph.AddNode(createOcclusionTextureNode_texture);
            graph.AddNode(createClearcoatTextureNode_texture);
            graph.AddNode(createClearcoatRoughnessTextureNode_texture);

            //Materials
            graph.AddNode(getTfTypeByNameNode_material);
            graph.AddNode(filterStageByTfTypeNode_material);
            graph.AddNode(readMaterialNode_material);
            graph.AddNode(createMaterialNode_material);
            graph.AddNode(mapTextureToMaterialNode_material);

            //Cameras
            graph.AddNode(getTfTypeByNameNode_camera);
            graph.AddNode(filterStageByTfTypeNode_camera);
            graph.AddNode(readCameraNode_camera);
            graph.AddNode(readXformNode_camera);
            graph.AddNode(createXformNode_camera);
            graph.AddNode(createCameraNode_camera);

            // Distant lights
            graph.AddNode(getTfTypeByNameNode_distantLight);
            graph.AddNode(filterStageByTfTypeNode_distantLight);
            graph.AddNode(readDistantLightNode_distantLight);
            graph.AddNode(readXformNode_distantLight);
            graph.AddNode(createXformNode_distantLight);
            graph.AddNode(createDistantLightNode_distantLight);

            //Sphere Lights
            graph.AddNode(getTfTypeByNameNode_sphereLight);
            graph.AddNode(filterStageByTfTypeNode_sphereLight);
            graph.AddNode(readSphereLightNode_sphereLight);
            graph.AddNode(readXformNode_sphereLight);
            graph.AddNode(createXformNode_sphereLight);
            graph.AddNode(createSphereLightNode_sphereLight);

            //Rect Lights
            graph.AddNode(getTfTypeByNameNode_rectLight);
            graph.AddNode(filterStageByTfTypeNode_rectLight);
            graph.AddNode(readRectLightNode_rectLight);
            graph.AddNode(readXformNode_rectLight);
            graph.AddNode(createXformNode_rectLight);
            graph.AddNode(createRectLightNode_rectLight);

            //Disk Lights
            graph.AddNode(getTfTypeByNameNode_diskLight);
            graph.AddNode(filterStageByTfTypeNode_diskLight);
            graph.AddNode(readDiskLightNode_diskLight);
            graph.AddNode(readXformNode_diskLight);
            graph.AddNode(createXformNode_diskLight);
            graph.AddNode(createDiskLightNode_diskLight);

            //Post Mesh
            graph.AddNode(aggregator_postMesh);
            graph.AddNode(materialsAggregator_postMesh);
            graph.AddNode(mapMaterialToMeshNode_postMesh);

            //Hierarchy
            graph.AddNode(aggregator_entry_hierarchy);
            graph.AddNode(getTfTypeByNameNode_hierarchy);
            graph.AddNode(filterMissingHierarchyNode_hierarchy);
            graph.AddNode(readXformNode_hierarchy);
            graph.AddNode(createXformNode_hierarchy);
            graph.AddNode(createEmptyObjectsNode_hierarchy);
            graph.AddNode(aggregator_final_hierarchy);
            graph.AddNode(buildHierarchyNode_hierarchy);
            graph.AddNode(disposeUsdImporter_hierarchy);

            //Xform animation
            graph.AddNode(getTfTypeByNameNode_xformAnimation);
            graph.AddNode(filterStageByTfTypeNode_xformAnimation);
            graph.AddNode(readXformAnimationNode_xformAnimation);

            //Animated properties
            graph.AddNode(createAnimationClipNode_xformAnimation);
            graph.AddNode(animatedPropertiesAggregator);
            graph.AddNode(trimTransformPath);

            //Skel animation
            graph.AddNode(filterStageByAppliedSchemaNode_skelAnimation);
            graph.AddNode(resolveSkelBindingsNode_skelAnimation);
            graph.AddNode(readJointXformAnimationNode_skelAnimation);
            graph.AddNode(readBlendShapeWeightAnimationNode_skelAnimation);

            //Camera animation
            graph.AddNode(readCameraAnimationNode_cameraAnimation);

            //Import settings declaration :
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.PreserveSceneRoot, false));
            graph.AddImportSetting(new ImportSetting<float>(UsdImporterImportSettings.LightIntensityMultiplier, 1.0f));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.RecalculateMeshNormals, false));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.RecalculateMeshTangents, false));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.LoopAnimations, false));

            // Texture Import Settings section
            graph.AddImportSetting(new ImportSetting<TextureImporterAlphaSource>(UsdImporterImportSettings.TexturesAlphaSource, TextureImporterAlphaSource.None));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.TexturesAlphaIsTransparency, false));
            graph.AddImportSetting(new ImportSetting<TextureImporterNPOTScale>(UsdImporterImportSettings.TexturesNPOTScale, TextureImporterNPOTScale.None));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.TexturesAreReadable, false));
            graph.AddImportSetting(new ImportSetting<MipMapSettings>(UsdImporterImportSettings.TexturesMipMap, new MipMapSettings()
            {
                enabled = true,
                mode = TextureImporterMipFilter.BoxFilter,
                fadeOut = false,
                border = false,
                preserveCoverage = false,
                alphaCutoff = 0.5f,
                fadeDistanceStart = 1,
                fadeDistanceEnd = 3,
                enableStreaming = false,
                streamingPriority = 0,
                ignoreMipMapLimit = false,
            }));
            graph.AddImportSetting(new ImportSetting<TextureWrapModeSettings>(UsdImporterImportSettings.TexturesWrapMode, new() { all = TextureWrapMode.Repeat }));
            graph.AddImportSetting(new ImportSetting<FilterMode>(UsdImporterImportSettings.TexturesFilterMode, FilterMode.Bilinear));
            graph.AddImportSetting(new ImportSetting<int>(UsdImporterImportSettings.TexturesAnisoLevel, 1));
            graph.AddImportSetting(new ImportSetting<int>(UsdImporterImportSettings.TexturesMaxSize, 4096));
            graph.AddImportSetting(new ImportSetting<TextureResizeAlgorithm>(UsdImporterImportSettings.TexturesResizeAlgorithm, TextureResizeAlgorithm.Bilinear));
            graph.AddImportSetting(new ImportSetting<TextureImporterFormat>(UsdImporterImportSettings.TexturesFormat, TextureImporterFormat.Automatic));
            graph.AddImportSetting(new ImportSetting<TextureImporterCompression>(UsdImporterImportSettings.TexturesCompression, TextureImporterCompression.Compressed));
            graph.AddImportSetting(new ImportSetting<CrunchedCompression>(UsdImporterImportSettings.TexturesCrunchCompression, new()
            {
                useCrunchedCompression = false,
                compressionQuality = TextureCompressionQuality.Normal,
            }));
            graph.AddImportSetting(new ImportSetting<float>(UsdImporterImportSettings.TexturesMipBias, 0f));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.TexturesPNGIgnoreGamma, false));
            // Normal maps specific settings
            graph.AddImportSetting(new ImportSetting<NormalMapFromGrayScale>(UsdImporterImportSettings.NormalMapsCreateFromGrayscale, new()
            {
                createFromGrayScale = false,
                heightmapScale = 0.25f,
                normalMapFilter = TextureImporterNormalFilter.Standard,
            }));
            graph.AddImportSetting(new ImportSetting<bool>(UsdImporterImportSettings.NormalMapsFlipGreenChannel, false));

            //Shared constants declaration :
            var primvarToVertexAttributeMapping = new PrimvarToVertexAttributeMapping();

            //Edges declaration :

            //############ STAGE :

            //extractUsdStageMetadataNode_stage
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Input.stage)));

            //############ XFORMS :

            //getTfTypeByNameNode_xform
            graph.AddImportConstant(new ImportConstant<string>("UsdGeomXform", getTfTypeByNameNode_xform,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_xform
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_xform, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_xform, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_xform, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readXformNode_xform
            graph.AddEdge(new Edge(filterStageByTfTypeNode_xform, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_xform, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readXformNode_xform,
                nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_xform
            graph.AddEdge(new Edge(readXformNode_xform, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_xform, nameof(CreateXFormNode.Input.xFormableDescriptions)));


            //############ SKELETON

            //getTfTypeByNameNode_blendShape
            graph.AddImportConstant(new ImportConstant<string>("UsdSkelBlendShape", getTfTypeByNameNode_blendShape,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_blendShape
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_blendShape, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_blendShape, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_blendShape, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //getTfTypeByNameNode_skeleton
            graph.AddImportConstant(new ImportConstant<string>("UsdSkelRoot", getTfTypeByNameNode_skeleton,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_skeleton
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_skeleton, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_skeleton, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_skeleton, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readSkeletonRootNode_skeleton
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Input.usdMetadata)));
            graph.AddEdge(new Edge(filterStageByTfTypeNode_skeleton, nameof(FilterStageByTfTypeNode.Output.primList),
                readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Input.skeletonPrims)));
            graph.AddEdge(new Edge(filterStageByTfTypeNode_blendShape, nameof(FilterStageByTfTypeNode.Output.primList),
                readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Input.blendShapes)));
            graph.AddEdge(new Edge(resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Output.skinlessSkeletons),
                readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Input.skinlessSkeletons)));

            //readXformNode_skeletonRoot
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_skeletonRoot, nameof(ReadXFormNode.Input.usdMetadata)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.skeletonRootPrims),
                readXformNode_skeletonRoot, nameof(ReadXFormNode.Input.prims)));

            //createXformNode_skeletonRoot
            graph.AddEdge(new Edge(readXformNode_skeletonRoot, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_skeletonRoot, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //convertSkeletonRigToXFormableNode_skeleton
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.rigDescriptions),
                convertSkeletonRigToXFormableNode_skeleton, nameof(ConvertSkeletonRigToXFormableNode.Input.rigDescriptions)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                convertSkeletonRigToXFormableNode_skeleton, nameof(ConvertSkeletonRigToXFormableNode.Input.usdMetadata)));

            //createXformNode_skeleton
            graph.AddEdge(new Edge(convertSkeletonRigToXFormableNode_skeleton, nameof(ConvertSkeletonRigToXFormableNode.Output.xFormableDescriptions),
                createXformNode_skeleton, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //aggregator_skeleton
            graph.AddEdge(new Edge(createXformNode_skeleton, nameof(CreateXFormNode.Output.gameObjects),
                aggregator_skeleton, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_0)));
            graph.AddEdge(new Edge(createXformNode_skeletonRoot, nameof(CreateXFormNode.Output.gameObjects),
                aggregator_skeleton, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_1)));

            //############ MESH FILTERING

            //getTfTypeByNameNode_meshFiltering
            graph.AddImportConstant(new ImportConstant<string>("UsdGeomMesh", getTfTypeByNameNode_meshFiltering,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_meshFiltering
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_meshFiltering, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_meshFiltering, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_meshFiltering, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //filterMeshTypeNode_meshFiltering
            graph.AddEdge(new Edge(filterStageByTfTypeNode_meshFiltering, nameof(FilterStageByTfTypeNode.Output.primList),
                filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Input.meshPrims)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.skinnedMeshData),
                filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Input.skinnedMeshData)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.usdSkelBlendShapes),
                filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Input.usdSkelBlendShapes)));

            //readMeshMaterialDescriptionNode_mesh
            graph.AddEdge(new Edge(filterStageByTfTypeNode_meshFiltering, nameof(FilterStageByTfTypeNode.Output.primList),
                readMeshMaterialDescriptionNode_mesh, nameof(ReadMeshMaterialDescriptionNode.Input.prims)));
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                readMeshMaterialDescriptionNode_mesh, nameof(ReadMeshMaterialDescriptionNode.Input.stage)));

            //createMaterialFromMeshColorNode_material
            graph.AddEdge(new Edge(readMeshMaterialDescriptionNode_mesh, nameof(ReadMeshMaterialDescriptionNode.Output.displayColorMaterialDescriptions),
                createMaterialFromMeshColorNode_material, nameof(CreateMaterialFromMeshColorNode.Input.displayColorMaterialDescriptions)));
            graph.AddImportConstant(new ImportConstant<Shader>(
                AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.importer.usd/Shaders/usd_preview_surface.shadergraph"),
                createMaterialFromMeshColorNode_material, nameof(CreateMaterialFromMeshColorNode.Input.usdPreviewSurfaceShader)));

            //readVisibilityNode_mesh
            graph.AddEdge(new Edge(filterStageByTfTypeNode_meshFiltering, nameof(FilterStageByTfTypeNode.Output.primList),
                readVisibilityNode_mesh, nameof(ReadVisibilityNode.Input.prims)));

            //############ MESH :

            //readReferencesNode_mesh
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.defaultMeshPrim),
                readReferencesNode_mesh, nameof(ReadReferencesNode.Input.prims)));

            //readMeshNode_mesh
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readMeshNode_mesh,
                nameof(ReadMeshNode.Input.UsdMetadata)));
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.defaultMeshPrim),
                readMeshNode_mesh, nameof(ReadMeshNode.Input.UsdMeshes)));
            graph.AddImportConstant(new ImportConstant<PrimvarToVertexAttributeMapping>(primvarToVertexAttributeMapping, readMeshNode_mesh,
                nameof(ReadMeshNode.Input.PrimVarMapping)));

            //duplicateVerticesRemovalNode_mesh
            graph.AddEdge(new Edge(readMeshNode_mesh, nameof(ReadMeshNode.Output.PathToMeshDescription),
                duplicateVerticesRemovalNode_mesh, nameof(DuplicateVerticesRemovalNode.Input.pathToMeshDescription)));

            //writeMeshNode_mesh
            graph.AddEdge(new Edge(duplicateVerticesRemovalNode_mesh, nameof(DuplicateVerticesRemovalNode.Output.pathToMeshDescription),
                writeMeshNode_mesh, nameof(WriteMeshNode.Input.pathToMeshDescription)));

            //computeNormalsAndTangentsNode_mesh
            graph.AddEdge(new Edge(writeMeshNode_mesh, nameof(WriteMeshNode.Output.pathToUnityMesh),
                computeNormalsAndTangentsNode_mesh, nameof(ComputeNormalsAndTangentsNode.Input.pathToUnityMesh)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.RecalculateMeshNormals, computeNormalsAndTangentsNode_mesh,
                nameof(ComputeNormalsAndTangentsNode.Input.recalculateNormals)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.RecalculateMeshTangents, computeNormalsAndTangentsNode_mesh,
                nameof(ComputeNormalsAndTangentsNode.Input.recalculateTangents)));

            //readXformNode_mesh
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.defaultMeshPrim),
                readXformNode_mesh, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readXformNode_mesh,
                nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_mesh
            graph.AddEdge(new Edge(readXformNode_mesh, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_mesh, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //addMeshFilterToGameObjectsNode_mesh
            graph.AddEdge(new Edge(createXformNode_mesh, nameof(CreateXFormNode.Output.gameObjects),
                addMeshFilterToGameObjectsNode_mesh, nameof(AddMeshFilterToGameObjectsNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readReferencesNode_mesh, nameof(ReadReferencesNode.Output.objsToRefs),
                addMeshFilterToGameObjectsNode_mesh, nameof(AddMeshFilterToGameObjectsNode.Input.objsToRefs)));
            graph.AddEdge(new Edge(computeNormalsAndTangentsNode_mesh, nameof(ComputeNormalsAndTangentsNode.Output.pathToUnityMesh),
                addMeshFilterToGameObjectsNode_mesh, nameof(AddMeshFilterToGameObjectsNode.Input.pathToUnityMesh)));
            graph.AddEdge(new Edge(readVisibilityNode_mesh, nameof(ReadVisibilityNode.Output.invisiblePaths),
                addMeshFilterToGameObjectsNode_mesh, nameof(AddMeshFilterToGameObjectsNode.Input.invisiblePaths)));

            //############ SKINNED MESHES

            //readReferencesNode_skinnedMesh
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.skinnedMeshPrim),
                readReferencesNode_skinnedMesh, nameof(ReadReferencesNode.Input.prims)));

            //readSkinnedMeshNode_skinnedMesh
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.usdSkelBlendShapes), readSkinnedMeshNode_skinnedMesh,
                nameof(ReadSkinnedMeshNode.Input.UsdSkelBlendShapes)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.skinnedMeshData), readSkinnedMeshNode_skinnedMesh,
                nameof(ReadSkinnedMeshNode.Input.SkinnedMeshData)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readSkinnedMeshNode_skinnedMesh,
                nameof(ReadSkinnedMeshNode.Input.usdMetadata)));
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.skinnedMeshPrim),
                readSkinnedMeshNode_skinnedMesh, nameof(ReadSkinnedMeshNode.Input.UsdMeshes)));
            graph.AddImportConstant(new ImportConstant<PrimvarToVertexAttributeMapping>(primvarToVertexAttributeMapping, readSkinnedMeshNode_skinnedMesh,
                nameof(ReadSkinnedMeshNode.Input.PrimVarMapping)));

            //duplicateVerticesRemovalNode_skinnedMesh
            graph.AddEdge((new Edge(readSkinnedMeshNode_skinnedMesh,
                nameof(ReadMeshNode.Output.PathToMeshDescription), duplicateVerticesRemovalNode_skinnedMesh,
                nameof(DuplicateVerticesRemovalNode.Input.pathToMeshDescription))));

            //writeMeshNode_skinnedMesh
            graph.AddEdge(new Edge(duplicateVerticesRemovalNode_skinnedMesh, nameof(DuplicateVerticesRemovalNode.Output.pathToMeshDescription),
                writeMeshNode_skinnedMesh, nameof(WriteMeshNode.Input.pathToMeshDescription)));

            //computeNormalsAndTangentsNode_skinnedMesh
            graph.AddEdge(new Edge(writeMeshNode_skinnedMesh, nameof(WriteMeshNode.Output.pathToUnityMesh),
                computeNormalsAndTangentsNode_skinnedMesh, nameof(ComputeNormalsAndTangentsNode.Input.pathToUnityMesh)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.RecalculateMeshNormals, computeNormalsAndTangentsNode_skinnedMesh,
                nameof(ComputeNormalsAndTangentsNode.Input.recalculateNormals)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.RecalculateMeshTangents, computeNormalsAndTangentsNode_skinnedMesh,
                nameof(ComputeNormalsAndTangentsNode.Input.recalculateTangents)));

            //readXformNode_skinnedMesh
            graph.AddEdge(new Edge(filterMeshTypeNode_meshFiltering, nameof(FilterMeshTypeNode.Output.skinnedMeshPrim),
                readXformNode_skinnedMesh, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readXformNode_skinnedMesh,
                nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_skinnedMesh
            graph.AddEdge(new Edge(readXformNode_skinnedMesh, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_skinnedMesh, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //addSkinnedMeshRendererToGameObjectsNode_skinnedMesh
            graph.AddEdge(new Edge(createXformNode_skinnedMesh, nameof(CreateXFormNode.Output.gameObjects),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.meshGameObjects)));
            graph.AddEdge(new Edge(aggregator_skeleton, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.skeletonGameObjects)));
            graph.AddEdge(new Edge(readReferencesNode_skinnedMesh, nameof(ReadReferencesNode.Output.objsToRefs),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.objsToRefs)));
            graph.AddEdge(new Edge(computeNormalsAndTangentsNode_skinnedMesh, nameof(ComputeNormalsAndTangentsNode.Output.pathToUnityMesh),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.pathToUnityMesh)));
            graph.AddEdge(new Edge(readVisibilityNode_mesh, nameof(ReadVisibilityNode.Output.invisiblePaths),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.invisiblePaths)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.skinnedMeshData),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.skinnedMeshData)));
            graph.AddEdge(new Edge(readSkeletonRootNode_skeleton, nameof(ReadSkeletonRootNode.Output.rigDescriptions),
                addSkinnedMeshRendererToGameObjectsNode_skinnedMesh, nameof(AddSkinnedMeshRendererToGameObjectsNode.Input.rigDescriptions)));


            //############ TEXTURES

            //createDiffuseImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createDiffuseImportSettings_texture, TextureImporterType.Default, true);

            //createNormalImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createNormalImportSettings_texture, TextureImporterType.NormalMap, false);
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.NormalMapsCreateFromGrayscale, createNormalImportSettings_texture,
                nameof(CreateTextureGenerationSettingsNode.Input.createFromGrayScale)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.NormalMapsFlipGreenChannel, createNormalImportSettings_texture,
                nameof(CreateTextureGenerationSettingsNode.Input.flipGreenChannel)));

            //createMetallicImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createMetallicImportSettings_texture, TextureImporterType.Default, false);

            //createRoughnessImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createRoughnessImportSettings_texture, TextureImporterType.Default, false);

            //createEmissiveImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createEmissiveImportSettings_texture, TextureImporterType.Default, true);

            //createOpacityImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createOpacityImportSettings_texture, TextureImporterType.Default, false);

            //createDisplacementImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createDisplacementImportSettings_texture, TextureImporterType.Default, false);

            //createSpecularColorImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createSpecularColorImportSettings_texture, TextureImporterType.Default, true);

            //createOcclusionImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createOcclusionImportSettings_texture, TextureImporterType.Default, false);

            //createClearcoatImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createClearcoatImportSettings_texture, TextureImporterType.Default, false);

            //createClearcoatRoughnessImportSettings_texture
            ConnectTextureSettingGenerationInputs(graph, createClearcoatRoughnessImportSettings_texture, TextureImporterType.Default, false);

            //createDiffuseColorTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.diffuseColorTextureSamplers),
                createDiffuseColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createDiffuseImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createDiffuseColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createNormalTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.normalTextureSamplers),
                createNormalTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createNormalImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createNormalTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createMetallicTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.metallicTextureSamplers),
                createMetallicTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createMetallicImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createMetallicTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createRoughnessTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.roughnessTextureSamplers),
                createRoughnessTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createRoughnessImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createRoughnessTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createEmissiveColorTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.emissiveColorTextureSamplers),
                createEmissiveColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createEmissiveImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createEmissiveColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createOpacityTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.opacityTextureSamplers),
                createOpacityTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createOpacityImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createOpacityTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createDisplacementTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.displacementTextureSamplers),
                createDisplacementTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createDisplacementImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createDisplacementTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createSpecularColorTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.specularColorTextureSamplers),
                createSpecularColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createSpecularColorImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createSpecularColorTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createOcclusionTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.occlusionTextureSamplers),
                createOcclusionTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createOcclusionImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createOcclusionTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createClearcoatTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.clearCoatTextureSamplers),
                createClearcoatTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createClearcoatImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createClearcoatTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));

            //createClearcoatRoughnessTextureNode_texture
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.clearCoatRoughnessTextureSamplers),
                createClearcoatRoughnessTextureNode_texture, nameof(CreateTextureNode.Input.textureSamplers)));
            graph.AddEdge(new Edge(createClearcoatRoughnessImportSettings_texture, nameof(CreateTextureGenerationSettingsNode.Output.settings),
                createClearcoatRoughnessTextureNode_texture, nameof(CreateTextureNode.Input.textureSettings)));


            //############ MATERIALS

            //getTfTypeByNameNode_material
            graph.AddImportConstant(new ImportConstant<string>("UsdShadeMaterial", getTfTypeByNameNode_material,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_material
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_material, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_material, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_material, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readMaterialNode_material
            graph.AddEdge(new Edge(filterStageByTfTypeNode_material, nameof(FilterStageByTfTypeNode.Output.primList),
                readMaterialNode_material, nameof(ReadMaterialNode.Input.usdMaterials)));

            //createMaterialNode_material
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.materialDescriptions),
                createMaterialNode_material, nameof(CreateMaterialNode.Input.materialDescriptions)));
            graph.AddImportConstant(new ImportConstant<Shader>(
                AssetDatabase.LoadAssetAtPath<Shader>(
                    "Packages/com.unity.importer.usd/Shaders/usd_preview_surface.shadergraph"),
                createMaterialNode_material, nameof(CreateMaterialNode.Input.usdPreviewSurfaceShader)));
            graph.AddImportConstant(new ImportConstant<PrimvarToVertexAttributeMapping>(primvarToVertexAttributeMapping, createMaterialNode_material,
                nameof(CreateMaterialNode.Input.primVarMapping)));

            //mapTextureToMaterialNode_material
            graph.AddEdge(new Edge(createMaterialNode_material, nameof(CreateMaterialNode.Output.materials),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.materials)));

            graph.AddEdge(new Edge(createDiffuseColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.diffuseColorTextures)));
            graph.AddEdge(new Edge(createNormalTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.normalTextures)));
            graph.AddEdge(new Edge(createMetallicTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.metallicTextures)));
            graph.AddEdge(new Edge(createRoughnessTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.roughnessTextures)));
            graph.AddEdge(new Edge(createEmissiveColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.emissiveColorTextures)));
            graph.AddEdge(new Edge(createOpacityTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.opacityTextures)));
            graph.AddEdge(new Edge(createDisplacementTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.displacementTextures)));
            graph.AddEdge(new Edge(createSpecularColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.specularColorTextures)));
            graph.AddEdge(new Edge(createOcclusionTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.occlusionTextures)));
            graph.AddEdge(new Edge(createClearcoatTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.clearCoatTextures)));
            graph.AddEdge(new Edge(createClearcoatRoughnessTextureNode_texture, nameof(CreateTextureNode.Output.texturesBySamplerId),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.clearCoatRoughnessTextures)));
            graph.AddEdge(new Edge(readMaterialNode_material, nameof(ReadMaterialNode.Output.materialDescriptions),
                mapTextureToMaterialNode_material, nameof(MapTextureToMaterialNode.Input.materialDescriptions)));


            //############ CAMERAS

            //getTfTypeByNameNode_camera
            graph.AddImportConstant(new ImportConstant<string>("UsdGeomCamera", getTfTypeByNameNode_camera,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_camera
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_camera, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_camera, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_camera, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readCameraNode_camera
            graph.AddEdge(new Edge(filterStageByTfTypeNode_camera, nameof(FilterStageByTfTypeNode.Output.primList),
                readCameraNode_camera, nameof(ReadCameraNode.Input.usdCameras)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readCameraNode_camera, nameof(ReadCameraNode.Input.usdMetadata)));

            //readXformNode_Camera
            graph.AddEdge(new Edge(filterStageByTfTypeNode_camera, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_camera, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_camera, nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_Camera
            graph.AddEdge(new Edge(readXformNode_camera, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_camera, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //createCameraNode_camera
            graph.AddEdge(new Edge(createXformNode_camera, nameof(CreateXFormNode.Output.gameObjects),
                createCameraNode_camera, nameof(CreateCameraNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readCameraNode_camera, nameof(ReadCameraNode.Output.cameras),
                createCameraNode_camera, nameof(CreateCameraNode.Input.cameras)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createCameraNode_camera, nameof(CreateCameraNode.Input.usdMetadata)));


            //############ DISTANT LIGHTS

            //getTfTypeByNameNode_distantLight
            graph.AddImportConstant(new ImportConstant<string>("UsdLuxDistantLight", getTfTypeByNameNode_distantLight,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_distantLight
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_distantLight, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_distantLight, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_distantLight, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readDistantLightNode_distantLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_distantLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readDistantLightNode_distantLight, nameof(ReadDistantLightNode.Input.usdDistantLights)));

            //readXformNode_distantLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_distantLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_distantLight, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_distantLight, nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_distantLight
            graph.AddEdge(new Edge(readXformNode_distantLight, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_distantLight, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //createDistantLightNode_distantLight
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.LightIntensityMultiplier,
                createDistantLightNode_distantLight, nameof(CreateDistantLightNode.Input.lightIntensityMultiplier)));
            graph.AddEdge(new Edge(createXformNode_distantLight, nameof(CreateXFormNode.Output.gameObjects),
                createDistantLightNode_distantLight, nameof(CreateDistantLightNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readDistantLightNode_distantLight, nameof(ReadDistantLightNode.Output.distantLights),
                createDistantLightNode_distantLight, nameof(CreateDistantLightNode.Input.distantLights)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createDistantLightNode_distantLight, nameof(CreateDistantLightNode.Input.usdMetadata)));


            //############ SPHERE LIGHTS

            //getTfTypeByNameNode_sphereLight
            graph.AddImportConstant(new ImportConstant<string>("UsdLuxSphereLight", getTfTypeByNameNode_sphereLight,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_sphereLight
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_sphereLight, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_sphereLight, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_sphereLight, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readSphereLightNode_sphereLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_sphereLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readSphereLightNode_sphereLight, nameof(ReadSphereLightNode.Input.usdSphereLights)));

            //readXformNode_sphereLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_sphereLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_sphereLight, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_sphereLight, nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_sphereLight
            graph.AddEdge(new Edge(readXformNode_sphereLight, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_sphereLight, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //createSphereLightNode_sphereLight
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.LightIntensityMultiplier,
                createSphereLightNode_sphereLight, nameof(CreateSphereLightNode.Input.lightIntensityMultiplier)));
            graph.AddEdge(new Edge(createXformNode_sphereLight, nameof(CreateXFormNode.Output.gameObjects),
                createSphereLightNode_sphereLight, nameof(CreateSphereLightNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readSphereLightNode_sphereLight, nameof(ReadSphereLightNode.Output.sphereLights),
                createSphereLightNode_sphereLight, nameof(CreateSphereLightNode.Input.sphereLights)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createSphereLightNode_sphereLight, nameof(CreateSphereLightNode.Input.usdMetadata)));


            //############ RECT LIGHTS

            //getTfTypeByNameNode_rectLight
            graph.AddImportConstant(new ImportConstant<string>("UsdLuxRectLight", getTfTypeByNameNode_rectLight,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_rectLight
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_rectLight, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_rectLight, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_rectLight, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readrectLightNode_rectLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_rectLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readRectLightNode_rectLight, nameof(ReadRectLightNode.Input.usdRectLights)));

            //readXformNode_rectLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_rectLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_rectLight, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_rectLight, nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_rectLight
            graph.AddEdge(new Edge(readXformNode_rectLight, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_rectLight, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //createrectLightNode_rectLight
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.LightIntensityMultiplier,
                createRectLightNode_rectLight, nameof(CreateRectLightNode.Input.lightIntensityMultiplier)));
            graph.AddEdge(new Edge(createXformNode_rectLight, nameof(CreateXFormNode.Output.gameObjects),
                createRectLightNode_rectLight, nameof(CreateRectLightNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readRectLightNode_rectLight, nameof(ReadRectLightNode.Output.rectLights),
                createRectLightNode_rectLight, nameof(CreateRectLightNode.Input.rectLights)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createRectLightNode_rectLight, nameof(CreateRectLightNode.Input.usdMetadata)));


            //############ DISK LIGHTS

            //getTfTypeByNameNode_diskLight
            graph.AddImportConstant(new ImportConstant<string>("UsdLuxDiskLight", getTfTypeByNameNode_diskLight,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_diskLight
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_diskLight, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_diskLight, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_diskLight, nameof(FilterStageByTfTypeNode.Input.tfType)));

            //readdiskLightNode_diskLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_diskLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readDiskLightNode_diskLight, nameof(ReadDiskLightNode.Input.usdDiskLights)));

            //readXformNode_diskLight
            graph.AddEdge(new Edge(filterStageByTfTypeNode_diskLight, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformNode_diskLight, nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformNode_diskLight, nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_diskLight
            graph.AddEdge(new Edge(readXformNode_diskLight, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_diskLight, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //creatediskLightNode_diskLight
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.LightIntensityMultiplier,
                createDiskLightNode_diskLight, nameof(CreateDiskLightNode.Input.lightIntensityMultiplier)));
            graph.AddEdge(new Edge(createXformNode_diskLight, nameof(CreateXFormNode.Output.gameObjects),
                createDiskLightNode_diskLight, nameof(CreateDiskLightNode.Input.gameObjects)));
            graph.AddEdge(new Edge(readDiskLightNode_diskLight, nameof(ReadDiskLightNode.Output.diskLights),
                createDiskLightNode_diskLight, nameof(CreateDiskLightNode.Input.diskLights)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createDiskLightNode_diskLight, nameof(CreateDiskLightNode.Input.usdMetadata)));


            //############ POST MESH

            //aggregator_postMesh
            graph.AddEdge(new Edge(addMeshFilterToGameObjectsNode_mesh,
                nameof(AddMeshFilterToGameObjectsNode.Output.gameObjects), aggregator_postMesh,
                nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_0)));
            graph.AddEdge(new Edge(addSkinnedMeshRendererToGameObjectsNode_skinnedMesh,
                nameof(AddMeshFilterToGameObjectsNode.Output.gameObjects), aggregator_postMesh,
                nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_1)));

            //materialsAggregator_postMesh
            graph.AddEdge(new Edge(mapTextureToMaterialNode_material,
                nameof(MapTextureToMaterialNode.Output.materials), materialsAggregator_postMesh,
                nameof(IDictionaryAggregatorNode<Dictionary<string, Material>>.Input.inputs_0)));
            graph.AddEdge(new Edge(createMaterialFromMeshColorNode_material,
                nameof(CreateMaterialFromMeshColorNode.Output.materials), materialsAggregator_postMesh,
                nameof(IDictionaryAggregatorNode<Dictionary<string, Material>>.Input.inputs_1)));

            //mapMaterialToMeshNode_postMesh
            graph.AddEdge(new Edge(materialsAggregator_postMesh, nameof(IDictionaryAggregatorNode<Dictionary<string, Material>>.Output.output),
                mapMaterialToMeshNode_postMesh, nameof(MapMaterialToMeshNode.Input.materials)));
            graph.AddEdge(new Edge(readMeshMaterialDescriptionNode_mesh,
                nameof(ReadMeshMaterialDescriptionNode.Output.meshMaterialsPaths), mapMaterialToMeshNode_postMesh,
                nameof(MapMaterialToMeshNode.Input.meshMaterialsPaths)));
            graph.AddEdge(new Edge(aggregator_postMesh,
                nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output), mapMaterialToMeshNode_postMesh,
                nameof(MapMaterialToMeshNode.Input.meshGameobjects)));


            //############ HIERARCHY :

            //aggregator_entry_hierarchy
            graph.AddEdge(new Edge(createXformNode_xform, nameof(CreateXFormNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_0)));
            graph.AddEdge(new Edge(mapMaterialToMeshNode_postMesh, nameof(MapMaterialToMeshNode.Output.meshGameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_1)));
            graph.AddEdge(new Edge(aggregator_skeleton, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_2)));
            graph.AddEdge(new Edge(createCameraNode_camera, nameof(CreateCameraNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_3)));
            graph.AddEdge(new Edge(createDistantLightNode_distantLight, nameof(CreateDistantLightNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_4)));
            graph.AddEdge(new Edge(createSphereLightNode_sphereLight, nameof(CreateSphereLightNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_5)));
            graph.AddEdge(new Edge(createRectLightNode_rectLight, nameof(CreateRectLightNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_6)));
            graph.AddEdge(new Edge(createDiskLightNode_diskLight, nameof(CreateDiskLightNode.Output.gameObjects),
                aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_7)));

            //getTfTypeByNameNode_hierarchy
            graph.AddImportConstant(new ImportConstant<string>("UsdGeomXformable", getTfTypeByNameNode_hierarchy,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterMissingHierarchyNode_hierarchy
            graph.AddEdge(new Edge(getTfTypeByNameNode_hierarchy, nameof(GetTfTypeByNameNode.Output.tfType),
                filterMissingHierarchyNode_hierarchy, nameof(FilterMissingHierarchyNode.Input.filterTfType)));
            graph.AddEdge(new Edge(aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                filterMissingHierarchyNode_hierarchy, nameof(FilterMissingHierarchyNode.Input.gameObjects)));
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterMissingHierarchyNode_hierarchy, nameof(FilterMissingHierarchyNode.Input.stage)));

            //readXformNode_hierarchy
            graph.AddEdge(new Edge(filterMissingHierarchyNode_hierarchy,
                nameof(FilterMissingHierarchyNode.Output.PrimOfTfType), readXformNode_hierarchy,
                nameof(ReadXFormNode.Input.prims)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata), readXformNode_hierarchy,
                nameof(ReadXFormNode.Input.usdMetadata)));

            //createXformNode_hierarchy
            graph.AddEdge(new Edge(readXformNode_hierarchy, nameof(ReadXFormNode.Output.xFormableDescriptions),
                createXformNode_hierarchy, nameof(CreateXFormNode.Input.xFormableDescriptions)));

            //createEmptyObjectsNode_hierarchy
            graph.AddEdge(new Edge(filterMissingHierarchyNode_hierarchy,
                nameof(FilterMissingHierarchyNode.Output.PrimOfNonTfType), createEmptyObjectsNode_hierarchy,
                nameof(CreateEmptyObjectsNode.Input.prims)));

            //aggregator_final_hierarchy
            graph.AddEdge(new Edge(createXformNode_hierarchy, nameof(CreateEmptyObjectsNode.Output.gameObjects),
                aggregator_final_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_0)));
            graph.AddEdge(new Edge(createEmptyObjectsNode_hierarchy, nameof(CreateXFormNode.Output.gameObjects),
                aggregator_final_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_1)));
            graph.AddEdge(new Edge(aggregator_entry_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                aggregator_final_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Input.inputs_2)));

            //buildHierarchyNode_hierarchy
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.PreserveSceneRoot, buildHierarchyNode_hierarchy,
                nameof(BuildHierarchyNode.Input.preserveSceneRoot)));
            graph.AddEdge(new Edge(aggregator_final_hierarchy, nameof(IDictionaryAggregatorNode<Dictionary<string, GameObject>>.Output.output),
                buildHierarchyNode_hierarchy, nameof(BuildHierarchyNode.Input.gameObjects)));
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                buildHierarchyNode_hierarchy, nameof(BuildHierarchyNode.Input.stage)));

            //disposeUsdImporter_hierarchy_hierarchy
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                disposeUsdImporter_hierarchy, nameof(DisposeUsdImporterNode.Input.stage)));
            graph.AddEdge(new Edge(buildHierarchyNode_hierarchy, nameof(BuildHierarchyNode.Output.root),
                disposeUsdImporter_hierarchy, nameof(DisposeUsdImporterNode.Input.root)));


            //############ XFORM ANIMATION :
            //getTfTypeByNameNode_xformAnimation
            graph.AddImportConstant(new ImportConstant<string>("UsdGeomXformable", getTfTypeByNameNode_xformAnimation,
                nameof(GetTfTypeByNameNode.Input.tfTypeName)));

            //filterStageByTfTypeNode_xformAnimation
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByTfTypeNode_xformAnimation, nameof(FilterStageByTfTypeNode.Input.stage)));
            graph.AddEdge(new Edge(getTfTypeByNameNode_xformAnimation, nameof(GetTfTypeByNameNode.Output.tfType),
                filterStageByTfTypeNode_xformAnimation, nameof(FilterStageByTfTypeNode.Input.tfType)));

            // readXformAnimNode_xformAnimation
            graph.AddEdge(new Edge(filterStageByTfTypeNode_xformAnimation, nameof(FilterStageByTfTypeNode.Output.primList),
                readXformAnimationNode_xformAnimation, nameof(ReadXFormAnimationNode.Input.geomXformables)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readXformAnimationNode_xformAnimation,
                nameof(ReadXFormAnimationNode.Input.usdMetadata)));

            // createAnimationClipNode_xformAnimation
            graph.AddEdge(new Edge(trimTransformPath, nameof(TrimTransformPathNode.Output.animatedProperties),
                createAnimationClipNode_xformAnimation, nameof(CreateAnimationClipNode.Input.animatedProperties)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.LoopAnimations, createAnimationClipNode_xformAnimation,
                nameof(CreateAnimationClipNode.Input.loopAnimations)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                createAnimationClipNode_xformAnimation, nameof(CreateAnimationClipNode.Input.usdMetadata)));

            //############ SKEL ANIMATION :
            // filterStageByAppliedSchemaNode_skelAnimation
            graph.AddImportConstant(new ImportConstant<string>("SkelBindingAPI", filterStageByAppliedSchemaNode_skelAnimation,
                nameof(FilterStageByAppliedSchemaNode.Input.schemaName)));
            graph.AddEdge(new Edge(usdStageOpenNode_stage, nameof(UsdStageOpenNode.Output.stage),
                filterStageByAppliedSchemaNode_skelAnimation, nameof(FilterStageByAppliedSchemaNode.Input.stage)));

            // resolveSkelBindingsNode_skelAnimation
            graph.AddEdge(new Edge(filterStageByAppliedSchemaNode_skelAnimation, nameof(FilterStageByTfTypeNode.Output.primList),
                resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Input.primsWithSkelBindingAPI)));
            graph.AddEdge(new Edge(filterStageByTfTypeNode_skeleton, nameof(FilterStageByTfTypeNode.Output.primList),
                resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Input.skelRoots)));

            // readJointXformAnimationNode_skelAnimation
            graph.AddEdge(new Edge(resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Output.skelAnimToSkelSkeletons),
                readJointXformAnimationNode_skelAnimation, nameof(readJointXformAnimationNode_skelAnimation.Input.skelAnimToSkelSkeletons)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readJointXformAnimationNode_skelAnimation, nameof(ReadXFormAnimationNode.Input.usdMetadata)));

            // readBlendShapeWeightAnimationNode_skelAnimation
            graph.AddEdge(new Edge(resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Output.skelAnimToSkelSkeletons),
                readBlendShapeWeightAnimationNode_skelAnimation, nameof(readBlendShapeWeightAnimationNode_skelAnimation.Input.skelAnimToSkelSkeletons)));
            graph.AddEdge(new Edge(resolveSkelBindingsNode_skelAnimation, nameof(ResolveSkelBindingsNode.Output.skelAnimToSkinningQueryData),
                readBlendShapeWeightAnimationNode_skelAnimation, nameof(readBlendShapeWeightAnimationNode_skelAnimation.Input.skelAnimToSkinningQueryData)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readBlendShapeWeightAnimationNode_skelAnimation, nameof(ReadXFormAnimationNode.Input.usdMetadata)));

            // animatedPropertiesAggregator
            graph.AddEdge(new Edge(readXformAnimationNode_xformAnimation, nameof(ReadXFormAnimationNode.Output.animatedProperties),
                animatedPropertiesAggregator, nameof(IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>.Input.inputs_0)));
            graph.AddEdge(new Edge(readJointXformAnimationNode_skelAnimation, nameof(ReadJointXformAnimationNode.Output.animatedProperties),
                animatedPropertiesAggregator, nameof(IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>.Input.inputs_1)));
            graph.AddEdge(new Edge(readCameraAnimationNode_cameraAnimation, nameof(ReadCameraAnimationNode.Output.animatedProperties),
                animatedPropertiesAggregator, nameof(IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>.Input.inputs_2)));
            graph.AddEdge(new Edge(readBlendShapeWeightAnimationNode_skelAnimation, nameof(readBlendShapeWeightAnimationNode_skelAnimation.Output.animatedProperties),
                animatedPropertiesAggregator, nameof(IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>.Input.inputs_3)));

            // trimTransformPath
            graph.AddEdge(new Edge(buildHierarchyNode_hierarchy, nameof(BuildHierarchyNode.Output.root),
                trimTransformPath, nameof(TrimTransformPathNode.InputPort.root)));
            graph.AddEdge(new Edge(animatedPropertiesAggregator, nameof(IDictionaryAggregatorNode<Dictionary<EditorCurveBinding, Keyframe[]>>.Output.output),
                trimTransformPath, nameof(TrimTransformPathNode.InputPort.animatedProperties)));

            //############ CAMERA ANIMATION :
            // readCameraAnimationNode_cameraAnimation
            graph.AddEdge(new Edge(filterStageByTfTypeNode_camera, nameof(FilterStageByTfTypeNode.Output.primList),
                readCameraAnimationNode_cameraAnimation, nameof(ReadCameraAnimationNode.Input.usdCameras)));
            graph.AddEdge(new Edge(extractUsdStageMetadataNode_stage, nameof(ExtractUsdStageMetadataNode.Output.usdMetadata),
                readCameraAnimationNode_cameraAnimation, nameof(ReadCameraAnimationNode.Input.usdMetadata)));

            //############ RESULTS :
            graph.AddResultEdge(new ResultEdge(nameof(BuildHierarchyNode.Output.root), disposeUsdImporter_hierarchy,
                nameof(BuildHierarchyNode.Output.root)));
            graph.AddResultEdge(new ResultEdge($"DiffuseColor{nameof(CreateTextureNode.Output.texturesByPath)}",
                createDiffuseColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Normal{nameof(CreateTextureNode.Output.texturesByPath)}",
                createNormalTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Metallic{nameof(CreateTextureNode.Output.texturesByPath)}",
                createMetallicTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Roughness{nameof(CreateTextureNode.Output.texturesByPath)}",
                createRoughnessTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"EmissiveColor{nameof(CreateTextureNode.Output.texturesByPath)}",
                createEmissiveColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Opacity{nameof(CreateTextureNode.Output.texturesByPath)}",
                createOpacityTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Displacement{nameof(CreateTextureNode.Output.texturesByPath)}",
                createDisplacementTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"SpecularColor{nameof(CreateTextureNode.Output.texturesByPath)}",
                createSpecularColorTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Occlusion{nameof(CreateTextureNode.Output.texturesByPath)}",
                createOcclusionTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"Clearcoat{nameof(CreateTextureNode.Output.texturesByPath)}",
                createClearcoatTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge($"ClearcoatRoughness{nameof(CreateTextureNode.Output.texturesByPath)}",
                createClearcoatRoughnessTextureNode_texture, nameof(CreateTextureNode.Output.texturesByPath)));
            graph.AddResultEdge(new ResultEdge(nameof(MapTextureToMaterialNode.Output.materials),
                materialsAggregator_postMesh, nameof(IDictionaryAggregatorNode<Dictionary<string, Material>>.Output.output)));
            graph.AddResultEdge(new ResultEdge("unityMeshes", writeMeshNode_mesh,
                nameof(WriteMeshNode.Output.pathToUnityMesh)));
            graph.AddResultEdge(new ResultEdge("unitySkinnedMeshes", writeMeshNode_skinnedMesh,
                nameof(WriteMeshNode.Output.pathToUnityMesh)));
            graph.AddResultEdge(new ResultEdge(nameof(CreateAnimationClipNode.Output.clip),
                createAnimationClipNode_xformAnimation, nameof(CreateAnimationClipNode.Output.clip)));

            return graph;
        }

        private static void ConnectTextureSettingGenerationInputs(ImporterGraph graph,
            CreateTextureGenerationSettingsNode textureGenerationSettingsNode, TextureImporterType textureType, bool isSRGB)
        {
            graph.AddImportConstant(new ImportConstant<TextureImporterType>(textureType,
                textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.textureType)));
            graph.AddImportConstant(new ImportConstant<bool>(isSRGB, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.sRGB)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesAlphaSource, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.alphaSource)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesAlphaIsTransparency, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.alphaIsTransparency)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesNPOTScale, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.npotScale)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesAreReadable, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.readable)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesMipMap, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.mipMaps)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesWrapMode, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.wrapMode)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesFilterMode, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.filterMode)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesAnisoLevel, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.anisoLevel)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesMaxSize, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.maxSize)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesResizeAlgorithm, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.resizeAlgorithm)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesFormat, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.format)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesCompression, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.compression)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesCrunchCompression, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.crunchedCompression)));
            graph.AddImportConstant(new ImportConstant<bool>(false, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.allowsAlphaSplitting)));
            graph.AddImportConstant(new ImportConstant<AndroidETC2FallbackOverride>(AndroidETC2FallbackOverride.Quality32Bit,
                textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.androidETC2FallbackOverride)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesMipBias, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.mipmapBias)));
            graph.AddSettingEdge(new SettingEdge(UsdImporterImportSettings.TexturesPNGIgnoreGamma, textureGenerationSettingsNode,
                nameof(CreateTextureGenerationSettingsNode.Input.ignorePNGGamma)));
        }
    }
}
