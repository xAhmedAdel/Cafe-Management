using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CafeManagement.Client.Services.Interfaces;

namespace CafeManagement.Client.Services;

public class ScreenCaptureService : IScreenCaptureService
{
    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        public const int SRCCOPY = 0x00CC0020;
    }

    public async Task<byte[]> CaptureScreenAsync()
    {
        return await Task.Run(() =>
        {
            var bounds = SystemParameters.PrimaryScreenWidth;
            var height = SystemParameters.PrimaryScreenHeight;
            return CaptureScreenNative(0, 0, (int)bounds, (int)height);
        });
    }

    public async Task<byte[]> CaptureScreenAsync(int x, int y, int width, int height)
    {
        return await Task.Run(() => CaptureScreenNative(x, y, width, height));
    }

    public async Task<byte[]> CaptureWindowAsync(IntPtr windowHandle)
    {
        return await Task.Run(() =>
        {
            if (windowHandle == IntPtr.Zero)
            {
                return new byte[0];
            }

            // Get window dimensions
            if (!GetWindowRect(windowHandle, out var rect))
            {
                return new byte[0];
            }

            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;

            return CaptureScreenNative(rect.Left, rect.Top, width, height);
        });
    }

    private byte[] CaptureScreenNative(int x, int y, int width, int height)
    {
        try
        {
            using (var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32))
            {
                // Create a visual representation of the screen area
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    // Capture the specified screen area
                    var brush = new VisualBrush(null)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top,
                        Viewbox = new Rect(x, y, width, height),
                        ViewboxUnits = BrushMappingMode.Absolute
                    };

                    context.DrawRectangle(brush, null, new Rect(0, 0, width, height));
                }

                bitmap.Render(visual);

                // Convert to byte array
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    return stream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            // Fallback to basic GDI capture
            return CaptureScreenGDI(x, y, width, height);
        }
    }

    private byte[] CaptureScreenGDI(int x, int y, int width, int height)
    {
        try
        {
            IntPtr hdcSrc = NativeMethods.GetDesktopWindow();
            IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = NativeMethods.SelectObject(hdcDest, hBitmap);

            NativeMethods.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, x, y, NativeMethods.SRCCOPY);

            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);

            var bitmap = System.Drawing.Image.FromHbitmap(hBitmap);
            NativeMethods.DeleteObject(hBitmap);

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        catch
        {
            return new byte[0];
        }
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public byte[] CompressImage(byte[] imageData, int quality = 75)
    {
        try
        {
            using (var inputMemoryStream = new MemoryStream(imageData))
            using (var outputMemoryStream = new MemoryStream())
            using (var image = SixLabors.ImageSharp.Image.Load(inputMemoryStream))
            {
                var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                {
                    Quality = quality
                };

                image.SaveAsJpeg(outputMemoryStream, encoder);
                return outputMemoryStream.ToArray();
            }
        }
        catch
        {
            return imageData; // Return original if compression fails
        }
    }

    public byte[] CompressWithDeflate(byte[] data)
    {
        using (var outputMemoryStream = new MemoryStream())
        using (var deflateStream = new DeflateStream(outputMemoryStream, CompressionMode.Compress))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Close();
            return outputMemoryStream.ToArray();
        }
    }
}