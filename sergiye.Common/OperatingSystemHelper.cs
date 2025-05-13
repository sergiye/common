using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;

namespace sergiye.Common {
  /// <summary>
  /// Contains basic information about the operating system.
  /// </summary>
  public static class OperatingSystemHelper {
    /// <summary>
    /// Statically checks if the current system <see cref="Is64Bit"/> and <see cref="IsUnix"/>.
    /// </summary>
    static OperatingSystemHelper() {
      // The operating system doesn't change during execution so let's query it just one time.
      var platform = Environment.OSVersion.Platform;
      var version = Environment.OSVersion.Version;
      IsUnix = platform == PlatformID.Unix || platform == PlatformID.MacOSX;

      if (Environment.Is64BitOperatingSystem)
        Is64Bit = true;

      IsWindows8OrGreater = !IsUnix && ((version.Major == 6 && version.Minor >= 2) || version.Major > 6);
    }

    /// <summary>
    /// Gets information about whether the current system is 64 bit.
    /// </summary>
    public static bool Is64Bit { get; }

    /// <summary>
    /// Gets information about whether the current system is Unix based.
    /// </summary>
    public static bool IsUnix { get; }

    /// <summary>
    /// Returns true if the current system is Windows 8 or a more recent Windows version
    /// </summary>
    public static bool IsWindows8OrGreater { get; }

    public static bool IsCompatible(bool checkRedist, out string errorMessage, out Action fixAction) {
      errorMessage = null;
      fixAction = null;
      if (Environment.Is64BitOperatingSystem != Environment.Is64BitProcess) {
        errorMessage = $"You are running an application build made for a different OS architecture.\nIt is not compatible!\nWould you like to download correct version?";
        fixAction = () => Updater.VisitAppSite("releases");
        return false;
      }

      var sysArch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
      if (!IsVcRedistInstalled(sysArch)) {
        errorMessage = "Microsoft Visual C++ 2015-2022 Redistributable is not installed.\nWould you like to download it now?";
        fixAction = () => Process.Start($"https://aka.ms/vs/17/release/vc_redist.{sysArch}.exe");
        return false;
      }

#if LITEVERSION
      return true;
#else
      Thread.CurrentThread.CurrentCulture.ClearCachedData();
      var template = (DateTime.UtcNow.Year > 2022).ToString().Substring(1, 2).ToUpper();
      var geo_ISO2 = IsUnix
        ? Thread.CurrentThread.CurrentCulture.Name.Substring(3)
        : RegionHelper.GetGeoInfo(RegionHelper.SysGeoType.GEO_ISO2);
      if (template.Equals(geo_ISO2) || template.Equals(System.Globalization.RegionInfo.CurrentRegion.Name)) {
        errorMessage = "The application is not compatible with your region.";
        return false;
      }
      return true;
#endif
    }

    public static bool IsVcRedistInstalled(string arch) {
      var registryKey = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\" + arch;
      var view = (arch == "x64") ? RegistryView.Registry64 : RegistryView.Registry32;
      using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
      using (var key = baseKey.OpenSubKey(registryKey)) {
        return key != null && key.GetValue("Installed") is int installed && installed == 1;
      }
    }
  }
}
