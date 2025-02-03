using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestApp.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TestApp.Managers
{
    public class AssetsManager : Singleton<AssetsManager>
    {
        private Dictionary<string, object> _nowLoading = new Dictionary<string, object>();
        private Dictionary<string, object> _cache = new Dictionary<string, object>();

        private const string TAG = "[AssetsManager]";

        protected AssetsManager() { }

        public async UniTask<T> CreateAsync<T>(string path, Transform parent = null, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"[AssetsManager] Asset path is null or empty.");
                return null;
            }

            bool parentIsNotNull = parent != null;

            T prefab = await LoadAsync<T>(path);

            cancellationToken.ThrowIfCancellationRequested();

            if (prefab && (parentIsNotNull && parent || !parentIsNotNull))
                return GameObject.Instantiate(prefab, parent);

            return null;
        }

        public bool IsCached(string path)
        {
            return _cache.ContainsKey(path);
        }

        public async UniTask<T> LoadAsync<T>(string path)
            where T : UnityEngine.Object
        {
            //Если уже грузится этот ресурс, то возвращаем его
            if (_nowLoading.ContainsKey(path) && _nowLoading[path] is UniTask<T> cashedTask)
                return await cashedTask;

            //Пробуем взять из кэша
            if (_cache.ContainsKey(path))
                return _cache[path] as T;

            UniTaskCompletionSource<T> resultSouce = new UniTaskCompletionSource<T>();

            _nowLoading[path] = resultSouce;

            bool tryLoadAddressables = Addressables.ResourceLocators
                .Any(x => x.Locate(path, typeof(T), out var _));

            try
            {
                T asset = null;

                if (tryLoadAddressables)
                    asset = await LoadFromAddressables<T>(path);

                if (!asset)
                    asset = await LoadFromResources<T>(path);

                if (asset != null)
                    _cache[path] = asset;

                resultSouce.TrySetResult(asset);
            }
            finally
            {
                _nowLoading.Remove(path);
            }

            return await resultSouce.Task;
        }

        private async UniTask<T> LoadFromResources<T>(string path)
            where T : UnityEngine.Object
            => (await Resources.LoadAsync<T>(path)) as T;

        private async UniTask<T> LoadFromAddressables<T>(string path, int retryCount = 1)
            where T : UnityEngine.Object
        {
            T result = await Addressables.LoadAssetAsync<T>(path).Task;

            if (result)
                return result;

            if (retryCount > 0)
            {
                Debug.LogWarning($"{TAG} Retrying to get {path} from addressables, try left {retryCount}");
                return await LoadFromAddressables<T>(path, retryCount - 1);
            }

            Debug.LogError($"{TAG} Error while loading Addressable {path}");
            return result;
        }

        public async UniTask InitializeAddressables()
        {
            await Addressables.InitializeAsync();
        }

        public async UniTask LoadAddressables()
        {
            var catalogsToUpdate = await Addressables.CheckForCatalogUpdates(true).Task;

            if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
                await Addressables.UpdateCatalogs(catalogsToUpdate, autoReleaseHandle: true).Task;
        }

        public async UniTask LoadAddressableCatalog(string catalogName)
        {
            var absoluteCatalogUrl = $"{Addressables.RuntimePath}/{catalogName}";

            var loadHandler = Addressables.LoadContentCatalogAsync(absoluteCatalogUrl, true);

            await loadHandler.Task;

            switch (loadHandler.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.Log($"{TAG} Catalog loaded: {absoluteCatalogUrl}");
                    break;

                case AsyncOperationStatus.Failed:
                    Debug.LogError($"{TAG} Error catalog loading: {absoluteCatalogUrl}");
                    break;
            }
        }

        private Dictionary<string, object> _nowLoadingURL = new Dictionary<string, object>();
        private Dictionary<string, object> _cacheURL = new Dictionary<string, object>();

        public async UniTask<Texture2D> LoadURLImage(string url)
        {
            //Если уже грузится этот ресурс, то возвращаем его
            if (_nowLoadingURL.ContainsKey(url) && _nowLoadingURL[url] is UniTask<Texture2D> cashedTask)
                return await cashedTask;

            //Пробуем взять из кэша
            if (_cacheURL.ContainsKey(url))
                return _cacheURL[url] as Texture2D;

            UniTaskCompletionSource<Texture2D> loadingTask = new UniTaskCompletionSource<Texture2D>();
            _nowLoadingURL[url] = loadingTask;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                loadingTask.TrySetResult(null);
                _nowLoadingURL.Remove(url);

                return null;
            }

            var content = DownloadHandlerTexture.GetContent(request);

            if (content != null)
                _cacheURL[url] = content;

            loadingTask.TrySetResult(content);
            _nowLoadingURL.Remove(url);

            return content;
        }
    }
}
