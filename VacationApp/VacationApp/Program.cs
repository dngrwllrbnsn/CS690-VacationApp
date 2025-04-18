using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using VacationApp.Trips;
using VacationApp.Photos;
using VacationApp.Expenses;
using VacationApp.Notes;
using VacationApp.DailyLog;
using VacationApp.Settings;
using VacationApp.UI;

namespace VacationApp
{
    class Program
    {
        // Manager classes (data/business logic)
        private static TripManager tripManager;
        private static PhotoManager photoManager;
        private static ExpenseManager expenseManager;
        private static NoteManager noteManager;
        private static DailyLogManager dailyLogManager;
        private static SettingsManager settingsManager;
        
        // UI classes
        private static TripUI tripUI;
        private static PhotoServiceUI photosUI;
        private static ExpenseUI expenseUI;
        private static NotesUI notesUI;
        private static DailyLogUI dailyLogUI;
        private static SettingsUI settingsUI;

        private static string GetFullPath(string filename)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        }
        
        // File paths for data storage
        private static string tripsFilePath = GetFullPath("trips.json");
        private static string photosFilePath = GetFullPath("photos.json");
        private static string expensesFilePath = GetFullPath("expenses.json");
        private static string notesFilePath = GetFullPath("notes.json");
        private static string dailyLogsFilePath = GetFullPath("dailylogs.json");
        
        static void Main(string[] args)
        {
            Console.Title = "Vacation Tracker";
            
            // Initialize all managers
            InitializeManagers();
            
            // Initialize UI components
            InitializeUI();
            
            // Load all data
            LoadData();
            
            // Apply settings (like dark mode if enabled)
            ApplySettings();
            
            // Main application loop
            while (true)
            {
                ShowMainMenu();
                var selectedOption = SelectFromMenu(new string[] {
                    "Trip Management",
                    "Photos Management",
                    "Expense Tracking",
                    "Notes",
                    "Daily Log",
                    "Settings",
                    "Exit"
                });
                
                if (ProcessMainMenuSelection(selectedOption) == false)
                {
                    // Save data before exiting
                    SaveData();
                    break;
                }
            }
        }
        
        // Initialize all manager classes
        static void InitializeManagers()
        {
            tripManager = new TripManager();
            photoManager = new PhotoManager();
            expenseManager = new ExpenseManager();
            noteManager = new NoteManager();
            settingsManager = new SettingsManager();
            
            // Daily log manager depends on other managers
            dailyLogManager = new DailyLogManager(photoManager, expenseManager, noteManager);
        }
        
        // Initialize all UI classes
        static void InitializeUI()
        {
            tripUI = new TripUI(tripManager);
            photosUI = new PhotoServiceUI(photoManager);
            expenseUI = new ExpenseUI(expenseManager, tripManager);
            notesUI = new NotesUI(noteManager, tripManager);
            dailyLogUI = new DailyLogUI(dailyLogManager, tripManager);
            settingsUI = new SettingsUI(settingsManager);
        }
        
