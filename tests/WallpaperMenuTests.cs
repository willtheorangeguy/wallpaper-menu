using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

internal static class WallpaperMenuTests
{
    private static int failures;
    private static int passed;

    private static int Main()
    {
        Run("Finds monitors left of and above the primary display", SelectsOffsetMonitors);
        Run("Uses the adjacent monitor at a shared edge", SelectsSharedEdge);
        Run("Rejects gaps and disconnected monitors", RejectsInactiveAreas);
        Run("Advances the selected monitor when supported", AdvancesSelectedMonitor);
        Run("Stops the slideshow fallback when the selected wallpaper changes", StopsFallbackAtSelectedMonitor);
        Run("Bounds slideshow fallback attempts", BoundsFallbackAttempts);
        Run("Stops when Windows refuses to advance the slideshow", StopsWhenFallbackFails);
        Run("Skips a wallpaper that already changed", SkipsChangedWallpaper);
        TestWallpaperPaths();

        Console.WriteLine("{0} passed, {1} failed", passed, failures);
        return failures == 0 ? 0 : 1;
    }

    private static void SelectsOffsetMonitors()
    {
        var desktop = new TestDesktop();
        Equal("left", WallpaperMenu.MonitorAt(desktop, new Point(-1920, 0)));
        Equal("above", WallpaperMenu.MonitorAt(desktop, new Point(100, -1200)));
    }

    private static void SelectsSharedEdge()
    {
        var desktop = new TestDesktop();
        Equal("left", WallpaperMenu.MonitorAt(desktop, new Point(-1, 400)));
        Equal("primary", WallpaperMenu.MonitorAt(desktop, new Point(0, 400)));
    }

    private static void RejectsInactiveAreas()
    {
        var desktop = new TestDesktop();
        Rejected(() => WallpaperMenu.MonitorAt(desktop, new Point(-1, -1)));
        Rejected(() => WallpaperMenu.MonitorAt(desktop, new Point(3500, 500)));
    }

    private static void AdvancesSelectedMonitor()
    {
        var desktop = new TestDesktop { SupportsTargetedAdvance = true };
        WallpaperMenu.RefreshMonitor(desktop, "primary", "primary.jpg");
        Equal("replacement.jpg", desktop.GetWallpaper("primary"));
        Equal("left.jpg", desktop.GetWallpaper("left"));
        Equal("primary", String.Join(",", desktop.AdvanceCalls));
    }

    private static void StopsFallbackAtSelectedMonitor()
    {
        var desktop = new TestDesktop("left", "primary", "above");
        WallpaperMenu.RefreshMonitor(desktop, "primary", "primary.jpg");
        Equal("replacement.jpg", desktop.GetWallpaper("primary"));
        Equal("above.jpg", desktop.GetWallpaper("above"));
        Equal("primary,next,next", String.Join(",", desktop.AdvanceCalls));
    }

    private static void BoundsFallbackAttempts()
    {
        var desktop = new TestDesktop("left", "left", "left", "left", "primary");
        WallpaperMenu.RefreshMonitor(desktop, "primary", "primary.jpg");
        Equal("primary.jpg", desktop.GetWallpaper("primary"));
        Equal("primary,next,next,next,next", String.Join(",", desktop.AdvanceCalls));
    }

    private static void StopsWhenFallbackFails()
    {
        var desktop = new TestDesktop();
        WallpaperMenu.RefreshMonitor(desktop, "primary", "primary.jpg");
        Equal("primary,next", String.Join(",", desktop.AdvanceCalls));
    }

    private static void SkipsChangedWallpaper()
    {
        var desktop = new TestDesktop();
        WallpaperMenu.RefreshMonitor(desktop, "primary", "previous.jpg");
        Equal(0, desktop.AdvanceCalls.Count);
    }

