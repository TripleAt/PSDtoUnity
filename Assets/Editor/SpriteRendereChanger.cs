using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.U2D.PSD;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[ScriptedImporter(1, "psd", AutoSelect = false)]
internal class SpriteRendereChanger
{

    /// <summary>
    /// SpriteRendererをUGUIのImageに変換
    /// </summary>
    [MenuItem("GameObject/SpriteRendereToUGUI", false, 20)]
    public static void SpriteRendereToUGUI()
    {
        var canvasRect = Selection.activeGameObject.GetComponent<RectTransform>();
        var gameObject = Selection.activeGameObject;
        if (gameObject != null)
        {
            var createObject = CopyObject(gameObject);
            createObject.transform.SetParent(canvasRect, false);
            GetAllChildlen(createObject, SetNativeSize);
        }
    }
    
    
    /// <summary>
    /// 選択中のオブジェクトのImageを一括でサイズ初期化を行う。
    /// </summary>
    [MenuItem("GameObject/AllSetNativeSize", false, 30)]
    private static void SetAllNativeSize()
    {
        var gameObject = Selection.activeGameObject;
        GetAllChildlen(gameObject, SetNativeSize);
    }

    /// <summary>
    /// オブジェクトにImageがついている場合サイズを初期化する
    /// </summary>
    /// <param name="obj"></param>
    private static void SetNativeSize(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        var image = obj.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.SetNativeSize();
        }
    }

    /// <summary>
    /// 子オブジェクトを検索して見つかったらコールバックで返す。
    /// </summary>
    /// <param name="parentObj"></param>
    /// <param name="callback"></param>
    private static void GetAllChildlen(GameObject parentObj, UnityAction<GameObject> callback)
    {
        var image = parentObj.GetComponent<UnityEngine.UI.Image>();

        foreach (Transform child in parentObj.GetComponentInChildren<Transform>())
        {
            GetAllChildlen(child.gameObject, callback);
        }
        callback(parentObj);
    }

    private static GameObject CopyObject(GameObject targetObj)
    {

        GameObject createObject = CreateCopyGameObject(targetObj);
        foreach (Transform child in targetObj.GetComponentInChildren<Transform>())
        {
            var childObj = CopyObject(child.gameObject);
            if (childObj != null)
            {
                childObj.transform.SetParent(createObject.transform, false);
            }
        }
        return createObject;
    }

    /// <summary>
    /// 元オブジェクトをコピーしてrecttransformにして生成
    /// </summary>
    /// <param name="targetObjct"></param>
    /// <returns></returns>
    private static GameObject CreateCopyGameObject(GameObject targetObjct)
    {
        if (targetObjct.activeInHierarchy == false)
        {
            return null;
        }
        GameObject copyObject = new GameObject(targetObjct.name);
        RectTransform copyTransform = copyObject.AddComponent<RectTransform>();
        Transform targetTransform = targetObjct.transform;
        SpriteRenderer targetSpriteRenderer = targetObjct.GetComponent<SpriteRenderer>();
        var targetPos = targetTransform.localPosition;

        copyTransform.localScale = Vector2.one;
        copyTransform.localPosition = new Vector2(targetPos.x*100, targetPos.y*100);    //なんで100なのかは分からん。座標系はUnityは1mが1だから?それともAffinity Designerのみ?
        copyTransform.localRotation = targetTransform.localRotation;
        AddImageSprite(targetSpriteRenderer, copyObject);
        return copyObject;
    }

    /// <summary>
    /// SpriteRendererをImageに変換
    /// </summary>
    /// <param name="target"></param>
    /// <param name="copy"></param>
    private static void AddImageSprite(SpriteRenderer target, GameObject copy) {
        if (target == null) return;
        var image = copy.AddComponent<UnityEngine.UI.Image>();
            
        var saveBytes = CreateReadabeTexture2D(target.sprite.texture,target.sprite.rect).EncodeToPNG();
            
        Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeGameObject);
        var path = AssetDatabase.GetAssetPath(parentObject);
        path = Path.GetDirectoryName(path);
        path += "/" + target.sprite.name+".png";
        File.WriteAllBytes(path, saveBytes);

        AssetDatabase.ImportAsset(path);
            
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        var settings = new TextureImporterSettings();
        if (!(importer is null)) {
            importer.ReadTextureSettings(settings);
                
            settings.textureType = TextureImporterType.Sprite;
            settings.npotScale = TextureImporterNPOTScale.None;
            settings.spriteMode = (int) SpriteImportMode.Single;
                
            importer.SetTextureSettings(settings);
                
            importer.SaveAndReimport();
        }

        Sprite s = (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
        image.sprite = s;
    }

    /// <summary>
    /// Texture2Dから切り取って画像を生成
    /// </summary>
    /// <param name="texture2d"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    private static Texture2D CreateReadabeTexture2D(Texture2D texture2d,Rect rect)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            texture2d.width,
            texture2d.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);
        

        Graphics.Blit(texture2d, renderTexture);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D readableTextur2D = new Texture2D((int) rect.width, (int) rect.height);
        readableTextur2D.ReadPixels(new Rect((int) rect.xMin, texture2d.height-rect.yMax, rect.width, rect.height),  0, 0);
        readableTextur2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTextur2D;
    }
}


