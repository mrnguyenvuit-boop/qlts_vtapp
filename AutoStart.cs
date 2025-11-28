using Microsoft.Win32;

public static class AutoStart
{
    public static void Enable()
    {
        string exe = Application.ExecutablePath;
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        key.SetValue("ClientPrinterTray", exe);
    }
    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        key.DeleteValue("ClientPrinterTray", false);
    }
}
