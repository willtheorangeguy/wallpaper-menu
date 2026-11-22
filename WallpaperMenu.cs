using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class WallpaperMenu
{
    private const string Title = "Remove this wallpaper";
    private const string MenuKey = @"Software\Classes\DesktopBackground\Shell\WallpaperMenu.RemoveCurrent";
    private static readonly Guid WallpaperClass = new Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD");
    private static readonly string[] Extensions = {
        ".jpg", ".jpeg", ".jfif", ".png", ".bmp", ".gif", ".tif", ".tiff", ".webp", ".heic", ".avif"
    };

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || (args.Length == 1 && args[0] == "--install"))
            {
                InstallMenu();
                if (args.Length == 0)
                    MessageBox.Show("Installed. Right-click an empty spot on the desktop, choose Show more options, then choose Remove this wallpaper.", Title);
                return 0;
            }

            if (args.Length == 1 && args[0] == "--uninstall")
            {
                Registry.CurrentUser.DeleteSubKeyTree(MenuKey, false);
                NotifyMenuChanged();
                return 0;
            }

            bool checking = args.Length == 1 && args[0] == "--check";
            if (!checking && (args.Length != 1 || args[0] != "--remove"))
                throw new ArgumentException("Unknown command.");

            Point cursor;
            if (!GetPhysicalCursorPos(out cursor))
                throw new InvalidOperationException("Windows could not locate the mouse pointer.");

            IDesktopWallpaper desktop = (IDesktopWallpaper)Activator.CreateInstance(
                Type.GetTypeFromCLSID(WallpaperClass, true));
            try
            {
                if (checking)
                {
                    string monitor = MonitorAt(desktop, cursor);
                    string path = desktop.GetWallpaper(monitor);
                    Console.WriteLine("Current wallpaper: " + path);
                    ValidateWallpaper(WallpapersFolder(), path);
                    Console.WriteLine("Found an eligible slideshow photo.");
                    return 0;
                }

                bool ownsMutex;
                using (Mutex mutex = new Mutex(true, @"Local\WallpaperMenu.RemoveCurrent", out ownsMutex))
                {
                    if (!ownsMutex)
                        return 0;
                    try
                    {
                        RemoveCurrent(desktop, cursor);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                return 0;
            }
            finally
            {
                Marshal.ReleaseComObject(desktop);
            }
        }
        catch (Exception error)
        {
            if (args.Length == 1 && args[0] == "--check")
                Console.Error.WriteLine(error.Message);
            else
                MessageBox.Show(error.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return 1;
        }
    }

    private static void InstallMenu()
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WallpaperMenu");
        string installedPath = Path.Combine(directory, "WallpaperMenu.exe");
        if (!String.Equals(Application.ExecutablePath, installedPath, StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(directory);
            File.Copy(Application.ExecutablePath, installedPath, true);
        }

        using (RegistryKey menu = Registry.CurrentUser.CreateSubKey(MenuKey))
        {
            menu.SetValue("MUIVerb", Title);
            using (RegistryKey command = menu.CreateSubKey("command"))
                command.SetValue("", "\"" + installedPath + "\" --remove");
        }
        NotifyMenuChanged();
    }

    private static string WallpapersFolder()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Wallpapers");
    }

    internal static string MonitorAt(IDesktopWallpaper desktop, Point cursor)
    {
        uint count = desktop.GetMonitorDevicePathCount();
        for (uint index = 0; index < count; index++)
        {
            string monitor = desktop.GetMonitorDevicePathAt(index);
            MonitorRect area;
            if (desktop.GetMonitorRECT(monitor, out area) == 0 &&
                cursor.X >= area.Left && cursor.X < area.Right &&
                cursor.Y >= area.Top && cursor.Y < area.Bottom)
                return monitor;
        }
        throw new InvalidOperationException("Windows could not identify the monitor under the mouse pointer.");
    }

    internal static void ValidateWallpaper(string folder, string path)
    {
        if (String.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
            throw new InvalidOperationException("Windows did not report a wallpaper file on this monitor.");

        string fullPath = Path.GetFullPath(path);
        string prefix = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !Array.Exists(Extensions, extension => String.Equals(extension, Path.GetExtension(fullPath), StringComparison.OrdinalIgnoreCase)) ||
            !File.Exists(fullPath))
            throw new InvalidOperationException("The current wallpaper is not an available photo in Pictures\\Wallpapers.");

        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException("The wallpaper is a linked file.");
        for (DirectoryInfo directory = new FileInfo(fullPath).Directory; directory != null; directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("The wallpaper is inside a linked folder.");
            if (String.Equals(directory.FullName + Path.DirectorySeparatorChar, prefix, StringComparison.OrdinalIgnoreCase))
                break;
        }
    }

    private static void RemoveCurrent(IDesktopWallpaper desktop, Point cursor)
    {
        string folder = WallpapersFolder();
        string monitor = MonitorAt(desktop, cursor);
        string path = desktop.GetWallpaper(monitor);
        ValidateWallpaper(folder, path);
        if (!Recycle(path))
            return;

        try
        {
            RefreshMonitor(desktop, monitor, path);
        }
        catch (COMException)
        {
            MessageBox.Show("The photo was removed. Right-click the desktop and choose Next desktop background to change the displayed image.", Title);
        }
    }

    internal static void RefreshMonitor(IDesktopWallpaper desktop, string monitor, string previousPath)
    {
        if (!String.Equals(desktop.GetWallpaper(monitor), previousPath, StringComparison.OrdinalIgnoreCase))
            return;

        if (desktop.AdvanceSlideshow(monitor, 0) >= 0)
            return;

        // Some Windows versions can only advance the monitor scheduled to change next.
        uint count = desktop.GetMonitorDevicePathCount();
        for (uint attempt = 0; attempt < count; attempt++)
        {
            if (desktop.AdvanceSlideshow(null, 0) < 0 ||
                !String.Equals(desktop.GetWallpaper(monitor), previousPath, StringComparison.OrdinalIgnoreCase))
                return;
        }
    }

    private static bool Recycle(string path)
    {
        Guid shellItemInterface = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
        IntPtr item;
        Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(path, IntPtr.Zero, ref shellItemInterface, out item));
        try
        {
            Guid fileOperationClass = new Guid("3AD05575-8857-4850-9277-11B85BDB8E09");
            IFileOperation operation = (IFileOperation)Activator.CreateInstance(Type.GetTypeFromCLSID(fileOperationClass, true));
            try
            {
                const uint silent = 0x0004;
                const uint warnBeforePermanentDelete = 0x4000;
                const uint recycleOnDelete = 0x00080000;
                operation.SetOperationFlags(silent | warnBeforePermanentDelete | recycleOnDelete);
                operation.DeleteItem(item, IntPtr.Zero);
                operation.PerformOperations();
                if (operation.GetAnyOperationsAborted())
                    return false;
                if (File.Exists(path))
                    throw new IOException("Windows could not remove the wallpaper file.");
                return true;
            }
            finally
            {
                Marshal.ReleaseComObject(operation);
            }
        }
        finally
        {
            Marshal.Release(item);
        }
    }

    private static void NotifyMenuChanged()
    {
        const uint associationsChanged = 0x08000000;
        SHChangeNotify(associationsChanged, 0, IntPtr.Zero, IntPtr.Zero);
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr first, IntPtr second);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicalCursorPos(out Point point);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHCreateItemFromParsingName(
        string path, IntPtr bindContext, ref Guid interfaceId,
        out IntPtr item);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorRect
    {
        public int Left, Top, Right, Bottom;
    }

    [ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDesktopWallpaper
    {
        // Methods are declared in native vtable order. Unused slots are placeholders.
        void SetWallpaper();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitor);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetMonitorDevicePathAt(uint index);
        uint GetMonitorDevicePathCount();
        [PreserveSig]
        int GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitor, out MonitorRect area);
        void SetBackgroundColor();
        void GetBackgroundColor();
        void SetPosition();
        void GetPosition();
        void SetSlideshow();
        void GetSlideshow();
        void SetSlideshowOptions();
        void GetSlideshowOptions();
        [PreserveSig]
        int AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitor, int direction);
    }

    [ComImport, Guid("947AAB5F-0A5C-4C13-B4D6-4BF7836FC9F8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOperation
    {
        // Methods are declared in native vtable order. Unused slots are placeholders.
        void Advise();
        void Unadvise();
        void SetOperationFlags(uint flags);
        void SetProgressMessage();
        void SetProgressDialog();
        void SetProperties();
        void SetOwnerWindow();
        void ApplyPropertiesToItem();
        void ApplyPropertiesToItems();
        void RenameItem();
        void RenameItems();
        void MoveItem();
        void MoveItems();
        void CopyItem();
        void CopyItems();
        void DeleteItem(IntPtr item, IntPtr progressSink);
        void DeleteItems();
        void NewItem();
        void PerformOperations();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetAnyOperationsAborted();
    }
}
