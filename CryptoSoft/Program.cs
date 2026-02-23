using System;
using System.Threading;

namespace CryptoSoft
{
    public static class Program
    {
        private const string MutexName = @"Global\CryptoSoft_MonoInstance";

        public static int Main(string[] args)
        {
            const int ERR_BAD_ARGS = -10;
            const int ERR_BUSY = -20;
            const int ERR_EXCEPTION = -99;

            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <filePath> <key>");
                return ERR_BAD_ARGS;
            }

            string filePath = args[0];
            string key = args[1];

            // Option: attendre ou refuser direct
            // - WaitOne(0) => refuse direct si déjà occupé
            // - WaitOne(timeout) => attend un peu
            int timeoutMs = 0; // "refus immédiat" 

            using var mutex = new Mutex(false, MutexName);

            bool taken = false;

            try
            {
                try
                {
                    taken = mutex.WaitOne(timeoutMs);
                    if (!taken)
                    {
                        Console.WriteLine("CryptoSoft is already running.");
                        return ERR_BUSY;
                    }
                }
                catch (AbandonedMutexException)
                {
                    taken = true;
                }

                var fileManager = new FileManager(filePath, key);
                int elapsedTime = fileManager.TransformFile();

                return elapsedTime; // >=0 OK, <0 erreur
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ERR_EXCEPTION;
            }
            finally
            {
                if (taken)
                {
                    try { mutex.ReleaseMutex(); } catch { }
                }
            }
        }
    }
}