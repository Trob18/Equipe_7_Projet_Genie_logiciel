using EasySave.Log.Models;

namespace EasySave.Log.Interfaces
{
    public interface ILogger
    {
        void WriteLog(LogEntry logEntry);
    }
}