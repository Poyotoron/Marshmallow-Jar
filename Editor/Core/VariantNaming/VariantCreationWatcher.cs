using UnityEditor;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class VariantCreationWatcher : AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string path)
        {
            VariantNameProcessor.NotifyCreating(path);
        }
    }
}
