using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class AssetBundleManager : MonoBehaviour
{
    enum VersionTableColumn // 버전 테이블 Column(열) 정보
    {
        fileName, // 번들 파일 명
        version, // 번들 버전 정보
        downloadLink // 번들 설치 링크
    }

    string versionTableURL;
    string localVersionTablePath;

    string bundleURL;
    string albumURL;
    string photoURL;

    [SerializeField] private Transform patchScreen;
    [SerializeField] private Text loadingText;
    [SerializeField] private Text loadingSubText;

    //List<int> patchList = new List<int>(); // 패치 해당되는 row(행) 값 저장
    string[,] versionTableFromLocal;
    List<string[]> patchListInfo = new List<string[]>();

    private AssetBundle rootAssetBundle;
    public Object[] RootAssetBundle { get { return rootAssetBundle.LoadAllAssets(); } }

    private AssetBundle photoAssetBundle;
    public Object[] PhotoAssetBundle { get { return photoAssetBundle.LoadAllAssets(); } }

    private AssetBundle albumAssetBundle;
    public Object[] AlbumAssetBundle { get { return albumAssetBundle.LoadAllAssets(); } }

    private void Start()
    {
        versionTableURL = "https://drive.google.com/uc?export=download&id=12Y_tx_5MrKd6yLVYyB8aQGM746pXlws4";
        localVersionTablePath = Application.streamingAssetsPath + "/AssetBundles/versionTable.csv";

        //bundleURL = "https://drive.google.com/uc?export=download&id=1MTt9qL92YiwLWwxZS_fH67KF_dtYxXTG";
        //bundleURL = Application.streamingAssetsPath + "/AssetBundles";
        //albumURL = "https://drive.google.com/uc?export=download&id=1pW90lypOZG5GFeRwULsEee-dQcAwRLfu";
        //photoURL = "https://drive.google.com/uc?export=download&id=1QZ6y01x-kRa6KG8euQn9OrE1QTHYgNcY";

        //StartCoroutine(LoadAssetBundle());
        StartCoroutine(LoadAssetBundleProgress());
    }

    IEnumerator LoadAssetBundleProgress()
    {
        patchScreen.gameObject.SetActive(true);

        loadingText.text = "패치 내역을 확인하는 중";
        yield return CheckBundleVersion();

        loadingText.text = "패치 진행 중";
        yield return DownloadBundleFromServerToLocal();

        loadingText.text = "곧 게임이 시작됩니다.";
        yield return LoadAssetBundleFromLocal();
        
        DataContainer.instance.Init(AlbumAssetBundle, PhotoAssetBundle);

        patchScreen.gameObject.SetActive(false);

        print($"done");
    }

    /// <summary>
    /// 서버 버전테이블과 로컬 버전테이블을 비교. 패치가 필요한 번들 정보 추출
    /// </summary>
    IEnumerator CheckBundleVersion()
    {
        loadingSubText.text = "인터넷에 접속합니다.";

        // # 1. 서버 버전 테이블 load
        using (UnityWebRequest uwr = UnityWebRequest.Get(versionTableURL))
        {
            loadingSubText.text = "서버 버전 테이블을 조회합니다.";
            yield return new WaitForSeconds(0.5f);

            yield return uwr.SendWebRequest();
            string[,] versionTableFromServer;

            string[] rows = uwr.downloadHandler.text.Split('\n');
            versionTableFromServer = new string[rows.Length-1, rows[0].Split(',').Length];

            for (int r = 0; r < versionTableFromServer.GetLength(0); r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < versionTableFromServer.GetLength(1); c++)
                {
                    versionTableFromServer[r, c] = cols[c];
                }
            }

            // # 2. 로컬 버전 테이블 load
            loadingSubText.text = "로컬 버전 테이블을 조회합니다.";
            yield return new WaitForSeconds(0.5f);

            StreamReader sr = new StreamReader(localVersionTablePath);
            
            rows = sr.ReadToEnd().Split('\n');
            versionTableFromLocal = new string[rows.Length-1, rows[0].Split(',').Length];
            for (int r = 0; r < versionTableFromLocal.GetLength(0); r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < versionTableFromLocal.GetLength(1); c++)
                {
                    versionTableFromLocal[r, c] = cols[c];
                }
            }
            sr.Dispose();

            // # 3. 비교 및 패치 진행할 번들 정보 추출
            // # 3-1. 패치 정보 로컬 버전 테이블에 기록
            //   >>>  그냥 서버 버전 테이블로 덮어쓰기?
            loadingSubText.text = "패치 정보를 비교하는 중입니다.";
            yield return new WaitForSeconds(0.5f);

            for (int r = 1; r < versionTableFromServer.GetLength(0); r++)
            {
                // 버전 비교. 패치가 필요한 번들 발견
                if (string.Compare(versionTableFromServer[r, (int)VersionTableColumn.version], versionTableFromLocal[r, (int)VersionTableColumn.version]) != 0)
                {
                    string[] s = new string[3];
                    s[(int)VersionTableColumn.fileName] = versionTableFromServer[r, (int)VersionTableColumn.fileName];
                    s[(int)VersionTableColumn.version] = versionTableFromServer[r, (int)VersionTableColumn.version];
                    s[(int)VersionTableColumn.downloadLink] = versionTableFromServer[r, (int)VersionTableColumn.downloadLink];

                    patchListInfo.Add(s);
                }
            }
            
            // 패치가 필요하다면. 서버 버전 테이블을 내려받아 로컬 버전테이블에 덮어쓰기
            if (patchListInfo.Count != 0)
            {
                byte[] data = uwr.downloadHandler.data;
                FileStream fs = new FileStream(localVersionTablePath, FileMode.Create);
                fs.Write(data, 0, data.Length);
                fs.Dispose();
            }
        }
        print($"version check done");

    }
    

    /// <summary>
    /// 로컬에 변동된(혹은 없는) 에셋 번들을 서버로부터 다운로드 및 파일 생성
    /// </summary>
    IEnumerator DownloadBundleFromServerToLocal()
    {
        print($"download server2local start {patchListInfo.Count}");

        loadingSubText.text = "서버로부터 파일을 내려받는 중 입니다.";
        yield return new WaitForSeconds(0.5f);

        foreach (string[] s in patchListInfo)
        {
            string originName = s[(int)VersionTableColumn.fileName];
            string fileDirectory = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
            string fileName;
            string version = s[(int)VersionTableColumn.version];
            string downloadURL = s[(int)VersionTableColumn.downloadLink];
            
            string[] split = originName.Split('/');
            for(int i=0;i<split.Length - 1; i++)
                fileDirectory = Path.Combine(fileDirectory, split[i]);
            fileName = split[split.Length - 1] + ".unity3d";

            UnityWebRequest request = UnityWebRequest.Get(downloadURL);
            yield return request.SendWebRequest();
            
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
                print($"{fileDirectory} directory created now");
            }

            // 파일로 저장
            FileStream fs = new FileStream(Path.Combine(fileDirectory, fileName), FileMode.Create);
            fs.Write(request.downloadHandler.data, 0, (int)request.downloadedBytes);
            fs.Close();
        }
        print($"download server to local done");
    }

    /// <summary>
    /// 로컬에 저장된 에셋 번들을 로드?
    /// </summary>
    IEnumerator LoadAssetBundleFromLocal()
    {
        print("loadAssetBundleFromLocal start");

        loadingSubText.text = "로컬 데이터를 읽어들이는 중입니다.";
        yield return new WaitForSeconds(0.5f);

        for (int row=1;row< versionTableFromLocal.GetLength(0); row++)
        {
            string originName = versionTableFromLocal[row,(int)VersionTableColumn.fileName];
            string fileDirectory = Application.streamingAssetsPath + "/AssetBundles";
            string fileName;
            
            string[] split = originName.Split('/');
            for (int j = 0; j < split.Length - 1; j++)
                fileDirectory = fileDirectory + "/" +  split[j];
            fileName = split[split.Length - 1];

            string path = fileDirectory + "/" + fileName;

            if (string.Compare(fileName, "AssetBundles") == 0)
                rootAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");
            
            else if(string.Compare(fileName, "albums") == 0)                
                albumAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");
            
            else if (string.Compare(fileName, "photos") == 0)
                photoAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");
            
        }

        loadingSubText.text = "곧 게임이 시작됩니다.";

        yield return null;
        print("loadAssetBundleFromLocal done");
        
    }



    IEnumerator LoadAssetBundle()
    {

        // 패치 정보 확인
        yield return CheckBundleVersion();

        // 패치할 데이터가 없다면? 종료 ?
        if (patchListInfo.Count == 0)
        {
            print($"패치할 데이터가 없습니다.");
            yield break;
        }

        print($"{patchListInfo.Count}개 파일에 대해 패치를 진행합니다.");
        

        while (!Caching.ready)
            yield return null;


        // 패치 진행
        foreach (string[] s in patchListInfo)
        {
            string bundleFileName = s[(int)VersionTableColumn.fileName];
            string version = s[(int)VersionTableColumn.version];
            string downloadLink = s[(int)VersionTableColumn.downloadLink];

            float startTIme = Time.time;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(downloadLink, uint.Parse(version), 0))
            {
                yield return uwr.SendWebRequest();

                if (!string.IsNullOrEmpty(uwr.error))
                {
                    print($"uer error : {uwr.error}");
                    yield return null;
                    continue;
                }
                
                if(string.Compare(bundleFileName, "AssetBundles") == 0)
                {
                    print($"fileName : {bundleFileName}");
                    rootAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
                else if (string.Compare(bundleFileName, "jmj/albums") == 0)
                {
                    print($"fileName : {bundleFileName}");
                    albumAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
                else if (string.Compare(bundleFileName, "jmj/photos") == 0)
                {
                    print($"fileName : {bundleFileName}");
                    photoAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
                
                float endTime = Time.time - startTIme;
                print($"{bundleFileName} used time : {endTime}");
            }
        }


        #region assetbundle 로드
        //var v = UnityWebRequestAssetBundle.GetAssetBundle("file://" + bundleURL + "/AssetBundles"); // file:// 붙여야됨
        //var v = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL, 1,0);
        //yield return v.SendWebRequest();
        //rootAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
        //print("load rootAssetBundle done");
        //AssetBundleManifest manifest = myAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        #endregion


        #region dependency
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


        //albumAssetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleURL, "jmj/albums"));
        //photoAssetBundle = AssetBundle.LoadFromFile(Path.Combine(bundleURL, "jmj/photos"));


        //v = UnityWebRequestAssetBundle.GetAssetBundle(albumURL, 1, 0);
        //yield return v.SendWebRequest();
        //albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
        //print("load albumAssetBundle done");

        //v = UnityWebRequestAssetBundle.GetAssetBundle(photoURL, 1, 0);
        //yield return v.SendWebRequest();
        //photoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
        //print("load photoAssetBundle done");

        print($"done");

        DataContainer.instance.Init(AlbumAssetBundle, PhotoAssetBundle);
        
    }

}
