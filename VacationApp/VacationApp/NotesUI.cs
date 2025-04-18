using System;
using System.Collections.Generic;
using VacationApp.Notes;
using VacationApp.Trips;



namespace VacationApp.UI
{
    public class NotesUI
    {
        private readonly NoteManager noteManager;
        private readonly TripManager tripManager;
        
        public NotesUI(NoteManager noteManager, TripManager tripManager)
        {
            this.noteManager = noteManager;
            this.tripManager = tripManager;
        }
        
        // show the notes main menu
        public void ShowNotesMenu(int tripId, string tripName)
        {
            while (true)
            {
                Console.Clear();
                DrawHeader($"NOTES: {tripName}");
                
                // show total note count
                var notes = noteManager.GetNotesForTrip(tripId);
                Console.WriteLine($"Total Notes: {notes.Count}");
                Console.WriteLine();
                
                string[] options = {
                    "View All Notes",
                    "Create New Note",
                    "Search Notes",
                    "Back to Main Menu"
                };
                
                Console.WriteLine("Use ↑/↓ arrow keys to navigate, ENTER to select:");
                Console.WriteLine();
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // view all notes
                        ViewAllNotes(tripId, tripName);
                        break;
                    case 1: // create new note
                        CreateNewNote(tripId);
                        break;
                    case 2: // search notes
                        SearchNotes(tripId, tripName);
                        break;
                    case 3: // back to main menu
                        return;
                }
            }
        }
        
