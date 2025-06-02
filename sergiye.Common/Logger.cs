using System;
using System.IO;

namespace sergiye.Common {

  public class Logger {

    public enum StateKind {
      Log,
      Info,
      Error,
    }

    public event Action<string, StateKind, bool> OnNewLogEvent;

    public virtual void Log(string message, StateKind kind = StateKind.Log, bool newLine = true) {
      OnNewLogEvent?.Invoke(message, kind, newLine);
    }
  }

  public class FileLogger: Logger, IDisposable {

    private readonly StreamWriter writer;

    public FileLogger(string logPath = null) {

      if (string.IsNullOrEmpty(logPath))
        logPath = Path.Combine(Path.GetDirectoryName(Updater.CurrentFileLocation), $"{Updater.ApplicationName}.log");
      writer = new StreamWriter(logPath, true);
    }

    public void Dispose() {
      writer?.Dispose();
    }

    public override void Log(string message, StateKind kind = StateKind.Log, bool newLine = true) {
      if (newLine)
        writer.Write($"\n{DateTime.Now:u} - ");
      writer.Write(message);

      base.Log(message, kind, newLine);
    }
  }
}
