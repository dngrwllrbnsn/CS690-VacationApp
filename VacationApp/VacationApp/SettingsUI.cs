using System;
using VacationApp.Settings;

namespace VacationApp.UI
{
    public class SettingsUI
    {
        private readonly SettingsManager settingsManager;
        
        public SettingsUI(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
        }
        
        // show the settings menu
        public void ShowSettingsMenu()
        {
            while (true)
            {
                Console.Clear();
                DrawHeader("Settings");
                
                var settings = settingsManager.GetSettings();
                
                Console.WriteLine("Current Settings:");
                Console.WriteLine($"Default Currency: {settings.DefaultCurrency}");
                Console.WriteLine($"Auto Save: {(settings.AutoSave ? "Enabled" : "Disabled")}");
                Console.WriteLine($"Data Save Path: {settings.DataSavePath}");
                
                if (settings.CustomSettings.Count > 0)
                {
                    Console.WriteLine("\nCustom Settings:");
                    foreach (var setting in settings.CustomSettings)
                    {
                        Console.WriteLine($"{setting.Key}: {setting.Value}");
                    }
                }
                
                Console.WriteLine();
                
                string[] options = {
                    "Change Default Currency",
                    "Toggle Auto Save",
                    "Change Data Save Path",
                    "Add Custom Setting",
                    "Reset to Defaults",
                    "Back to Main Menu"
                };
                
                Console.WriteLine("Use ↑/↓ arrow keys to navigate, Enter to select:");
                Console.WriteLine();
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // change default currency
                        ChangeDefaultCurrency();
                        break;
                    case 1: // toggle auto save
                        settingsManager.UpdateSetting("AutoSave", !settings.AutoSave);
                        break;
                    case 2: // change data save path
                        ChangeDataSavePath();
                        break;
                    case 3: // add custom setting
                        AddCustomSetting();
                        break;
                    case 4: // reset to defaults
                        if (ConfirmReset())
                        {
                            settingsManager.ResetSettings();
                        }
                        break;
                    case 5: // back to main menu
                        return;
                }
            }
        }
        
        // change default currency
        private void ChangeDefaultCurrency()
        {
            Console.Clear();
            DrawHeader("Change Default Currency");
            
            Console.WriteLine("Common currencies: USD, EUR, GBP, JPY, CAD, AUD");
            Console.Write("\nEnter new default currency code: ");
            
            string currency = Console.ReadLine().ToUpper();
            
            if (!string.IsNullOrWhiteSpace(currency))
            {
                settingsManager.UpdateSetting("DefaultCurrency", currency);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nDefault currency updated to {currency}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made.");
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // change data save path
        private void ChangeDataSavePath()
        {
            Console.Clear();
            DrawHeader("Change data save path");
            
            var settings = settingsManager.GetSettings();
            Console.WriteLine($"Current path: {settings.DataSavePath}");
            Console.Write("\nEnter new path: ");
            
            string path = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(path))
            {
                // add trailing slash if missing
                if (!path.EndsWith("/") && !path.EndsWith("\\"))
                {
                    path += "/";
                }
                
                settingsManager.UpdateSetting("DataSavePath", path);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nData save path updated to {path}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made.");
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // add a custom setting
        private void AddCustomSetting()
        {
            Console.Clear();
            DrawHeader("Add a Custom Setting");
            
            Console.Write("Setting name: ");
            string key = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("\nInvalid setting name. Operation cancelled.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            Console.Write("Setting value: ");
            string value = Console.ReadLine();
            
            settingsManager.UpdateSetting(key, value);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nCustom setting '{key}' added/updated successfully!");
            Console.ResetColor();
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // confirm settings reset
        private bool ConfirmReset()
        {
            Console.Clear();
            DrawHeader("Reset Settings");
            
            Console.WriteLine("Warning: This will reset all settings to their default values.");
            Console.Write("\nAre you sure you want to continue? (Y/N): ");
            
            return Console.ReadLine()?.ToUpper() == "Y";
        }
        
        // Apply color scheme based on dark mode setting
        private void ApplyColorScheme(bool darkMode)
        {
            if (darkMode)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            
            Console.Clear();
        }
        
        // method to draw a header
        private void DrawHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            Console.ResetColor();
        }
        
        // method to show a menu with arrow key navigation
        private int ShowMenu(string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;
            int startRow = Console.CursorTop;
            
            do
            {
                // display all options
                for (int i = 0; i < options.Length; i++)
                {
                    Console.SetCursorPosition(0, startRow + i);
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.SetCursorPosition(0, startRow + i);
                    
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($" {options[i]} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($" {options[i]} ");
                    }
                }
                
                // get key press
                key = Console.ReadKey(true).Key;
                
                // handle arrow keys
                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < options.Length - 1) ? selectedIndex + 1 : 0;
                }
                
            } while (key != ConsoleKey.Enter);
            
            // move cursor to end of menu
            Console.SetCursorPosition(0, startRow + options.Length);
            
            return selectedIndex;
        }
    }
}