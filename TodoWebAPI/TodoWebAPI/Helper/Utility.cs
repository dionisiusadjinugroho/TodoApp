using System.Reflection;

namespace TodoWebAPI.Helper
{
    public class Utility
    {
        private static object LockObject = new object();
        public void WriteToFile(string method, string url, string bodyPayload, string returnValue, string statuscode)
        {
            string customLogPath = "";
            if (string.IsNullOrEmpty(customLogPath))
            {
                customLogPath = Path.GetDirectoryName(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))) + "/customlogs/";
            }

            if (!Directory.Exists(customLogPath))
            {
                Directory.CreateDirectory(customLogPath);
            }

            string filepath = customLogPath + string.Format("[{0}] TodoAPILog.txt", DateTime.Now.ToString("yyyy-MM-dd"));

            try
            {
                lock (LockObject)
                {
                    string logMessage = FormatLogMessage(method, url, bodyPayload, returnValue, statuscode);

                    if (logMessage.Length > 1000)
                    {
                        logMessage = logMessage.Substring(0, 1000);
                    }

                    if (!File.Exists(filepath))
                    {
                        using (StreamWriter sw = File.CreateText(filepath))
                        {
                            sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), logMessage));
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(filepath))
                        {
                            sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), logMessage));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log failed: {ex.Message}");
            }
        }

        private string FormatLogMessage(string method, string url, string bodyPayload, string returnValue, string statuscode)
        {
            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return $"{method} {url} Return value: {returnValue} Status Code: {statuscode}";
            }
            else
            {
                return $"{method} {url} BodyPayload: {bodyPayload} Return value: {returnValue} Status Code: {statuscode}";
            }
        }

    }
}