        // view all notes for a vacation
        private void ViewAllNotes(int tripId, string tripName)
        {
            var notes = noteManager.GetNotesForTrip(tripId);
            
            if (notes.Count == 0)
            {
                Console.Clear();
                DrawHeader($"Notes: {tripName}");
                Console.WriteLine("No notes found for this vacation.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            while (true)
            {
                Console.Clear();
                DrawHeader($"Notes: {tripName}");
                
                Console.WriteLine("ID  | Date       | Title                 | Tags");
                Console.WriteLine(new string('-', 60));
                
                foreach (var note in notes)
                {
                    string tags = note.Tags.Count > 0 ? string.Join(", ", note.Tags) : "";
                    if (tags.Length > 20) tags = tags.Substring(0, 17) + "...";
                    
                    Console.WriteLine($"{note.Id:D3} | {note.CreatedDate:yyyy-MM-dd} | " +
                                    $"{note.Title,-20} | {tags}");
                }
                
                Console.WriteLine("\nEnter a note ID to view its details (or 0 to go back): ");
                string input = Console.ReadLine();
                
                if (input == "0" || string.IsNullOrEmpty(input))
                {
                    return;
                }
                
                if (int.TryParse(input, out int noteId))
                {
                    var note = noteManager.GetNote(noteId);
                    if (note != null && note.TripId == tripId)
                    {
                        ViewNote(note);
                    }
                    else
                    {
                        Console.WriteLine("Note not located. Press any key to continue...");
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
        
        // view a specific note
        private void ViewNote(Note note)
        {
            while (true)
            {
                Console.Clear();
                DrawHeader($"Note: {note.Title}");
                
                Console.WriteLine($"Created: {note.CreatedDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Tags: {string.Join(", ", note.Tags)}");
                Console.WriteLine();
                Console.WriteLine("Content:");
                Console.WriteLine(note.Content);
                Console.WriteLine();
                
                string[] options = {
                    "Edit Note",
                    "Delete Note",
                    "Back to Notes List"
                };
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // edit note
                        EditNote(note);
                        break;
                    case 1: // delete note
                        if (DeleteNote(note))
                        {
                            return; // go back to notes list
                        }
                        break;
                    case 2: // back
                        return;
                }
            }
        }
        
        // edit a note
        private void EditNote(Note note)
        {
            Console.Clear();
            DrawHeader("Edit Note");
            
            Console.WriteLine($"Current Title: {note.Title}");
            Console.Write("Enter new Title (or press ENTER to keep current): ");
            string titleInput = Console.ReadLine();
            string title = string.IsNullOrWhiteSpace(titleInput) ? note.Title : titleInput;
            
            Console.WriteLine($"\nCurrent Tags: {string.Join(", ", note.Tags)}");
            Console.Write("Enter new Tags (comma separated, or press ENTER to keep current): ");
            string tagsInput = Console.ReadLine();
            List<string> tags = note.Tags;
            
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                tags = new List<string>();
                foreach (string tag in tagsInput.Split(','))
                {
                    string trimmedTag = tag.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTag) && !tags.Contains(trimmedTag))
                    {
                        tags.Add(trimmedTag);
                    }
                }
            }
            
            Console.WriteLine("\nCurrent Content:");
            Console.WriteLine(note.Content);
            Console.WriteLine("\nEnter new Content (or press ENTER to keep current):");
            Console.WriteLine("Type END on a separate line when finished.");
            
            string contentInput = "";
            string line;
            while (true)
            {
                line = Console.ReadLine();
                if (line == "END") break;
                contentInput += line + Environment.NewLine;
            }
            
            string content = string.IsNullOrWhiteSpace(contentInput) ? note.Content : contentInput.Trim();
            
            // update the note
            noteManager.UpdateNote(note.Id, title, content, tags);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nNote updated successfully!");
            Console.ResetColor();
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // delete a note
        private bool DeleteNote(Note note)
        {
            Console.Clear();
            DrawHeader("Delete Note");
            
            Console.WriteLine($"Are you sure you want to delete the note \"{note.Title}\"? (Y/N): ");
            if (Console.ReadLine()?.ToUpper() == "Y")
            {
                if (noteManager.DeleteNote(note.Id))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nNote deleted successfully!");
                    Console.ResetColor();
                    
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nFailed to delete note.");
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
        
        // create a new note
        private void CreateNewNote(int tripId)
        {
            Console.Clear();
            DrawHeader("Create New Note");
            
            Console.Write("Title: ");
            string title = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("\nTitle cannot be empty. Operation cancelled.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            Console.Write("\nTags (comma separated, or press ENTER to skip): ");
            string tagsInput = Console.ReadLine();
            List<string> tags = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                foreach (string tag in tagsInput.Split(','))
                {
                    string trimmedTag = tag.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedTag) && !tags.Contains(trimmedTag))
                    {
                        tags.Add(trimmedTag);
                    }
                }
            }
            
            Console.WriteLine("\nContent (type END on a separate line when finished):");
            string content = "";
            string line;
            while (true)
            {
                line = Console.ReadLine();
                if (line == "END") break;
                content += line + Environment.NewLine;
            }
            
            // create the note
            var note = noteManager.AddNote(tripId, title, content.Trim(), tags);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nNote created successfully!");
            Console.ResetColor();
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // search notes
        private void SearchNotes(int tripId, string tripName)
        {
            Console.Clear();
            DrawHeader($"Search Notes: {tripName}");
            
            string[] options = {
                "Search by Text",
                "Search by Tag",
                "Back"
            };
            
            int selectedOption = ShowMenu(options);
            
            List<Note> results = null;
            
            switch (selectedOption)
            {
                case 0: // search by Text
                    Console.Write("\nEnter search text: ");
                    string searchText = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        results = noteManager.SearchNotes(tripId, searchText);
                    }
                    break;
                    
                case 1: // search by Tag
                    Console.Write("\nEnter tag to search for: ");
                    string tag = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        results = noteManager.SearchNotesByTag(tripId, tag.Trim());
                    }
                    break;
                    
                case 2: // back
                    return;
            }
            
            // display search results
            if (results != null)
            {
                DisplaySearchResults(results, tripName);
            }
        }
        
        // method to display search results
        private void DisplaySearchResults(List<Note> results, string tripName)
        {
            Console.Clear();
            DrawHeader($"Search results: {tripName}");
            
            if (results.Count == 0)
            {
                Console.WriteLine("There are no notes matching your search criteria.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"Found {results.Count} notes:");
            Console.WriteLine();
            Console.WriteLine("ID  | Date       | Title                 | Tags");
            Console.WriteLine(new string('-', 60));
            
            foreach (var note in results)
            {
                string tags = note.Tags.Count > 0 ? string.Join(", ", note.Tags) : "";
                if (tags.Length > 20) tags = tags.Substring(0, 17) + "...";
                
                Console.WriteLine($"{note.Id:D3} | {note.CreatedDate:yyyy-MM-dd} | " +
                                $"{note.Title,-20} | {tags}");
            }
            
            Console.WriteLine("\nEnter note ID to view details (or 0 to go back): ");
            string input = Console.ReadLine();
            
            if (input != "0" && !string.IsNullOrEmpty(input) && int.TryParse(input, out int noteId))
            {
                var note = noteManager.GetNote(noteId);
                if (note != null && results.Exists(n => n.Id == noteId))
                {
                    ViewNote(note);
                }
                else
                {
                    Console.WriteLine("Note not found in search results. Press any key to continue...");
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
