using System;

namespace sergiye.Common {

  public class Logger {

    public enum StateKind {
      Log,
      Info,
      Error,
    }

    public event Action<string, StateKind, bool> OnNewLogEvent;

    public void Log(string message, StateKind kind = StateKind.Log, bool newLine = true) {
      OnNewLogEvent?.Invoke(message, kind, newLine);
    }
  }
}
