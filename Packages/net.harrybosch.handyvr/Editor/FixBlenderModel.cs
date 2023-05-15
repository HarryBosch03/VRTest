using UnityEditor;

namespace HandyVREditor
{
    public sealed class FixBlenderModel : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            var importer = assetImporter as ModelImporter;
            importer.bakeAxisConversion = true;
        }
    }
}
