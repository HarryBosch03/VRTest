using UnityEditor;

namespace HandyVREditor.Editor
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
