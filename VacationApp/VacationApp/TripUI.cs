using System;
using System.Collections.Generic;
using VacationApp.Trips; //refers to TripManager.cs

namespace VacationApp.UI
{
    public class TripUI
    {
        private readonly TripManager tripManager;
        
        public TripUI(TripManager tripManager)
        {
            this.tripManager = tripManager;
        }
        
        //display trip management menu
        public void ShowTripManagement()
        {
            while (true)
            {
                Console.Clear();
                DrawHeader("Vacation Management");
                
                // display active trip (if any)
                var activeTrip = tripManager.GetActiveTrip();
                if (activeTrip != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Active Vacation: {activeTrip.Name} ({activeTrip.Destination}) [{activeTrip.StartDate:MMM d} - {activeTrip.EndDate:MMM d, yyyy}]");
                    Console.ResetColor();
                    Console.WriteLine();
                }

                // display list of all vacations or message if none
                var trips = tripManager.GetAllTrips();
                if (trips.Count > 0)
                {
                    Console.WriteLine("Available vacations:");
                    Console.WriteLine("ID  | Name                  | Destination      | Dates");
                    Console.WriteLine(new string('-', 56));
                    
                    foreach (var trip in trips)
                    {
                        Console.Write($"{trip.Id:D3} | {trip.Name,-20} | {trip.Destination,-16} | {trip.StartDate:MMM d} - {trip.EndDate:MMM d, yyyy}");
                        if (trip.IsActive)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" [Active]"); //designates which vacation is currently active in the app
                            Console.ResetColor();
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No vacations located. Create your first vacation to get started!");
                    Console.ResetColor();
                }
                Console.WriteLine();

                string[] options = {
                    "Create New Vacation",
                    "Select Active Vacation",
                    "Edit Vacation",
                    "Delete Vacation",
                    "Back to Main Menu"
                };
                
                Console.WriteLine("Use ↑/↓ arrow keys to navigate, ENTER to select:");
                Console.WriteLine();
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // create new vacation
                        CreateNewTrip();
                        break;
                    case 1: // select active vacation
                        if (trips.Count == 0)
                        {
                            ShowNoTripsMessage();
                        }
                        else
                        {
                            SelectActiveTrip();
                        }
                        break;
                    case 2: // edit vacation info
                        if (trips.Count == 0)
                        {
                            ShowNoTripsMessage();
                        }
                        else
                        {
                            EditTrip();
                        }
                        break;
                    case 3: // delete vacation
                        if (trips.Count == 0)
                        {
                            ShowNoTripsMessage();
                        }
                        else
                        {
                            DeleteTrip();
                        }
                        break;
                    case 4: // back to main menu
                        return;
                }
            }
        }
        
        // create a new vacation
        private void CreateNewTrip()
        {
            Console.Clear();
            DrawHeader("Create New Vacation");
            
            var trips = tripManager.GetAllTrips();
            if (trips.Count == 0)
            {
                Console.WriteLine("Let's create your first vacation!\n");
            }
            
            Console.Write("Vacation Name (e.g., 'Spring Break 2025'): ");
            string name = Console.ReadLine();
            
            Console.Write("Destination (e.g., 'Paris, France'): ");
            string destination = Console.ReadLine();
            
            Console.Write("Start Date [YYYY-MM-DD]: ");
            DateTime startDate;
            while (!DateTime.TryParse(Console.ReadLine(), out startDate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid date format. Please use YYYY-MM-DD");
                Console.ResetColor();
                Console.Write("Start Date [YYYY-MM-DD]: ");
            }
            
            Console.Write("End Date [YYYY-MM-DD]: ");
            DateTime endDate;
            while (!DateTime.TryParse(Console.ReadLine(), out endDate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid date format. Please use YYYY-MM-DD");
                Console.ResetColor();
                Console.Write("End Date [YYYY-MM-DD]: ");
            }
            
            // create the vacation
            var newTrip = tripManager.CreateTrip(name, destination, startDate, endDate);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nVacation created successfully!");
            Console.ResetColor();
            
            // for 1st vacation, auto-set it as active
            if (trips.Count == 0)
            {
                tripManager.SetActiveTrip(newTrip.Id);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Vacation \"{newTrip.Name}\" is now active.");
                Console.ResetColor();
            }
            else
            {
                Console.Write("Set as active vacation? [Y/N]: ");
                if (Console.ReadLine()?.ToUpper() == "Y")
                {
                    tripManager.SetActiveTrip(newTrip.Id);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nTrip \"{newTrip.Name}\" is now active.");
                    Console.ResetColor();
                }
            }
            
            Console.WriteLine("\nPress any key to return to Vacation Management...");
            Console.ReadKey();
        }
        