    private static void TestWallpaperPaths()
    {
        string root = Path.Combine(Path.GetTempPath(), "WallpaperMenu-" + Guid.NewGuid().ToString("N"));
        string folder = Directory.CreateDirectory(Path.Combine(root, "Wallpapers")).FullName;
        try
        {
            string album = Directory.CreateDirectory(Path.Combine(folder, "Trip album")).FullName;
            string sibling = Directory.CreateDirectory(Path.Combine(root, "WallpapersBackup")).FullName;
            string photo = CreateFile(folder, "Photo.jpg");
            string nestedPhoto = CreateFile(album, "Museum.JPG");
            string backupPhoto = CreateFile(sibling, "Photo.jpg");
            string textFile = CreateFile(folder, "Notes.txt");
            string directory = Directory.CreateDirectory(Path.Combine(folder, "directory.png")).FullName;

            Run("Accepts photos in the wallpaper folder and its subfolders", () =>
            {
                WallpaperMenu.ValidateWallpaper(folder, photo);
                WallpaperMenu.ValidateWallpaper(folder, nestedPhoto);
            });
            Run("Rejects photos in a similarly named sibling folder", () =>
            {
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, backupPhoto));
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder,
                    Path.Combine(folder, @"..\WallpapersBackup\Photo.jpg")));
            });
            Run("Rejects missing files, directories, and unsupported extensions", () =>
            {
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, Path.Combine(folder, "missing.jpg")));
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, directory));
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, textFile));
            });
            Run("Rejects empty and relative paths", () =>
            {
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, null));
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, " "));
                Rejected(() => WallpaperMenu.ValidateWallpaper(folder, "Photo.jpg"));
            });
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static string CreateFile(string directory, string name)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllText(path, "Disposable test fixture");
        return path;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            passed++;
            Console.WriteLine("PASS {0}", name);
        }
        catch (Exception error)
        {
            failures++;
            Console.Error.WriteLine("FAIL {0}\n{1}", name, error);
        }
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception(String.Format("Expected '{0}', got '{1}'.", expected, actual));
    }

    private static void Rejected(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException)
        {
            return;
        }
        throw new Exception("Expected an InvalidOperationException.");
    }

    private sealed class TestDesktop : WallpaperMenu.IDesktopWallpaper
    {
        private const int NotImplemented = unchecked((int)0x80004001);
        private static readonly string[] Monitors = { "left", "primary", "above", "disconnected" };
        private static readonly WallpaperMenu.MonitorRect[] Areas =
        {
            new WallpaperMenu.MonitorRect { Left = -1920, Top = 0, Right = 0, Bottom = 1080 },
            new WallpaperMenu.MonitorRect { Left = 0, Top = 0, Right = 2560, Bottom = 1440 },
            new WallpaperMenu.MonitorRect { Left = 100, Top = -1200, Right = 1700, Bottom = 0 },
            new WallpaperMenu.MonitorRect { Left = 3000, Top = 0, Right = 4000, Bottom = 1000 }
        };

        private readonly Dictionary<string, string> wallpapers = new Dictionary<string, string>
        {
            { "left", "left.jpg" }, { "primary", "primary.jpg" }, { "above", "above.jpg" }
        };

        private readonly Queue<string> scheduled;
        public readonly List<string> AdvanceCalls = new List<string>();
        public bool SupportsTargetedAdvance;

        public TestDesktop(params string[] scheduledMonitors)
        {
            scheduled = new Queue<string>(scheduledMonitors);
        }

        public uint GetMonitorDevicePathCount() { return (uint)Monitors.Length; }
        public string GetMonitorDevicePathAt(uint index) { return Monitors[index]; }
        public string GetWallpaper(string monitor) { return wallpapers[monitor]; }

        public int GetMonitorRECT(string monitor, out WallpaperMenu.MonitorRect area)
        {
            area = Areas[Array.IndexOf(Monitors, monitor)];
            return monitor == "disconnected" ? 1 : 0;
        }

        public int AdvanceSlideshow(string monitor, int direction)
        {
            Equal(0, direction);
            AdvanceCalls.Add(monitor ?? "next");
            if (monitor == null && scheduled.Count > 0)
                monitor = scheduled.Dequeue();
            else if (monitor == null || !SupportsTargetedAdvance)
                return NotImplemented;

            wallpapers[monitor] = "replacement.jpg";
            return 0;
        }

        public void SetWallpaper() { throw new NotSupportedException(); }
        public void SetBackgroundColor() { throw new NotSupportedException(); }
        public void GetBackgroundColor() { throw new NotSupportedException(); }
        public void SetPosition() { throw new NotSupportedException(); }
        public void GetPosition() { throw new NotSupportedException(); }
        public void SetSlideshow() { throw new NotSupportedException(); }
        public void GetSlideshow() { throw new NotSupportedException(); }
        public void SetSlideshowOptions() { throw new NotSupportedException(); }
        public void GetSlideshowOptions() { throw new NotSupportedException(); }
    }
}
