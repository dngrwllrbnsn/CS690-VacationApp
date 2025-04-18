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

    // data container class to hold all app data
    public class AppData
    {
        public List<Trip> Trips { get; set; } = new List<Trip>();
        public List<Photo> Photos { get; set; } = new List<Photo>();
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        public List<Note> Notes { get; set; } = new List<Note>();
        public List<DailyLogEntry> DailyLogs { get; set; } = new List<DailyLogEntry>();
        public AppSettings Settings { get; set; } = new AppSettings();
    }
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

        // data file path
        private static readonly string dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vacationapp_data.json");
        
        // main method
        static void Main(string[] args)
        {
            Console.Title = "Vacation Journal";
            
            InitializeManagers();
            
            InitializeUI();
            
            LoadData();
            
            // Main application loop
            while (true)
            {
                ShowMainMenu();
                var selectedOption = SelectFromMenu(new string[] {
                    "Vacation Management",
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
                    Console.WriteLine("Thanks for using the Vacation Journal!");
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

        // Load app data from JSON file
        static void LoadData()
        {
            Console.WriteLine("Loading data...");

            try
            {
                if(System.IO.File.Exists(dataFilePath))
                {
                    string json = System.IO.File.ReadAllText(dataFilePath);

                    // If the file is empty
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        // Deserialize all data
                        var appData = JsonSerializer.Deserialize<AppData>(json, options);

                        if(appData != null)
                        {
                            // Place data in each manager
                            tripManager.SetTrips(appData.Trips);
                            photoManager.SetPhotos(appData.Photos);
                            expenseManager.SetExpenses(appData.Expenses);
                            noteManager.SetNotes(appData.Notes);
                            dailyLogManager.SetDailyLogs(appData.DailyLogs);
                            //settingsManager.SetSettings(appData.Settings);

                            Console.WriteLine($"Loaded {appData.Trips.Count} vacations, " +
                                                $"{appData.Photos.Count} photos, " +
                                                $"{appData.Expenses.Count} expenses, " +
                                                $"{appData.Notes.Count} notes, " +
                                                $"{appData.DailyLogs.Count} daily logs");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Data file is empty. Starting with fresh data.");
                    }
                }
                else
                {
                    Console.WriteLine("No existing data file found. Starting with fresh data.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Starting with fresh data.");
            }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
        }

        // Save app data to JSON file
        static void SaveData()
        {
            try
            {
                Console.WriteLine("Saving data...");

                // Create container object with all data
                var appData = new AppData
                {
                    Trips = tripManager.GetAllTrips(),
                    Photos = photoManager.GetAllPhotos(),
                    Expenses = expenseManager.GetAllExpenses(),
                    Notes = noteManager.GetAllNotes(),
                    DailyLogs = dailyLogManager.GetAllDailyLogs(),
                    Settings = settingsManager.GetSettings()
                };

                // Serialize data
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(appData, options);

                // Save to file
                File.WriteAllText(dataFilePath, json);

                Console.WriteLine($"Data saved successfully to: {Path.GetFullPath(dataFilePath)}");
                Console.WriteLine($"  - {appData.Trips.Count} trips");
                Console.WriteLine($"  - {appData.Photos.Count} photos");
                Console.WriteLine($"  - {appData.Expenses.Count} expenses");
                Console.WriteLine($"  - {appData.Notes.Count} notes");
                Console.WriteLine($"  - {appData.DailyLogs.Count} daily logs");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
                
        // Show the main menu
        static void ShowMainMenu()
        {
            Console.Clear();
            DrawHeader("Vacation Journal v1.0");
            
            var activeTrip = tripManager.GetActiveTrip();
            if (activeTrip != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Active Vacation: {activeTrip.Name} ({activeTrip.Destination})");
                Console.ResetColor();
                Console.WriteLine();
            }
            else if (tripManager.GetAllTrips().Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No vacations added yet. Select 'Vacation Management' to create your first vacation!");
                Console.ResetColor();
                Console.WriteLine();
            }
            
            Console.WriteLine("Use ↑/↓ arrow keys to navigate, ENTER to select:");
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
                // Get user selection/key press
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
                    Console.WriteLine("Saving data before exiting...");

                    // Save and show confirmation
                    var tripCount = tripManager.GetAllTrips().Count;
                    var photoCount = photoManager.GetAllPhotos().Count;
                    var expenseCount = expenseManager.GetAllExpenses().Count;
                    var noteCount = noteManager.GetAllNotes().Count;
                    var logCount = dailyLogManager.GetAllDailyLogs().Count;

                    Console.WriteLine($"Saving {tripCount} vacations, {photoCount} photos, {expenseCount} expenses, {noteCount} notes and {logCount} daily logs");
                    
                    SaveData();

                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();                    
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
            Console.WriteLine("You need to create a vacation first before using this feature.");
            Console.ResetColor();
            
            Console.WriteLine("\nWould you like to create a new vacation now? (Y/N): ");
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
            Console.WriteLine("No active vacation selected. Please select an active vacation first.");
            Console.ResetColor();
            
            Console.WriteLine("\nWould you like to select an active vacation now? (Y/N): ");
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