        // Apply settings to the application
        static void ApplySettings()
        {
            var settings = settingsManager.GetSettings();
            
            // Apply dark mode if enabled
            if (settings.DarkMode)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            
            // Update file paths if custom save path is set
            if (!string.IsNullOrEmpty(settings.DataSavePath))
            {
                string path = settings.DataSavePath;
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch
                    {
                        // If directory creation fails, use default paths
                        Console.WriteLine("Failed to create data directory. Using default paths.");
                        return;
                    }
                }
                
                tripsFilePath = Path.Combine(path, "trips.json");
                photosFilePath = Path.Combine(path, "photos.json");
                expensesFilePath = Path.Combine(path, "expenses.json");
                notesFilePath = Path.Combine(path, "notes.json");
                dailyLogsFilePath = Path.Combine(path, "dailylogs.json");
            }
        }
        
        // Load all application data
        static void LoadData()
        {
            Console.WriteLine("Loading data...");
            Console.WriteLine($"Loading from: {Path.GetFullPath(tripsFilePath)}");

            LoadTrips();
            LoadPhotos();
            LoadExpenses();
            LoadNotes();
            LoadDailyLogs();

            Console.WriteLine($"Loaded {tripManager.GetAllTrips().Count} trips");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        // Save all application data
        static void SaveData()
        {
            SaveTrips();
            SavePhotos();
            SaveExpenses();
            SaveNotes();
            SaveDailyLogs();
        }
        
        // Load trips from JSON file
        static void LoadTrips()
        {
            try
            {
                if (File.Exists(tripsFilePath))
                {
                    string json = File.ReadAllText(tripsFilePath);
                    Console.WriteLine($"Loaded JSON: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var trips = JsonSerializer.Deserialize<List<Trip>>(json);
                    Console.WriteLine($"Deserialized {trips?.Count ?? 0} trips");

                    tripManager.SetTrips(trips);
                }
                else
                {
                    Console.WriteLine($"File not located: {tripsFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trips: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        // Save trips to JSON file
        static void SaveTrips()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(tripManager.GetAllTrips(), options);
                File.WriteAllText(tripsFilePath, json);
                Console.WriteLine($"Vacations saved to: {Path.GetFullPath(tripsFilePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving trips: {ex.Message}");
            }
        }
        
        // Load photos from JSON file
        static void LoadPhotos()
        {
            try
            {
                if (File.Exists(photosFilePath))
                {
                    string json = File.ReadAllText(photosFilePath);
                    var photos = JsonSerializer.Deserialize<List<Photo>>(json);
                    photoManager.SetPhotos(photos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading photos: {ex.Message}");
            }
        }
        
        // Save photos to JSON file
        static void SavePhotos()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(photoManager.GetAllPhotos(), options);
                File.WriteAllText(photosFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving photos: {ex.Message}");
            }
        }
        
        // Load expenses from JSON file
        static void LoadExpenses()
        {
            try
            {
                if (File.Exists(expensesFilePath))
                {
                    string json = File.ReadAllText(expensesFilePath);
                    var expenses = JsonSerializer.Deserialize<List<Expense>>(json);
                    expenseManager.SetExpenses(expenses);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading expenses: {ex.Message}");
            }
        }
        
        // Save expenses to JSON file
        static void SaveExpenses()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(expenseManager.GetAllExpenses(), options);
                File.WriteAllText(expensesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving expenses: {ex.Message}");
            }
        }
        
        // Load notes from JSON file
        static void LoadNotes()
        {
            try
            {
                if (File.Exists(notesFilePath))
                {
                    string json = File.ReadAllText(notesFilePath);
                    var notes = JsonSerializer.Deserialize<List<Note>>(json);
                    noteManager.SetNotes(notes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notes: {ex.Message}");
            }
        }
        
        // Save notes to JSON file
        static void SaveNotes()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(noteManager.GetAllNotes(), options);
                File.WriteAllText(notesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving notes: {ex.Message}");
            }
        }
        
        // Load daily logs from JSON file
        static void LoadDailyLogs()
        {
            try
            {
                if (File.Exists(dailyLogsFilePath))
                {
                    string json = File.ReadAllText(dailyLogsFilePath);
                    var logs = JsonSerializer.Deserialize<List<DailyLogEntry>>(json);
                    dailyLogManager.SetDailyLogs(logs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading daily logs: {ex.Message}");
            }
        }
        
        // Save daily logs to JSON file
        static void SaveDailyLogs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(dailyLogManager.GetAllDailyLogs(), options);
                File.WriteAllText(dailyLogsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving daily logs: {ex.Message}");
            }
        }
        
        // Show the main menu
        static void ShowMainMenu()
        {
            Console.Clear();
            DrawHeader("VACATION TRACKER v1.0");
            
            var activeTrip = tripManager.GetActiveTrip();
            if (activeTrip != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Active Trip: {activeTrip.Name} ({activeTrip.Destination})");
                Console.ResetColor();
                Console.WriteLine();
            }
            else if (tripManager.GetAllTrips().Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No trips added yet. Select 'Trip Management' to create your first trip!");
                Console.ResetColor();
                Console.WriteLine();
            }
            
            Console.WriteLine("Use ↑/↓ arrow keys to navigate, Enter to select:");
            Console.WriteLine();
        }
        
        // Draw a header
        static void DrawHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            Console.ResetColor();
        }
        
        // Show a menu with arrow key navigation
        static int SelectFromMenu(string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;
            int menuStartRow = Console.CursorTop;
            
            // Draw menu initially
            DrawMenuOptions(options, selectedIndex, menuStartRow);
            
            do
            {
                // Get key press
                key = Console.ReadKey(true).Key;
                
                // Handle arrow keys
                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < options.Length - 1) ? selectedIndex + 1 : 0;
                }
                
                // Redraw menu
                DrawMenuOptions(options, selectedIndex, menuStartRow);
                
            } while (key != ConsoleKey.Enter);
            
            // Move cursor past menu for next content
            Console.SetCursorPosition(0, menuStartRow + options.Length);
            
            return selectedIndex;
        }
        
        // Draw menu options
        static void DrawMenuOptions(string[] options, int selectedIndex, int startRow)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(0, startRow + i);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, startRow + i);
                
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($" {i + 1}. {options[i]} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($" {i + 1}. {options[i]} ");
                }
            }
        }
        
        // Process main menu selection
        static bool ProcessMainMenuSelection(int selectedIndex)
        {
            var activeTrip = tripManager.GetActiveTrip();
            
            switch (selectedIndex)
            {
                case 0: // Trip Management
                    tripUI.ShowTripManagement();
                    break;
                case 1: // Photos Management
                    if (tripManager.GetAllTrips().Count == 0)
                    {
                        ShowNoTripsMessage("Photos Management");
                    }
                    else if (activeTrip == null)
                    {
                        ShowNoActiveTripMessage();
                    }
                    else
                    {
                        photosUI.ShowPhotosMainMenu(activeTrip.Id, activeTrip.Name);
                    }
                    break;
                case 2: // Expense Tracking
                    if (tripManager.GetAllTrips().Count == 0)
                    {
                        ShowNoTripsMessage("Expense Tracking");
                    }
                    else if (activeTrip == null)
                    {
                        ShowNoActiveTripMessage();
                    }
                    else
                    {
                        expenseUI.ShowExpenseMenu(activeTrip.Id, activeTrip.Name);
                    }
                    break;
                case 3: // Notes
                    if (tripManager.GetAllTrips().Count == 0)
                    {
                        ShowNoTripsMessage("Notes");
                    }
                    else if (activeTrip == null)
                    {
                        ShowNoActiveTripMessage();
                    }
                    else
                    {
                        notesUI.ShowNotesMenu(activeTrip.Id, activeTrip.Name);
                    }
                    break;
                case 4: // Daily Log
                    if (tripManager.GetAllTrips().Count == 0)
                    {
                        ShowNoTripsMessage("Daily Log");
                    }
                    else if (activeTrip == null)
                    {
                        ShowNoActiveTripMessage();
                    }
                    else
                    {
                        dailyLogUI.ShowDailyLogMenu(activeTrip.Id, activeTrip.Name);
                    }
                    break;
                case 5: // Settings
                    settingsUI.ShowSettingsMenu();
                    break;
                case 6: // Exit
                    Console.Clear();
                    Console.WriteLine("Thanks for using Vacation Tracker!");
                    return false;
            }
            
            return true;
        }
        
        // Show message when no trips available
        static void ShowNoTripsMessage(string featureName)
        {
            Console.Clear();
            DrawHeader(featureName.ToUpper());
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("You need to create a trip first before using this feature.");
            Console.ResetColor();
            
            Console.WriteLine("\nWould you like to create a new trip now? (Y/N): ");
            if (Console.ReadLine()?.ToUpper() == "Y")
            {
                tripUI.ShowTripManagement();
            }
            else
            {
                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
            }
        }
        
        // Show message when no active trip selected
        static void ShowNoActiveTripMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No active trip selected. Please select an active trip first.");
            Console.ResetColor();
            
            Console.WriteLine("\nWould you like to select an active trip now? (Y/N): ");
            if (Console.ReadLine()?.ToUpper() == "Y")
            {
                tripUI.ShowTripManagement();
            }
            else
            {
                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
            }
        }
    }
}


    /*most basic menu option w/in first 5mins of beginning implementation
    
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Your Vacation Journal");        
        Console.WriteLine("");
        Console.WriteLine("Please select from the following options:");
        Console.WriteLine("1. Select/Manage Vacations");
        Console.WriteLine("2. Photos Management");
        Console.WriteLine("3. Expense Tracking");
        Console.WriteLine("4. Notes");
        Console.WriteLine("5. Daily Logs");
        Console.WriteLine("6. Settings");
        Console.WriteLine("7. Exit");
        Console.WriteLine("");
        Console.WriteLine("What would you like to do? (Enter 1-7):");
        String selection = Console.ReadLine();
    }
}*/
