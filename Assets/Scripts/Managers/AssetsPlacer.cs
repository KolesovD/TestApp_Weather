using Cysharp.Threading.Tasks;
using System.Threading;
using TestApp.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TestApp.Managers
{
    public static class AssetsPlacer
    {
        public static async UniTask<Sprite> LoadFromAssetsAsync(this Image img, string path, bool preserveAspect = true,
            bool hideIfNull = true, CancellationToken cancellationToken = default)
        {
            if (!img)
                return null;

            img.SetColor(a: 0f);

            Sprite sprite = await AssetsManager.Instance.CreateAsync<Sprite>(path, cancellationToken: cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (!img)
                return null;

            if (!sprite)
            {
                Debug.LogWarning($"No image for [{path}]");
                if (!hideIfNull)
                    img.SetColor(a: 1f);

                return null;
            }

            img.preserveAspect = preserveAspect;
            img.sprite = sprite;
            img.SetColor(a: 1f);

            return sprite;
        }

        public static async UniTask<Sprite> LoadFromAssetsAsync(this SpriteRenderer renderer, string path,
            bool hideIfNull = true, CancellationToken cancellationToken = default)
        {
            if (!renderer)
                return null;

            renderer.SetColor(a: 0f);

            Sprite sprite = await AssetsManager.Instance.CreateAsync<Sprite>(path, cancellationToken: cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (!renderer)
                return null;

            if (!sprite)
            {
                Debug.LogWarning($"No image for [{path}]");
                if (!hideIfNull)
                    renderer.SetColor(a: 1f);

                return null;
            }

            renderer.sprite = sprite;
            renderer.SetColor(a: 1f);

            return sprite;
        }

        public static async UniTask<Sprite> LoadImageFromURL(this Image img, string url, bool preserveAspect = true, bool hideIfNull = true,
            CancellationToken cancellationToken = default)
        {
            if (!img)
                return null;

            img.SetColor(a: 0);

            Texture2D texture = await AssetsManager.Instance.LoadURLImage(url);

            if (!img)
                return null;

            cancellationToken.ThrowIfCancellationRequested();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            if (!sprite)
            {
                Debug.LogWarning($"Error while creating sprite for texture from [{url}]");
                if (!hideIfNull)
                    img.SetColor(a: 1f);

                return null;
            }

            img.preserveAspect = preserveAspect;
            img.sprite = sprite;
            img.SetColor(a: 1f);

            return sprite;
        }
    }
}
