using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VacationApp.Photos;


namespace VacationApp.UI
{
    public class PhotoServiceUI
    {
        private readonly PhotoManager photoManager;

        public PhotoServiceUI(PhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        //main entry to the photo menu
        public void ShowPhotosMainMenu(int tripId, string tripName)
        {
            while(true)
            {
                Console.Clear();
                DrawHeader($"Photos: {tripName}");
                
                var photos = photoManager.GetPhotosForTrip(tripId);
                Console.WriteLine($"Total Photos: {photos.Count}");
                Console.WriteLine();

                string[] options = {
                    "View Photos",
                    "Add Photo",
                    "Search Photos",
                    "Back to Main Menu"
                };

                int selectedOption = ShowPhotosMenu(options);

                switch(selectedOption)
                {
                    case 0: //view photos
                        ViewPhotos(tripId, tripName);
                        break;
                    case 1: //add photo
                        AddPhoto(tripId);
                        break;
                    case 2: //search photos
                        SearchPhotos(tripId, tripName);
                        break;
                    case 3: //back to main menu
                        return;
                }
            }
        }

        private void ViewPhotos(int tripId, string tripName)
        {
            var photos = photoManager.GetPhotosForTrip(tripId);

            if(photos.Count == 0)
            {
                Console.Clear();
                DrawHeader($"Photos: {tripName}");
                Console.WriteLine("No photos found for this trip.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                Console.Clear();
                DrawHeader($"Photos: {tripName}");
                
                Console.WriteLine("ID  | Date       | Location               | Tags");
                Console.WriteLine(new string('-', 60));

                foreach(var photo in photos)
                {
                    string tags = string.Join(", ", photo.Tags);
                    if (tags.Length > 20) tags = tags.Substring(0,17)+"..."; //preview tags

                    Console.WriteLine($"{photo.Id:D3} | {photo.CaptureDate: MM-dd-yyyy} | " +
                                        $"{(photo.Location ?? "Unknown"), -15} | {tags}");
                }

                Console.WriteLine("\nEnter photo Id to view details (or 0 to go back): ");
                string input = Console.ReadLine();

                if (input == "0" || string.IsNullOrEmpty(input))
                {
                    return;
                }

                if (int.TryParse(input, out int photoId))
                {
                    var photo = photoManager.GetPhoto(photoId);
                    if (photo != null && photo.TripId == tripId)
                    {
                        ViewPhotoDetails(photo);
                    }
                    else
                    {
                        Console.WriteLine("Photo not found. Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid ID. Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }
        private void ViewPhotoDetails(Photo photo)
        {
            while (true)
            {
                Console.Clear();
                DrawHeader($"Photo Details: ID {photo.Id}");
                
                Console.WriteLine($"File: {photo.FilePath}");
                Console.WriteLine($"Date: {photo.CaptureDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Location: {photo.Location ?? "Unknown"}");
                Console.WriteLine($"Tags: {string.Join(", ", photo.Tags)}");
                Console.WriteLine();
                Console.WriteLine("Notes:");
                Console.WriteLine(photo.Notes ?? "No notes");
                Console.WriteLine();
                
                string[] options = {
                    "Add/Edit Tags",
                    "Add/Edit Notes",
                    "Update Location",
                    "Delete Photo",
                    "Back to Photos List"
                };
                
                int selectedOption = ShowPhotosMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // Add/Edit Tags
                        EditTags(photo);
                        break;
                    case 1: // Add/Edit Notes
                        EditNotes(photo);
                        break;
                    case 2: // Update Location
                        EditLocation(photo);
                        break;
                    case 3: // Delete Photo
                        if (DeletePhoto(photo))
                        {
                            return; // Go back to photos list
                        }
                        break;
                    case 4: // Back
                        return;
                }
            }
        }
        
        private void EditTags(Photo photo)
        {
            Console.Clear();
            DrawHeader("Edit Tags");
            
            Console.WriteLine("Current Tags: " + string.Join(", ", photo.Tags));
            Console.WriteLine("\nEnter new tags (comma separated) or press ENTER to keep current tags:");
            string input = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                photo.Tags.Clear();
                foreach (string tag in input.Split(','))
                {
                    string trimmedTag = tag.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTag) && !photo.Tags.Contains(trimmedTag))
                    {
                        photo.Tags.Add(trimmedTag);
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTags updated successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made to tags.");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        
        private void EditNotes(Photo photo)
        {
            Console.Clear();
            DrawHeader("Edit Notes");
            
            Console.WriteLine("Current Notes:");
            Console.WriteLine(photo.Notes ?? "No notes");
            
            Console.WriteLine("\nEnter new notes (press ENTER two times if you are finished):");
            string input = "";
            string line;
            
            while (true)
            {
                line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                input += line + Environment.NewLine;
            }
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                photoManager.UpdateNotes(photo.Id, input.Trim());
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nNotes updated successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made to notes.");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        
        private void EditLocation(Photo photo)
        {
            Console.Clear();
            DrawHeader("Update Location");
            
            Console.WriteLine($"Current Location: {photo.Location ?? "Unknown"}");
            Console.WriteLine("\nEnter new location:");
            string input = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                photoManager.UpdateLocation(photo.Id, input.Trim());
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nLocation updated successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made to location.");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        
        private bool DeletePhoto(Photo photo)
        {
            Console.Clear();
            DrawHeader("Delete Photo");
            
            Console.WriteLine($"Are you sure you want to delete photo ID {photo.Id}?");
            Console.WriteLine($"File: {photo.FilePath}");
            Console.WriteLine($"Date: {photo.CaptureDate:yyyy-MM-dd}");
            
            Console.Write("\nType 'DELETE' to confirm or press ENTER to cancel: ");
            string input = Console.ReadLine();
            
            if (input == "DELETE")
            {
                if (photoManager.DeletePhoto(photo.Id))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nPhoto deleted successfully!");
                    Console.ResetColor();
                    
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nFailed to delete photo.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("\nDelete operation cancelled.");
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            return false;
        }
        
        private void AddPhoto(int tripId)
        {
            Console.Clear();
            DrawHeader("Add Photo");
            
            Console.WriteLine("Enter your photo's file path (e.g., C:\\Vacation\\photo.jpg):");
            string filePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Operation cancelled. Press any key to continue...");
                Console.ReadKey();
                return;
            }

            string validPath;
            bool fileFound = FileHelper.TryFindFile(filePath, out validPath);

            //show message if filepath needed help
            if (fileFound && validPath != filePath)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nFile found at: {validPath}");
                Console.ResetColor();
                filePath = validPath; //use fixed filepath
            }

            //if file wasn't found
            else if(!fileFound)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nFile does not exist at the specified path.");
                Console.ResetColor();
                Console.WriteLine("Operation cancelled. Press any key to continue...");
                Console.ReadKey();
                return;
            }
                      
            // add the photo
            var photo = photoManager.AddPhoto(tripId, filePath);
            
            // prompt user for photo info
            Console.Write("\nEnter location (or press ENTER to skip): ");
            string location = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(location))
            {
                photoManager.UpdateLocation(photo.Id, location.Trim());
            }
            
            Console.Write("\nEnter tags (comma separated, or press ENTER to skip): ");
            string tags = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(tags))
            {
                foreach (string tag in tags.Split(','))
                {
                    string trimmedTag = tag.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTag))
                    {
                        photoManager.AddTag(photo.Id, trimmedTag);
                    }
                }
            }
            
            Console.Write("\nEnter notes (press ENTER to skip): ");
            string notes = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(notes))
            {
                photoManager.UpdateNotes(photo.Id, notes.Trim());
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPhoto added successfully!");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        
        private void SearchPhotos(int tripId, string tripName)
        {
            Console.Clear();
            DrawHeader($"Search photos: {tripName}");
            
            string[] options = {
                "Search by Tag",
                "Search by Location",
                "Search by Date Range",
                "Search in Notes",
                "Back"
            };
            
            int selectedOption = ShowPhotosMenu(options);
            
            List<Photo> results = null;
            
            switch (selectedOption)
            {
                case 0: // Search by Tag
                    Console.Write("\nEnter tag to search for: ");
                    string tag = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        results = photoManager.SearchByTag(tripId, tag.Trim());
                    }
                    break;
                    
                case 1: // Search by Location
                    Console.Write("\nEnter location to search for: ");
                    string location = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        results = photoManager.SearchByLocation(tripId, location.Trim());
                    }
                    break;
                    
                case 2: // Search by Date Range
                    Console.Write("\nEnter start date [YYYY-MM-DD]: ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
                    {
                        Console.Write("Enter end date [YYYY-MM-DD]: ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
                        {
                            results = photoManager.SearchByDateRange(tripId, startDate, endDate);
                        }
                    }
                    break;
                    
                case 3: // Search in Notes
                    Console.Write("\nEnter text to search for in notes: ");
                    string searchText = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        results = photoManager.SearchByNotes(tripId, searchText.Trim());
                    }
                    break;
                    
                case 4: // Back
                    return;
            }
            
            // Display search results
            if (results != null)
            {
                DisplaySearchResults(results, tripName);
            }
            else
            {
                Console.WriteLine("\nSearch cancelled. Press any key to continue...");
                Console.ReadKey();
            }
        }
        
        private void DisplaySearchResults(List<Photo> results, string tripName)
        {
            Console.Clear();
            DrawHeader($"Search results: {tripName}");
            
            if (results.Count == 0)
            {
                Console.WriteLine("No photos found matching your search criteria.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"Found {results.Count} photos:");
            Console.WriteLine();
            Console.WriteLine("ID  | Date       | Location        | Tags");
            Console.WriteLine(new string('-', 60));
            
            foreach (var photo in results)
            {
                string tags = string.Join(", ", photo.Tags);
                if (tags.Length > 20) tags = tags.Substring(0, 17) + "...";
                
                Console.WriteLine($"{photo.Id:D3} | {photo.CaptureDate:yyyy-MM-dd} | " +
                                $"{(photo.Location ?? "Unknown"),-15} | {tags}");
            }
            
            Console.WriteLine("\nEnter photo ID to view details (or 0 to go back): ");
            string input = Console.ReadLine();
            
            if (input != "0" && !string.IsNullOrEmpty(input) && int.TryParse(input, out int photoId))
            {
                var photo = photoManager.GetPhoto(photoId);
                if (photo != null && results.Any(p => p.Id == photoId))
                {
                    ViewPhotoDetails(photo);
                }
                else
                {
                    Console.WriteLine("Photo not found in search results. Press any key to continue...");
                    Console.ReadKey();
                }
            }
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
        private int ShowPhotosMenu(string[] options)
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

