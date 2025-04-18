using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace VacationApp.Settings
{
    // class to store app settings
    public class AppSettings
    {
        public string DefaultCurrency { get; set; } = "USD";
        public bool AutoSave { get; set; } = true;
        public string DataSavePath { get; set; } = "./data/";
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }
    
    // class to handle settings operations
    public class SettingsManager
    {
        private AppSettings settings;
        private readonly string settingsFilePath = "settings.json";
        
        public SettingsManager()
        {
            // initialize with default settings
            settings = new AppSettings();
            
            // try to load saved settings
            LoadSettings();
        }
        
        // get all settings
        public AppSettings GetSettings()
        {
            return settings;
        }
        
        // update a setting
        public void UpdateSetting(string key, object value)
        {
            switch (key.ToLower())
            {
                case "defaultcurrency":
                    settings.DefaultCurrency = value.ToString();
                    break;
                case "autosave":
                    settings.AutoSave = Convert.ToBoolean(value);
                    break;
                case "datasavepath":
                    settings.DataSavePath = value.ToString();
                    break;
                default:
                    // custom setting
                    if (settings.CustomSettings.ContainsKey(key))
                    {
                        settings.CustomSettings[key] = value;
                    }
                    else
                    {
                        settings.CustomSettings.Add(key, value);
                    }
                    break;
            }
            
            // save changes
            SaveSettings();
        }
        
        // save settings to file
        public bool SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(settingsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }
        
        // load settings from file
        public bool LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            return false;
        }
        
        // reset settings to defaults
        public void ResetSettings()
        {
            settings = new AppSettings();
            SaveSettings();
        }
    }
}