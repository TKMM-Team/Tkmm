using System;
using Tcml.Attributes;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using ConfigFactory.Avalonia.Helpers;
using ConfigFactory.Core.Attributes;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tcml.Builders.MenuModels
{
    public class ModInfo
    {
        // Define the properties of the ModInfo class
        public string Name { get; set; }
        // Add other properties as needed
    }

    public class ShellViewMenu
    {
        [Menu("Exit", "File", "Alt + F4", "fa-solid fa-right-from-bracket")]
        public static void File_Exit()
        {
            // Handle exit cleanup here
            Environment.Exit(0);
        }

        [Menu("Import", "Mod", "Ctrl + I", "fa-solid fa-right-from-bracket")]
        public static async Task Mod_Import()
        {
            BrowserDialog dialog = new(BrowserMode.OpenFolder, "Open Mod");
            string selectedFolder = await dialog.ShowDialog();

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                string jsonFilePath = Path.Combine(selectedFolder, "info.json");

                if (File.Exists(jsonFilePath))
                {
                    Trace.WriteLine("Found the JSON!");
                    return;
                }

                string jsonContent = File.ReadAllText(jsonFilePath);

                ModInfo modInfo;
                try
                {
                    modInfo = JsonSerializer.Deserialize<ModInfo>(jsonContent);
                }
                catch (JsonException ex)
                {
                    // Handle JSON parsing errors
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    return;
                }

                // Handle the rest of your import logic here using modInfo
            }
        }
    }
}
