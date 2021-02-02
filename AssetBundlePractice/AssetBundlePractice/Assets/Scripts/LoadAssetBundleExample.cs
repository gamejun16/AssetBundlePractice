using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LoadAssetBundleExample : MonoBehaviour
{
    public string bundleURL;
    public int version;

    private void Start()
    {
        StartCoroutine(LoadAssetBundle());
    }

    IEnumerator LoadAssetBundle()
    {
        while (!Caching.ready)
            yield return null;

        bundleURL = Application.streamingAssetsPath + "/AssetBundles";

        #region assetbundle 로드
        var myobjects = UnityWebRequestAssetBundle.GetAssetBundle("file://" + bundleURL + "/jmj/myobject"); // file:// 붙여야됨
        yield return myobjects.SendWebRequest();
        AssetBundle myobjectBundle = DownloadHandlerAssetBundle.GetContent(myobjects);
        #endregion

        #region manifest 
        // load
        AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleURL + "/AssetBundles"); // file:// 붙이면 안됨
        AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        // dependency check
        string[] dependencies = manifest.GetAllDependencies("jmj/myobject");
        foreach (string s in dependencies)
        {
            AssetBundle.LoadFromFile(Path.Combine(bundleURL, s));
        }
        #endregion

        GameObject cube = myobjectBundle.LoadAsset<GameObject>("Cube");
        Instantiate(cube);

        GameObject capsule = Instantiate(myobjectBundle.LoadAsset<GameObject>("capsule"));
        capsule.transform.Translate(Vector3.up * 2);
        
        print($"done");
    }
}