        // designate a vacation as active
        private void SelectActiveTrip()
        {
            Console.Clear();
            DrawHeader("Select Active Vacation");
            
            Console.Write("Enter Vacation ID: ");
            if (int.TryParse(Console.ReadLine(), out int tripId))
            {
                var selectedTrip = tripManager.GetTrip(tripId);
                if (selectedTrip != null)
                {
                    tripManager.SetActiveTrip(tripId);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nVacation \"{selectedTrip.Name}\" is now active.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nVacation not found!");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid vacation ID!");
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPress any key to return to Vacation Management...");
            Console.ReadKey();
        }
        
        // edit an existing vacation
        private void EditTrip()
        {
            Console.Clear();
            DrawHeader("Edit Vacation");
            
            Console.WriteLine("Available Vacations:");
            Console.WriteLine("ID  | Name                  | Destination");
            Console.WriteLine(new string('-', 45));
            
            foreach (var trip in tripManager.GetAllTrips())
            {
                Console.WriteLine($"{trip.Id:D3} | {trip.Name,-20} | {trip.Destination}");
            }
            
            Console.Write("\nEnter Vacation ID to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int tripId))
            {
                Console.WriteLine("Invalid ID format.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            var selectedTrip = tripManager.GetTrip(tripId);
            if (selectedTrip == null)
            {
                Console.WriteLine("Vacation not found.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            string[] editOptions = {
                "Edit Name",
                "Edit Destination",
                "Edit Start Date",
                "Edit End Date",
                "Back"
            };
            
            while (true)
            {
                Console.Clear();
                DrawHeader($"Editing Vacation: {selectedTrip.Name}");
                
                Console.WriteLine("Current Vacation Details:");
                Console.WriteLine($"Name: {selectedTrip.Name}");
                Console.WriteLine($"Destination: {selectedTrip.Destination}");
                Console.WriteLine($"Start Date: {selectedTrip.StartDate:yyyy-MM-dd}");
                Console.WriteLine($"End Date: {selectedTrip.EndDate:yyyy-MM-dd}");
                Console.WriteLine("\nWhat would you like to edit?");
                Console.WriteLine("Use ↑/↓ arrow keys to navigate, Enter to select:\n");
                
                int selectedOption = ShowMenu(editOptions);
                
                if (selectedOption == 4) // back option
                {
                    return;
                }
                
                switch (selectedOption)
                {
                    case 0: // edit vacation name
                        Console.WriteLine($"\nCurrent Name: {selectedTrip.Name}");
                        Console.Write("Enter new Name: ");
                        string oldName = selectedTrip.Name;
                        string newName = Console.ReadLine();
                        
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            selectedTrip.Name = newName;
                            tripManager.UpdateTrip(selectedTrip.Id, selectedTrip.Name, selectedTrip.Destination, selectedTrip.StartDate, selectedTrip.EndDate);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n\"{oldName}\" was replaced with \"{selectedTrip.Name}\" and saved to Vacation ID: {selectedTrip.Id}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("\nNo changes made - name cannot be empty.");
                        }
                        break;
                        
                    case 1: // edit vacation destination
                        Console.WriteLine($"\nCurrent Destination: {selectedTrip.Destination}");
                        Console.Write("Enter new Destination: ");
                        string oldDestination = selectedTrip.Destination;
                        string newDestination = Console.ReadLine();
                        
                        if (!string.IsNullOrWhiteSpace(newDestination))
                        {
                            selectedTrip.Destination = newDestination;
                            tripManager.UpdateTrip(selectedTrip.Id, selectedTrip.Name, selectedTrip.Destination, selectedTrip.StartDate, selectedTrip.EndDate);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n\"{oldDestination}\" was replaced with \"{selectedTrip.Destination}\" and saved to Vacation: {selectedTrip.Name}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("\nNo changes made - destination cannot be empty.");
                        }
                        break;
                        
                    case 2: // edit start date
                        Console.WriteLine($"\nCurrent Start Date: {selectedTrip.StartDate:yyyy-MM-dd}");
                        Console.Write("Enter new Start Date [YYYY-MM-DD]: ");
                        string oldStartDate = selectedTrip.StartDate.ToString("yyyy-MM-dd");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime newStartDate))
                        {
                            selectedTrip.StartDate = newStartDate;
                            tripManager.UpdateTrip(selectedTrip.Id, selectedTrip.Name, selectedTrip.Destination, selectedTrip.StartDate, selectedTrip.EndDate);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n\"{oldStartDate}\" was replaced with \"{selectedTrip.StartDate:yyyy-MM-dd}\" and saved to Vacation: {selectedTrip.Name}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid date format. No changes made.");
                            Console.ResetColor();
                        }
                        break;
                        
                    case 3: // edit end date
                        Console.WriteLine($"\nCurrent End Date: {selectedTrip.EndDate:yyyy-MM-dd}");
                        Console.Write("Enter new End Date [YYYY-MM-DD]: ");
                        string oldEndDate = selectedTrip.EndDate.ToString("yyyy-MM-dd");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime newEndDate))
                        {
                            selectedTrip.EndDate = newEndDate;
                            tripManager.UpdateTrip(selectedTrip.Id, selectedTrip.Name, selectedTrip.Destination, selectedTrip.StartDate, selectedTrip.EndDate);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n\"{oldEndDate}\" was replaced with \"{selectedTrip.EndDate:yyyy-MM-dd}\" and saved to Vacation: {selectedTrip.Name}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid date format. No changes made.");
                            Console.ResetColor();
                        }
                        break;
                }
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
        
