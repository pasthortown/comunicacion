using System;
using System.IO;

namespace ImageActivityMonitor.Infrastructure
{
    public class ActivityLogger
    {
        private readonly string _logFilePath;

        public ActivityLogger(string logFileName = "activity.log")
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _logFilePath = Path.Combine(basePath, logFileName);

            // Crear encabezado si el archivo no existe
            if (!File.Exists(_logFilePath))
            {
                File.WriteAllText(_logFilePath, "FechaHora,Zona,Estado\n");
            }
        }

        public void Log(int zona, string estado)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{zona},{estado}";
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
    }
}
