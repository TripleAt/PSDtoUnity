using UnityEngine;
using UnityEditor.Experimental;
using UnityEditor.Experimental.AssetImporters;

namespace UnityEditor.U2D.PSD
{

    [ScriptedImporter(1, "psd", AutoSelect = false)]
    internal class PSDImporterOverride : PSDImporter
    {
        [MenuItem("Assets/2D Importer/Change PSD File Importer", false, 30)]
        static void ChangeImporter()
        {
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var ext = System.IO.Path.GetExtension(path);
                if (ext == ".psd")
                {
                    var importer = AssetImporter.GetAtPath(path);
                    if (importer is PSDImporterOverride)
                    {
                        Debug.Log(string.Format("{0} is now imported with TextureImporter", path));
                        AssetDatabaseExperimental.ClearImporterOverride(path);
                    }
                    else
                    {
                        Debug.Log(string.Format("{0} is now imported with PSDImporter", path));
                        AssetDatabaseExperimental.SetImporterOverride<PSDImporterOverride>(path);
                    }
                }
            }
        }
    }

}
