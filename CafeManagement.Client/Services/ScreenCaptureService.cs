using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CafeManagement.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace CafeManagement.Client.Services;

public class ScreenCaptureService : IScreenCaptureService
{
    private readonly ILogger<ScreenCaptureService> _logger;

    public ScreenCaptureService(ILogger<ScreenCaptureService> logger)
    {
        _logger = logger;
    }
    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

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

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public const int SRCCOPY = 0x00CC0020;
        public const int CAPTUREBLT = 0x40000000;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
    }

    public async Task<byte[]> CaptureFullDesktopAsync()
    {
        return await Task.Run(() =>
        {
            // Use GetSystemMetrics for more reliable screen dimension detection
            var width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
            var height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

            // Add logging to debug dimension detection
            _logger.LogInformation($"Screen dimensions: {width}x{height}");

            if (width <= 0 || height <= 0)
            {
                _logger.LogError("Invalid screen dimensions detected");
                return new byte[0];
            }

            return CaptureScreenWPF(0, 0, width, height);
        });
    }

    public async Task<byte[]> CaptureScreenAsync(int x, int y, int width, int height)
    {
        return await Task.Run(() => CaptureScreenWPF(x, y, width, height));
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

            return CaptureScreenWPF(rect.Left, rect.Top, width, height);
        });
    }

    private byte[] CaptureScreenWPF(int x, int y, int width, int height)
    {
        // Skip the problematic WPF VisualBrush approach and go directly to Win32
        return CaptureScreenWin32(x, y, width, height);
    }

    private byte[] CaptureScreenWin32(int x, int y, int width, int height)
    {
        try
        {
            _logger.LogInformation($"Starting screen capture: {x},{y} {width}x{height}");

            // Get the entire screen device context - not just a window
            IntPtr hdcSrc = NativeMethods.GetDC(IntPtr.Zero); // Get DC for entire screen
            _logger.LogInformation($"Source DC: {hdcSrc}");

            if (hdcSrc == IntPtr.Zero)
            {
                _logger.LogError("Failed to get source DC");
                return new byte[0];
            }

            IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            _logger.LogInformation($"Destination DC: {hdcDest}");

            if (hdcDest == IntPtr.Zero)
            {
                _logger.LogError("Failed to create destination DC");
                NativeMethods.ReleaseDC(IntPtr.Zero, hdcSrc);
                return new byte[0];
            }

            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, width, height);
            _logger.LogInformation($"Bitmap handle: {hBitmap}");

            if (hBitmap == IntPtr.Zero)
            {
                _logger.LogError("Failed to create bitmap");
                NativeMethods.DeleteDC(hdcDest);
                NativeMethods.ReleaseDC(IntPtr.Zero, hdcSrc);
                return new byte[0];
            }

            IntPtr hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
            _logger.LogInformation($"Old object: {hOld}");

            // Copy the specified screen area with CAPTUREBLT flag to capture hardware-accelerated content
            bool success = NativeMethods.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, x, y,
                NativeMethods.SRCCOPY | NativeMethods.CAPTUREBLT);
            _logger.LogInformation($"BitBlt success: {success} with CAPTUREBLT flag");

            // Clean up GDI objects
            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(IntPtr.Zero, hdcSrc);

            if (!success)
            {
                _logger.LogError("BitBlt operation failed");
                NativeMethods.DeleteObject(hBitmap);
                return new byte[0];
            }

            // Convert to Imaging.BitmapSource
            var bs = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            _logger.LogInformation($"BitmapSource created: {bs.Width}x{bs.Height}");
            NativeMethods.DeleteObject(hBitmap);

            // Convert to PNG byte array
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bs));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                byte[] result = stream.ToArray();
                _logger.LogInformation($"Final image size: {result.Length} bytes");
                return result;
            }
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            _logger.LogError($"Screen capture error: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
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

                image.SaveAsJpeg(outputMemoryStream);
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