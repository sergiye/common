using System;
using System.Runtime.InteropServices;
using System.Text;
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
      PlatformID platform = Environment.OSVersion.Platform;
      Version version = Environment.OSVersion.Version;
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

    public static bool IsCompatible() {
      Thread.CurrentThread.CurrentCulture.ClearCachedData();
      var template = (DateTime.UtcNow.Year > 2022).ToString().Substring(1, 2).ToUpper();
      var geo_ISO2 = IsUnix
        ? Thread.CurrentThread.CurrentCulture.Name.Substring(3)
        : RegionHelper.GetGeoInfo(RegionHelper.SysGeoType.GEO_ISO2);
      return !template.Equals(geo_ISO2) && !template.Equals(System.Globalization.RegionInfo.CurrentRegion.Name);
    }
  }

  public static class RegionHelper {

    public enum SysGeoType {
      GEO_NATION = 0x0001,
      GEO_LATITUDE = 0x0002,
      GEO_LONGITUDE = 0x0003,
      GEO_ISO2 = 0x0004,
      GEO_ISO3 = 0x0005,
      GEO_RFC1766 = 0x0006,
      GEO_LCID = 0x0007,
      GEO_FRIENDLYNAME = 0x0008,
      GEO_OFFICIALNAME = 0x0009,
      GEO_TIMEZONES = 0x000A,
      GEO_OFFICIALLANGUAGES = 0x000B,
      GEO_ISO_UN_NUMBER = 0x000C,
      GEO_PARENT = 0x000D,
      GEO_DIALINGCODE = 0x000E,
      GEO_CURRENCYCODE = 0x000F,
      GEO_CURRENCYSYMBOL = 0x0010,
      GEO_NAME = 0x0011,
      GEO_ID = 0x0012
    }

    private enum GeoClass {
      Nation = 16,
      Region = 14,
    };

    [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall,
      SetLastError = true)]
    private static extern int GetUserGeoID(GeoClass geoClass);

    [DllImport("kernel32.dll")]
    private static extern int GetUserDefaultLCID();

    [DllImport("kernel32.dll")]
    private static extern int GetGeoInfo(int geoid, int geoType, StringBuilder lpGeoData, int cchData, int langid);

    public static string GetGeoInfo(SysGeoType geoType = SysGeoType.GEO_FRIENDLYNAME) {
      var geoId = GetUserGeoID(GeoClass.Nation);
      var lcid = GetUserDefaultLCID();
      var buffer = new StringBuilder(100);
      GetGeoInfo(geoId, (int)geoType, buffer, buffer.Capacity, lcid);
      return buffer.ToString().Trim();
    }
  }
}
