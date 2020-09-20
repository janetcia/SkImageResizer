using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkImageResizer
{
    public class SKImageProcess
    {
        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                var bitmap = SKBitmap.Decode(filePath);
                var imgPhoto = SKImage.FromBitmap(bitmap);
                var imgName = Path.GetFileNameWithoutExtension(filePath);

                var sourceWidth = imgPhoto.Width;
                var sourceHeight = imgPhoto.Height;

                var destinationWidth = (int)(sourceWidth * scale);
                var destinationHeight = (int)(sourceHeight * scale);

                using var scaledBitmap = bitmap.Resize(
                    new SKImageInfo(destinationWidth, destinationHeight),
                    SKFilterQuality.High);
                using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                data.SaveTo(s);
            }
        }

        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            await Task.Yield();

            var allFiles = FindImages(sourcePath);
            Parallel.ForEach(allFiles, async (filePath) =>
            {
                var bitmap = SKBitmap.Decode(filePath);
                var imgPhoto = SKImage.FromBitmap(bitmap);

                var sourceWidth = imgPhoto.Width;
                var sourceHeight = imgPhoto.Height;

                var destinationWidth = (int) (sourceWidth * scale);
                var destinationHeight = (int) (sourceHeight * scale);
                using var scaledBitmap = bitmap.Resize(
                    new SKImageInfo(destinationWidth, destinationHeight),
                    SKFilterQuality.High);
                using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                var imgName = Path.GetFileNameWithoutExtension(filePath);
                using var stream = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                data.SaveTo(stream);
            });
        }

        public async Task ResizeImagesAsync2(string sourcePath, string destPath, double scale)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            await Task.Yield();
            
            var tasks = new List<Task>();
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                var task = Task.Run(() =>
                {
                    var bitmap = SKBitmap.Decode(filePath);
                    var imgPhoto = SKImage.FromBitmap(bitmap);
                    var imgName = Path.GetFileNameWithoutExtension(filePath);

                    var sourceWidth = imgPhoto.Width;
                    var sourceHeight = imgPhoto.Height;

                    var destinationWidth = (int) (sourceWidth * scale);
                    var destinationHeight = (int) (sourceHeight * scale);

                    using var scaledBitmap = bitmap.Resize(
                        new SKImageInfo(destinationWidth, destinationHeight),
                        SKFilterQuality.High);
                    using var scaledImage = SKImage.FromBitmap(scaledBitmap);
                    using var data = scaledImage.Encode(SKEncodedImageFormat.Jpeg, 100);
                    using var s = File.OpenWrite(Path.Combine(destPath, imgName + ".jpg"));
                    data.SaveTo(s);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> extensionPatterns = new List<string> { " *.png", "*.jpg", "*.jpeg" };
            List<string> files = new List<string>();
            Parallel.ForEach(extensionPatterns, (extensionPattern) =>
            {
                var extensionfiles = Directory.GetFiles(srcPath, extensionPattern, SearchOption.AllDirectories);
                files.AddRange(extensionfiles);
            });
            return files;
        }
    }
}