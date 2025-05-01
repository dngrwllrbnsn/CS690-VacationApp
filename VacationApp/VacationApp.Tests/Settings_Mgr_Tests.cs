using System.Text.Json;
using VacationApp.Settings;

//dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
//reportgenerator -reports:"./coverage.cobertura.xml" -targetdir:"cobertura" -reporttypes:html


namespace VacationApp.Tests
{
    public class SettingsManagerTests : IDisposable
        {
            private readonly string testSettingsFile = "settings.json";
            private SettingsManager settingsManager;

            public SettingsManagerTests()
            {
                // Delete any existing test settings file before each test
                if (File.Exists(testSettingsFile))
                {
                    File.Delete(testSettingsFile);
                }
                
                // Create a fresh SettingsManager for each test
                settingsManager = new SettingsManager();
            }

            public void Dispose()
            {
                // Clean up after each test
                if (File.Exists(testSettingsFile))
                {
                    File.Delete(testSettingsFile);
                }
            }

            [Fact]
            public void Constructor_CreatesDefaultSettings()
            {
                var settings = settingsManager.GetSettings();

                Assert.NotNull(settings);
                Assert.Equal("USD", settings.DefaultCurrency);
                Assert.True(settings.AutoSave);
                Assert.Equal("./data/", settings.DataSavePath);
                Assert.NotNull(settings.CustomSettings);
                Assert.Empty(settings.CustomSettings);
            }

            [Fact]
            public void UpdateSetting_DefaultCurrency_ChangesValue()
            {
                string newCurrency = "EUR";

                settingsManager.UpdateSetting("defaultcurrency", newCurrency);
                var settings = settingsManager.GetSettings();

                Assert.Equal(newCurrency, settings.DefaultCurrency);
            }

            [Fact]
            public void UpdateSetting_AutoSave_ChangesValue()
            {
                bool newAutoSave = false;

                settingsManager.UpdateSetting("autosave", newAutoSave);
                var settings = settingsManager.GetSettings();

                Assert.Equal(newAutoSave, settings.AutoSave);
            }

            [Fact]
            public void UpdateSetting_DataSavePath_ChangesValue()
            {
                string newPath = "./customdata/";

                settingsManager.UpdateSetting("datasavepath", newPath);
                var settings = settingsManager.GetSettings();

                Assert.Equal(newPath, settings.DataSavePath);
            }

            [Fact]
            public void UpdateSetting_IsCaseInsensitive()
            {
                string newCurrency = "GBP";

                settingsManager.UpdateSetting("DEFAULTCURRENCY", newCurrency);
                var settings = settingsManager.GetSettings();

                Assert.Equal(newCurrency, settings.DefaultCurrency);
            }

            [Fact]
            public void SaveSettings_CreatesSettingsFile()
            {
                settingsManager.UpdateSetting("defaultcurrency", "JPY");

                bool result = settingsManager.SaveSettings();

                Assert.True(result);
                Assert.True(File.Exists(testSettingsFile));

                // Verify file content
                string json = File.ReadAllText(testSettingsFile);
                var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json);
                Assert.NotNull(loadedSettings);
                Assert.Equal("JPY", loadedSettings.DefaultCurrency);
            }

            [Fact]
            public void LoadSettings_LoadsValuesFromFile()
            {
                var customSettings = new AppSettings
                {
                    DefaultCurrency = "CAD",
                    AutoSave = false,
                    DataSavePath = "./custompath/"
                };
                
                string json = JsonSerializer.Serialize(customSettings);
                File.WriteAllText(testSettingsFile, json);

                bool result = settingsManager.LoadSettings();
                var loadedSettings = settingsManager.GetSettings();

                Assert.True(result);
                Assert.Equal("CAD", loadedSettings.DefaultCurrency);
                Assert.False(loadedSettings.AutoSave);
                Assert.Equal("./custompath/", loadedSettings.DataSavePath);
            }

            [Fact]
            public void LoadSettings_ReturnsFalseWhenFileDoesNotExist()
            {
                if (File.Exists(testSettingsFile))
                {
                    File.Delete(testSettingsFile);
                }

                var newManager = new SettingsManager();
                
                // Make sure the file doesn't exist (deleted in constructor)
                Assert.False(File.Exists(testSettingsFile));

                bool result = newManager.LoadSettings();

                Assert.False(result);
            }

            [Fact]
            public void ResetSettings_RestoresToDefaults()
            {
                settingsManager.UpdateSetting("defaultcurrency", "EUR");
                settingsManager.UpdateSetting("autosave", false);
                settingsManager.UpdateSetting("datasavepath", "./custom/");
                
                // Verify settings were changed
                var modifiedSettings = settingsManager.GetSettings();
                Assert.Equal("EUR", modifiedSettings.DefaultCurrency);
                Assert.False(modifiedSettings.AutoSave);

                settingsManager.ResetSettings();
                var resetSettings = settingsManager.GetSettings();

                Assert.Equal("USD", resetSettings.DefaultCurrency);
                Assert.True(resetSettings.AutoSave);
                Assert.Equal("./data/", resetSettings.DataSavePath);
            }

            [Fact]
            public void UpdateSetting_SavesChanges()
            {
                string newCurrency = "AUD";

                settingsManager.UpdateSetting("defaultcurrency", newCurrency);

                // Create a new settings manager to load from the saved file
                var newManager = new SettingsManager();
                var loadedSettings = newManager.GetSettings();

                Assert.Equal(newCurrency, loadedSettings.DefaultCurrency);
            }
        }
}