using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace sergiye.Common {
  public class GitHubRelease {
    public string Tag_name { get; set; }
    public string Name { get; set; }
    public bool Prerelease { get; set; }
    public Asset[] Assets { get; set; }
    //public Uri assets_url { get; set; }
    //public Uri html_url { get; set; }
    //public DateTime created_at { get; set; }
    //public DateTime published_at { get; set; }
  }

  public class Asset {
    public string Name { get; set; }
    public string Browser_download_url { get; set; }
    //public string State { get; set; }
    //public Uri Url { get; set; }
    //public object Label { get; set; }
    //public string Content_type { get; set; }
    //public long Size { get; set; }
    //public long Download_count { get; set; }
    //public DateTime Created_at { get; set; }
    //public DateTime Updated_at { get; set; }
  }

  public static class Updater {
    public static readonly string ApplicationName;
    public static readonly string ApplicationTitle;
    public static readonly string ApplicationCompany;
    public static readonly string selfFileName;
    public static readonly string CurrentVersion;
    public static readonly string CurrentFileLocation;

    static Updater() {
      var asm = Assembly.GetExecutingAssembly(); //typeof(Updater).Assembly
      ApplicationName = GetAttribute<AssemblyProductAttribute>(asm)?.Product;
      ApplicationTitle = GetAttribute<AssemblyTitleAttribute>(asm)?.Title;
      ApplicationCompany = GetAttribute<AssemblyCompanyAttribute>(asm)?.Company;
      CurrentVersion = asm.GetName().Version.ToString(3); //Application.ProductVersion;
      CurrentFileLocation = asm.Location;
      selfFileName = Path.GetFileName(CurrentFileLocation);
      ServicePointManager.Expect100Continue = false;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
    }

    public static event Action<string, bool> OnMessage;
    public static event Func<string, bool> OnQuestion;
    public static event Action OnExit;

    public static void Subscribe(Action<string, bool> onMessage, Func<string, bool> onQuestion, Action onExit = null) {
      if (onMessage != null) OnMessage += onMessage;
      if (onQuestion != null) OnQuestion += onQuestion;
      if (onExit != null) OnExit += onExit;
    }

    private static T GetAttribute<T>(ICustomAttributeProvider assembly, bool inherit = false) where T : Attribute {
      foreach (var o in assembly.GetCustomAttributes(typeof(T), inherit))
        if (o is T attribute)
          return attribute;
      return null;
    }

    private static string GetAppSiteUrl(string subPage = null) {
      var url = $"https://github.com/{ApplicationCompany}/{ApplicationName}";
      if (!string.IsNullOrEmpty(subPage))
        url += "/" + subPage;
      return url;
    }
    
    private static string GetAppReleasesUrl() {
      return $"https://api.github.com/repos/{ApplicationCompany}/{ApplicationName}/releases";
    }
    
    /// <summary>
    /// Check for a new version
    /// </summary>
    /// <returns>True if the check was completed, False if there were errors</returns>
    public static bool CheckForUpdates(bool silent) {
      try {
        string jsonString;
        using (var wc = new WebClient()) {
          wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.500.27 Safari/537.36");
          jsonString = wc.DownloadString(GetAppReleasesUrl()); // .TrimEnd();
        }
        return CheckForUpdatesInternal(silent, jsonString);
      }
      catch (Exception ex) {
        if (!silent)
          OnMessage?.Invoke($"Error checking for a new version.\n{ex.Message}", true);
        return false;
      }
    }

    /// <summary>
    /// Check for a new version
    /// </summary>
    /// <returns>True if the check was completed, False if there were errors</returns>
    public static async Task<bool> CheckForUpdatesAsync(bool silent) {
      try {
        string jsonString;
        using (var wc = new WebClient()) {
          wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.500.27 Safari/537.36");
          jsonString = await wc.DownloadStringTaskAsync(GetAppReleasesUrl()).ConfigureAwait(false);
        }
        return CheckForUpdatesInternal(silent, jsonString);
      }
      catch (Exception ex) {
        if (!silent)
          OnMessage?.Invoke($"Error checking for a new version.\n{ex.Message}", true);
        return false;
      }
    }

    private static bool CheckForUpdatesInternal(bool silent, string jsonString) {
      var releases = jsonString.FromJson<GitHubRelease[]>();
      if (releases == null || releases.Length == 0)
        throw new Exception("Error getting list of releases.");

      var lastRelease = releases.FirstOrDefault(r => !r.Prerelease) ?? releases[0];
      var newVersion = lastRelease.Tag_name;
      var asset = lastRelease.Assets.FirstOrDefault(a => a.Name == selfFileName);
      var newVersionUrl = asset?.Browser_download_url;
      if (string.IsNullOrEmpty(newVersionUrl)) {
        if (!silent)
          OnMessage?.Invoke($"Your version is: {CurrentVersion}\nLatest released version is: {newVersion}\nNo assets found to update.", false);
        return true;
      }

      if (string.Compare(CurrentVersion, newVersion, StringComparison.Ordinal) >= 0) {
        if (!silent)
          OnMessage?.Invoke($"Your version: {CurrentVersion}\nLast release: {newVersion}\nNo need to update.", false);
        return true;
      }
      if (OnQuestion == null) {
        OnMessage?.Invoke($"New version found: {newVersion}, app will be updated after close.", false);
      }
      else {
        if (!OnQuestion($"Your version: {CurrentVersion}\nLast release: {newVersion}\nDownload this update?"))
          return true;
      }

      try {
        var tempPath = Path.GetTempPath();
        var updateFilePath = $"{tempPath}{selfFileName}";

        using (var wc = new WebClient())
          wc.DownloadFile(newVersionUrl, updateFilePath);
        RestartApp(3, updateFilePath);
        return true;
      }
      catch (Exception ex) {
        if (!silent)
          OnMessage?.Invoke($"Error downloading new version\n{ex.Message}", true);
        return false;
      }
    }

    public static void RestartApp(int timeout = 0, string replaceWithFile = null) {
      var cmdFilePath = Path.GetTempPath() + $"{selfFileName}_updater.cmd";
      using (var batFile = new StreamWriter(File.Create(cmdFilePath))) {
        batFile.WriteLine("@ECHO OFF");
        batFile.WriteLine($"TIMEOUT /t {timeout} /nobreak > NUL");
        batFile.WriteLine("TASKKILL /IM \"{0}\" > NUL", selfFileName);
        if (!string.IsNullOrEmpty(replaceWithFile))
          batFile.WriteLine("MOVE \"{0}\" \"{1}\"", replaceWithFile, CurrentFileLocation);
        batFile.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", CurrentFileLocation);
      }
      var startInfo = new ProcessStartInfo(cmdFilePath) {
        CreateNoWindow = true,
        UseShellExecute = false,
        WorkingDirectory = Path.GetTempPath()
      };
      Process.Start(startInfo);
      OnExit?.Invoke();
      Environment.Exit(0);
    }

    public static void VisitAppSite(string subPage = null) {
      Process.Start(GetAppSiteUrl(subPage));
    }
  }
}
