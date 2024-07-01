using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;

namespace PlantCare.App.Utils;

using IImage = Microsoft.Maui.Graphics.IImage;

public static class ImageHelper
{
    public const int DefaultPhotoMaxWidthOrHeight = 800;

    public static byte[] ResizeImage(Stream imageStream, int maxWidth, int maxHeight)
    {
        using var inputStream = new SKManagedStream(imageStream);
        using var original = SKBitmap.Decode(inputStream);

        int width = original.Width;
        int height = original.Height;

        float aspectRatio = (float)width / height;

        if (width > maxWidth || height > maxHeight)
        {
            if (aspectRatio > 1)
            {
                // Landscape image
                width = maxWidth;
                height = (int)(maxWidth / aspectRatio);
            }
            else
            {
                // Portrait image
                height = maxHeight;
                width = (int)(maxHeight * aspectRatio);
            }
        }

        using var resizedBitmap = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using var image = SKImage.FromBitmap(resizedBitmap);
        using var outputStream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(outputStream);
        return outputStream.ToArray();
    }

    public static Task<byte[]> ResizeImageAsync(Stream sourceImageStream, int maxWidthOrHeight)
    {
        return Task.Run(() =>
        {
            IImage sourceImage = PlatformImage.FromStream(sourceImageStream/*, ImageFormat.Jpeg ???*/);

            if (sourceImage != null)
            {
                if (sourceImage.Width > maxWidthOrHeight || sourceImage.Height > maxWidthOrHeight)
                {
                    IImage downsizedImage = sourceImage.Downsize(maxWidthOrHeight, disposeOriginal: true);

                    byte[] result = downsizedImage.AsBytes();
                    return result;
                }
            }

            return StreamToByteArray(sourceImageStream);

            //using (var binaryReader = new BinaryReader(sourceImageStream))
            //{
            //    byte[] result = binaryReader.ReadBytes((int)sourceImageStream.Length);
            //    return result;
            //}
        });
    }

    private static byte[] StreamToByteArray(Stream inputStream)
    {
        if (inputStream.CanSeek)
        {
            inputStream.Position = 0; // Ensure the position is at the beginning
        }

        using MemoryStream memoryStream = new();
        inputStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static Task SaveResizedPhotoAsync(string sourcePhotoPath, string resizedPhotoPath)
    {
        if (!File.Exists(sourcePhotoPath))
            return Task.CompletedTask;

        return Task.Run(async () =>
        {
            using Stream sourceStream = File.OpenRead(sourcePhotoPath);
            string fileName = Path.GetFileName(sourcePhotoPath);
            //using Stream sourceStream = await photoFileResult.OpenReadAsync();

            // Resize the image
            byte[] resizedImage = await ResizeImageAsync(sourceStream, DefaultPhotoMaxWidthOrHeight);

            await File.WriteAllBytesAsync(resizedPhotoPath, resizedImage);
        });
    }
}