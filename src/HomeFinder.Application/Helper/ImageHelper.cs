using System;
using System.Collections.Generic;
using System.Text;

namespace HomeFinder.Application.Helper
{
    internal static class ImageHelper
    {
        public static async Task<(Stream, int, int)> ResizeImageAsync(Stream imageStream, Services.IImageProcessor imageProcessor, int width, int height, int maxResolution, CancellationToken cancellationToken)
        {
            imageStream.Position = 0;
            Stream uploadStream;
            int finalWidth = width, finalHeight = height;
            if (width > maxResolution || height > maxResolution)
            {
                uploadStream = await imageProcessor.ResizeAsync(imageStream, maxResolution, maxResolution, cancellationToken);
                (finalWidth, finalHeight) = await imageProcessor.GetDimensionsAsync(uploadStream, cancellationToken);
                uploadStream.Position = 0;
            }
            else
            {
                uploadStream = imageStream;
            }
            return (uploadStream, finalWidth, finalHeight);
        }
        
    }
}
