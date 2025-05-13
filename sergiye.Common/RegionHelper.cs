﻿using System.Runtime.InteropServices;
using System.Text;

namespace sergiye.Common {

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
