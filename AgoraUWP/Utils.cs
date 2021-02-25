using AgoraWinRT;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;

namespace AgoraUWP
{
    public static class Utils
    {
        public static unsafe SoftwareBitmap ConvertToImage(VideoMediaFrame input)
        {
            if (input != null)
            {
                var inputBitmap = input.SoftwareBitmap;
                var surface = input.Direct3DSurface;
                try
                {
                    if (surface != null) inputBitmap = SoftwareBitmap.CreateCopyFromSurfaceAsync(surface, BitmapAlphaMode.Ignore).AsTask().GetAwaiter().GetResult();
                    if (inputBitmap != null) return SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
                }
                finally
                {
                    inputBitmap?.Dispose();
                    surface?.Dispose();
                }
            }

            return null;
        }

        public static byte[] ConvertToNv12(VideoFrame frame)
        {
            var size = frame.width * frame.height;
            var totalLength = size * 3 / 2;
            var result = new byte[totalLength];
            var ybuffer = frame.yBuffer;
            Array.Copy(ybuffer, result, ybuffer.Length);
            byte[] ubuffer = frame.uBuffer, vbuffer = frame.vBuffer;
            for (int i = (int)size, j = 0; i < totalLength; j++)
            {
                result[i++] = ubuffer[j];
                result[i++] = vbuffer[j];
            }
            return result;
        }

        public static unsafe SoftwareBitmap ConvertToImage(byte[] input, int width, int height)
        {
            using (var yuv = SoftwareBitmap.CreateCopyFromBuffer(input.AsBuffer(), BitmapPixelFormat.Nv12, width, height))
            {
                return SoftwareBitmap.Convert(yuv, BitmapPixelFormat.Bgra8);
            }

        }

        public static SoftwareBitmap Resize(SoftwareBitmap softwareBitmap, float newWidth, float newHeight)
        {
            using (var resourceCreator = CanvasDevice.GetSharedDevice())
            using (var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(resourceCreator, softwareBitmap))
            using (var canvasRenderTarget = new CanvasRenderTarget(resourceCreator, newWidth, newHeight, canvasBitmap.Dpi))
            using (var drawingSession = canvasRenderTarget.CreateDrawingSession())
            using (var scaleEffect = new ScaleEffect())
            {
                scaleEffect.Source = canvasBitmap;
                scaleEffect.Scale = new System.Numerics.Vector2(newWidth / softwareBitmap.PixelWidth, newHeight / softwareBitmap.PixelHeight);
                drawingSession.DrawImage(scaleEffect);
                drawingSession.Flush();
                return SoftwareBitmap.CreateCopyFromBuffer(canvasRenderTarget.GetPixelBytes().AsBuffer(), BitmapPixelFormat.Bgra8, (int)newWidth, (int)newHeight, BitmapAlphaMode.Premultiplied);
            }
        }

    }

}
