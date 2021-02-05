using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using System.Text;

public class AssetBundleManager : MonoBehaviour
{
    enum VersionTableColumn // 버전 테이블 Column(열) 정보
    {
        fileName, // 번들 파일 명
        version, // 번들 버전 정보
        downloadLink // 번들 설치 링크
    }
    
    private string serverVersionTableURL; // 서버 버전 테이블 접속 URL
    private string localVersionTablePath; // 로컬 버전 테이블 경로
    
    private string[,] localVersionTable; // 로컬 버전 테이블
    private List<string[]> patchListInfo = new List<string[]>(); // 패치가 필요한 데이터 정보

    private string[,] serverVersionTable; // 서버 버전 테이블
    
    private AssetBundle rootAssetBundle; // 에셋 번들
    private AssetBundle albumAssetBundle; //
    private AssetBundle photoAssetBundle; //
    private AssetBundle videoAssetBundle; //

    public Object[] RootAssetBundle { get { return rootAssetBundle.LoadAllAssets(); } }
    public Object[] AlbumAssetBundle { get { return albumAssetBundle.LoadAllAssets(); } }
    public Object[] PhotoAssetBundle { get { return photoAssetBundle.LoadAllAssets(); } }
    public Object[] VideoAssetBundle{ get { return videoAssetBundle.LoadAllAssets(); } }


    [Header("Patch Progress UI")]
    [SerializeField] private Transform patchScreen;
    [SerializeField] private Text loadingText;
    [SerializeField] private Text loadingSubText;

    private void Start()
    {
        serverVersionTableURL = "https://drive.google.com/uc?export=download&id=12Y_tx_5MrKd6yLVYyB8aQGM746pXlws4"; // 
        localVersionTablePath = Application.streamingAssetsPath + "/AssetBundles/versionTable.csv";

        StartCoroutine(LoadAssetBundleProgress()); // 패치 시작
    }

    /// <summary>
    /// 에셋 번들 패치 프로세스
    /// </summary>
    IEnumerator LoadAssetBundleProgress()
    {
        patchScreen.gameObject.SetActive(true);
        float _totalTime = Time.time;

        #region old version
        //loadingText.text = "패치 내역을 확인하는 중";
        //yield return CheckBundleVersion();

        //loadingText.text = "패치 진행 중";
        //yield return DownloadBundleFromServerToLocal();

        //loadingText.text = "곧 게임이 시작됩니다.";
        //yield return LoadAssetBundleFromLocal();
        #endregion
        
        #region new version
        loadingText.text = "버전 정보를 받아오는 중";
        yield return CheckBundleVersion();

        loadingText.text = "패치를 진행하는 중";
        yield return SerialLoadAssetBundle();
        //yield return ParallerLoadAssetBundle();
        #endregion

        print($"total time : {Time.time - _totalTime}");

        DataContainer.instance.Init(AlbumAssetBundle, PhotoAssetBundle, VideoAssetBundle);
        patchScreen.gameObject.SetActive(false);

        print($"done");
    }