        // delete a vacation from the app
        private void DeleteTrip()
        {
            Console.Clear();
            DrawHeader("Delete Vacation");
            
            Console.WriteLine("Available Vacations:");
            Console.WriteLine("ID  | Name                  | Destination");
            Console.WriteLine(new string('-', 45));
            
            foreach (var trip in tripManager.GetAllTrips())
            {
                Console.WriteLine($"{trip.Id:D3} | {trip.Name,-20} | {trip.Destination}");
            }
            
            Console.Write("\nEnter Vacation ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int tripId))
            {
                Console.WriteLine("Invalid ID format.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            var selectedTrip = tripManager.GetTrip(tripId);
            if (selectedTrip == null)
            {
                Console.WriteLine("Vacation not lcoated.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"\nAre you sure you want to delete vacation: \"{selectedTrip.Name}\"? (Y/N): ");
            if (Console.ReadLine()?.ToUpper() == "Y")
            {
                if (tripManager.DeleteTrip(tripId))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nVacation deleted successfully!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nFailed to delete vacation.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("\nDelete operation cancelled.");
            }
            
            Console.WriteLine("\nPress any key to return to Vacation Management...");
            Console.ReadKey();
        }
        
        // show message when no vacations are available yet
        private void ShowNoTripsMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No vacations available. Please create a vacation first.");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
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
                // display all menu options
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
                
                // get user selection/key press
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
