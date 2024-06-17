﻿using SkiaSharp;

namespace PlantCare.App.Utils;

public static class ImageResizer
{
    public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var original = SKBitmap.Decode(inputStream);
        using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
        using var image = SKImage.FromBitmap(resized);
        using var outputStream = new MemoryStream();

        image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(outputStream);
        return outputStream.ToArray();
    }
}

public static class ImageHelper
{
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
        image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(outputStream);
        return outputStream.ToArray();
    }
}