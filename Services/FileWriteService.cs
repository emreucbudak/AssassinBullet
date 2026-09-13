using AssassinBullet.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AssassinBullet.Services
{
    public class FileWriteService
    {
        public static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public static event Action<string>? ResultWritten;

        public static async Task FileWrite(string content)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                string path = Path.Combine(FileWriteSettings.FileWriteLocation, FileWriteSettings.FileName);
                await File.AppendAllTextAsync(path, content + Environment.NewLine);
            }
            catch(Exception ex)
            {
                var exmessage = ex.Message;
                return;
            }
            finally
            {
                semaphoreSlim.Release();
            }

            ResultWritten?.Invoke(content);
        }
    }
}