    /// <summary>
    /// 서버 버전 테이블 저장
    /// </summary>
    IEnumerator CheckBundleVersion()
    {
        loadingSubText.text = "인터넷에 접속합니다.";

        // # 서버 버전 테이블 load
        using (UnityWebRequest v = UnityWebRequest.Get(serverVersionTableURL))
        {
            loadingSubText.text = "서버 버전 테이블을 조회합니다.";

            yield return v.SendWebRequest();

            string[] rows = v.downloadHandler.text.Split('\n');
            serverVersionTable = new string[rows.Length - 1, rows[0].Split(',').Length];

            for (int r = 0; r < serverVersionTable.GetLength(0); r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < serverVersionTable.GetLength(1); c++)
                {
                    serverVersionTable[r, c] = cols[c];
                }
            }

            loadingSubText.text = "로컬 버전 테이블을 갱신합니다.";

            // 패치 정보에 변화가 있다면 서버 버전 테이블을 내려받아 로컬 버전테이블에 덮어쓰기
            byte[] data = v.downloadHandler.data;
            FileStream fs = new FileStream(localVersionTablePath, FileMode.Create);
            fs.Write(data, 0, data.Length);
            fs.Dispose();   
        }
    }

    /// <summary>
    /// 서버 버전을 기반으로 캐시 혹은 웹서버에서 순차적으로 에셋 번들 로드
    /// </summary>
    IEnumerator SerialLoadAssetBundle()
    {
        for (int row = 1; row < serverVersionTable.GetLength(0); row++)
        {
            float beginTime = Time.time;

            loadingSubText.text = $"패치를 진행하고 있습니다. ({row}/{serverVersionTable.GetLength(0) - 1})";

            string fileName = serverVersionTable[row, (int)VersionTableColumn.fileName];
            string version = serverVersionTable[row, (int)VersionTableColumn.version];
            string downloadURL = serverVersionTable[row, (int)VersionTableColumn.downloadLink];
            
            var v = UnityWebRequestAssetBundle.GetAssetBundle(downloadURL, uint.Parse(version), 0);

            // 통신 진행 정도 확인
            v.SendWebRequest();
            while (!v.isDone) // 통신 끝날 때까지 진행
            {
                StringBuilder comment = new StringBuilder();
                comment.Append($"패치를 진행하고 있습니다. ({row}/{serverVersionTable.GetLength(0) - 1})");
                int progress = (int)(v.downloadProgress * 100);
                if (progress != 0)
                    comment.Append($" {progress}%");
                loadingSubText.text = comment.ToString();
                yield return null;
            }

            if (string.Compare(fileName, "AssetBundles") == 0)
                rootAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
           
            else if (string.Compare(fileName, "jmj/albums") == 0)
                albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
            
            else if (string.Compare(fileName, "jmj/photos") == 0)
                photoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

            else if (string.Compare(fileName, "jmj/videos") == 0)
                videoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
        }

        loadingSubText.text = "곧 게임이 시작됩니다.";
    }

    /// <summary>
    /// 서버 버전을 기반으로 캐시 혹은 웹서버에서 병렬적으로 에셋 번들 로드
    /// </summary>
    IEnumerator ParallerLoadAssetBundle()
    {
        yield return null;

        // 병렬 통신 개수
        int parallerSize = 1;
        
        Queue<System.Tuple<UnityWebRequest, string>> waitQueue = new Queue<System.Tuple<UnityWebRequest, string>>();
        List<System.Tuple<UnityWebRequest, string>> progressList = new List<System.Tuple<UnityWebRequest, string>>();

        // 대기 큐 생성
        for (int row = 1; row < serverVersionTable.GetLength(0); row++)
        {
            float beginTime = Time.time;

            loadingSubText.text = $"패치를 진행하고 있습니다. ({row}/{serverVersionTable.GetLength(0) - 1})";

            string fileName = serverVersionTable[row, (int)VersionTableColumn.fileName];
            string version = serverVersionTable[row, (int)VersionTableColumn.version];
            string downloadURL = serverVersionTable[row, (int)VersionTableColumn.downloadLink];

            var v = UnityWebRequestAssetBundle.GetAssetBundle(downloadURL, uint.Parse(version), 0);
            System.Tuple<UnityWebRequest, string> tp = new System.Tuple<UnityWebRequest, string>(v, fileName);
            waitQueue.Enqueue(tp);
        }

        print($"대기 큐 생성 완료");

        // 진행
        while (waitQueue.Count > 0 || progressList.Count > 0)
        {
            if(progressList.Count < parallerSize && waitQueue.Count > 0)
            {
                var v = waitQueue.Peek();
                waitQueue.Dequeue();

                print($"{v.Item2} load start");

                v.Item1.SendWebRequest();
                progressList.Add(v);
            }

            for(int i=0;i<progressList.Count; i++)
            {
                if (progressList[i].Item1.isDone)
                {
                    UnityWebRequest v = progressList[i].Item1;
                    string fileName = progressList[i].Item2;

                    print($"{progressList[i].Item2} load done");

                    if (string.Compare(fileName, "AssetBundles") == 0)
                        rootAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

                    else if (string.Compare(fileName, "jmj/albums") == 0)
                        albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

                    else if (string.Compare(fileName, "jmj/photos") == 0)
                        photoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

                    else if (string.Compare(fileName, "jmj/videos") == 0)
                        videoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

                    progressList.RemoveAt(i);
                }
            }
            yield return null;
        }


        //if (string.Compare(fileName, "AssetBundles") == 0)
        //    rootAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

        //else if (string.Compare(fileName, "jmj/albums") == 0)
        //    albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);

        //else if (string.Compare(fileName, "jmj/photos") == 0)
        //    photoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);


        loadingSubText.text = "곧 게임이 시작됩니다.";
    }

    /// <summary>
    /// 서버 버전테이블과 로컬 버전테이블을 비교. 패치가 필요한 번들 정보 추출
    /// </summary>
    IEnumerator OldCheckBundleVersion()
    {
        loadingSubText.text = "인터넷에 접속합니다.";

        // # 1. 서버 버전 테이블 load
        using (UnityWebRequest uwr = UnityWebRequest.Get(serverVersionTableURL))
        {
            loadingSubText.text = "서버 버전 테이블을 조회합니다.";

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

            StreamReader sr = new StreamReader(localVersionTablePath);
            
            rows = sr.ReadToEnd().Split('\n');
            localVersionTable = new string[rows.Length-1, rows[0].Split(',').Length];
            for (int r = 0; r < localVersionTable.GetLength(0); r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < localVersionTable.GetLength(1); c++)
                {
                    localVersionTable[r, c] = cols[c];
                }
            }
            sr.Dispose();

            // # 3. 비교 및 패치 진행할 번들 정보 추출
            // # 3-1. 패치 정보 로컬 버전 테이블에 기록
            //   >>>  그냥 서버 버전 테이블로 덮어쓰기?
            loadingSubText.text = "패치 정보를 비교하는 중입니다.";

            for (int r = 1; r < versionTableFromServer.GetLength(0); r++)
            {
                // 버전 비교. 패치가 필요한 번들 발견
                if (string.Compare(versionTableFromServer[r, (int)VersionTableColumn.version], localVersionTable[r, (int)VersionTableColumn.version]) != 0)
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
    }

    /// <summary>
    /// 로컬에 변동된(혹은 없는) 에셋 번들을 서버로부터 다운로드 및 파일 생성
    /// </summary>
    IEnumerator OldDownloadBundleFromServerToLocal()
    {
        print($"start download {patchListInfo.Count} bundles from server to local ");

        loadingSubText.text = "서버로부터 파일을 내려받는 중 입니다.";

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
            
            // 디렉토리 생성
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);
            
            // 파일로 저장
            FileStream fs = new FileStream(Path.Combine(fileDirectory, fileName), FileMode.Create);
            fs.Write(request.downloadHandler.data, 0, (int)request.downloadedBytes);
            fs.Close();

        }
    }
    
    /// <summary>
    /// 로컬에 저장된 에셋 번들을 로드
    /// </summary>
    IEnumerator OldLoadAssetBundleFromLocal()
    {
        print("start load assetBundle from local");

        loadingSubText.text = "로컬 데이터를 읽어들이는 중입니다.";

        for (int row = 1; row < localVersionTable.GetLength(0); row++)
        {
            string originName = localVersionTable[row, (int)VersionTableColumn.fileName];
            string fileDirectory = Application.streamingAssetsPath + "/AssetBundles";
            string fileName;

            string[] split = originName.Split('/');
            for (int j = 0; j < split.Length - 1; j++)
                fileDirectory = fileDirectory + "/" + split[j];
            fileName = split[split.Length - 1];

            string path = fileDirectory + "/" + fileName;

            var v = UnityWebRequestAssetBundle.GetAssetBundle(path + ".unity3d", 0);
            yield return v.SendWebRequest();

            if (string.Compare(fileName, "AssetBundles") == 0)
            {
                //rootAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");    
                rootAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
            }
            else if (string.Compare(fileName, "albums") == 0)
            {
                //albumAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");
                albumAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
            }
            else if (string.Compare(fileName, "photos") == 0)
            {
                //photoAssetBundle = AssetBundle.LoadFromFile(path + ".unity3d");
                photoAssetBundle = DownloadHandlerAssetBundle.GetContent(v);
            }
        }

        loadingSubText.text = "곧 게임이 시작됩니다.";

        yield return null;
        print("loadAssetBundleFromLocal done");
        
    }
    
    /// <summary>
    /// no use
    /// </summary>
    IEnumerator LoadAssetBundle()
    {

        // 패치 정보 확인
        yield return OldCheckBundleVersion();

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

        DataContainer.instance.Init(AlbumAssetBundle, PhotoAssetBundle, VideoAssetBundle);
        
    }

}
