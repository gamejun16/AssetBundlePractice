using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class AssetBundleManager : MonoBehaviour
{
    public string bundleURL;
    public int version;

    string albumURL;
    string photoURL;

    private AssetBundle photoAssetBundle;
    public Object[] PhotoAssetBundle { get { return photoAssetBundle.LoadAllAssets(); } }

    private AssetBundle albumAssetBundle;
    public Object[] AlbumAssetBundle { get { return albumAssetBundle.LoadAllAssets(); } }

    private void Start()
    {
        StartCoroutine(LoadAssetBundle());
    }

    IEnumerator LoadAssetBundle()
    {
        while (!Caching.ready)
            yield return null;

        //bundleURL = Application.streamingAssetsPath + "/AssetBundles";
        bundleURL = "https://www.dropbox.com/s/2r8w68kign84qof/AssetBundles?dl=1";
        albumURL = "https://www.dropbox.com/s/845gjrr5a4morp0/albums?dl=1";
        photoURL = "https://www.dropbox.com/s/5322kz4ysosmais/photos?dl=1";

        #region assetbundle 로드
        //var v = UnityWebRequestAssetBundle.GetAssetBundle("file://" + bundleURL + "/AssetBundles"); // file:// 붙여야됨
        var v = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL); 
        yield return v.SendWebRequest();
        AssetBundle myAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
        #endregion

        #region test
        #region manifest 
        // load
        //AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleURL + "/AssetBundles"); // file:// 붙이면 안됨
        //AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        // dependency check
        //string[] dependencies = manifest.GetAllDependencies("jmj/myobject");
        //foreach (string s in dependencies)
        //{
        //    AssetBundle.LoadFromFile(Path.Combine(bundleURL, s));
        //}
        #endregion

        //GameObject cube = myobjectBundle.LoadAsset<GameObject>("Cube");
        //Instantiate(cube);
        //GameObject capsule = Instantiate(myobjectBundle.LoadAsset<GameObject>("capsule"));
        //capsule.transform.Translate(Vector3.up * 2);
        #endregion

        AssetBundleManifest manifest = myAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //foreach(string s in manifest.GetAllAssetBundles())
        //{
        //print($"s : {s}");
        //AssetBundle sub = AssetBundle.LoadFromFile(Path.Combine(bundleURL, s));
        //sub.LoadAllAssets();
        //}

        //albumAssetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleURL, "jmj/albums"));
        //photoAssetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleURL, "jmj/photos"));

        v = UnityWebRequestAssetBundle.GetAssetBundle(albumURL);
        yield return v.SendWebRequest();
        albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

        v = UnityWebRequestAssetBundle.GetAssetBundle(photoURL);
        yield return v.SendWebRequest();
        photoAssetBundle= DownloadHandlerAssetBundle.GetContent(v);

        print($"done");

        DataContainer.instance.Init(AlbumAssetBundle, PhotoAssetBundle);
    }
}
