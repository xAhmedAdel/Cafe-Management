namespace CafeManagement.Client.Services.Interfaces;

public interface IScreenCaptureService
{
    Task<byte[]> CaptureFullDesktopAsync();
    Task<byte[]> CaptureScreenAsync(int x, int y, int width, int height);
    Task<byte[]> CaptureWindowAsync(IntPtr windowHandle);
    byte[] CompressImage(byte[] imageData, int quality = 75);
    byte[] CompressWithDeflate(byte[] data);
}