using System;

namespace EasySave.WPF.Models
{
    public class BlockedProcessException : Exception
    {
        public string ProcessName { get; }

        public BlockedProcessException(string processName) 
            : base($"The process '{processName}' is running and must be closed.")
        {
            ProcessName = processName;
        }
    }
}
