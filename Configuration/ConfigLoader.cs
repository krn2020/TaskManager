using System.Xml.Linq;

namespace TaskManager.Configuration
{
    public static class ConfigLoader
    {
        public static int LoadWorkerCount(string filePath = "config.xml")
        {
            if (!File.Exists(filePath))
            {
                // Создаём файл по умолчанию
                var defaultXml = @"<Configuration><WorkerCount>3</WorkerCount></Configuration>";
                File.WriteAllText(filePath, defaultXml);
            }

            var doc = XDocument.Load(filePath);
            var element = doc.Root?.Element("WorkerCount");
            if (element != null && int.TryParse(element.Value, out int count) && count > 0)
                return count;
            else
                return 3; // значение по умолчанию
        }
    }
}
