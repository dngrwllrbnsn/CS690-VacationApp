using System;
using System.Collections.Generic;
using System.Linq; //for querying capabilities


namespace VacationApp.Notes
{
// class to store notes info
    public class Note
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
    
    // class to handle note operations
    public class NoteManager
    {
        private List<Note> notes = new List<Note>();
        private int nextId = 1;
        
        // add a new note
        public Note AddNote(int tripId, string title, string content, List<string> tags = null)
        {
            var note = new Note
            {
                Id = nextId++,
                TripId = tripId,
                Title = title,
                Content = content,
                CreatedDate = DateTime.Now,
                Tags = tags ?? new List<string>()
            };
            
            notes.Add(note);
            return note;
        }
        
        // get all notes for a specific vacation
        public List<Note> GetNotesForTrip(int tripId)
        {
            return notes.FindAll(n => n.TripId == tripId);
        }
        
        // get a specific note by ID
        public Note GetNote(int noteId)
        {
            return notes.Find(n => n.Id == noteId);
        }
        
        // update a note
        public bool UpdateNote(int noteId, string title, string content, List<string> tags)
        {
            var note = GetNote(noteId);
            if (note != null)
            {
                note.Title = title;
                note.Content = content;
                note.Tags = tags;
                return true;
            }
            return false;
        }
        
        // delete a note
        public bool DeleteNote(int noteId)
        {
            var note = GetNote(noteId);
            if (note != null)
            {
                return notes.Remove(note);
            }
            return false;
        }
        
        // search notes by text
        public List<Note> SearchNotes(int tripId, string searchText)
        {
            searchText = searchText.ToLower();
            return notes.FindAll(n => n.TripId == tripId && 
                               (n.Title.ToLower().Contains(searchText) || 
                                n.Content.ToLower().Contains(searchText)));
        }
        
        // search notes by tag
        public List<Note> SearchNotesByTag(int tripId, string tag)
        {
            return notes.FindAll(n => n.TripId == tripId && n.Tags.Contains(tag));
        }
        
        // for serialization; get all notes
        public List<Note> GetAllNotes()
        {
            return notes;
        }
        
        // for serialization; load notes from storage
        public void SetNotes(List<Note> loadedNotes)
        {
            if (loadedNotes != null)
            {
                notes = loadedNotes;
                
                // Identify the highest ID in order to correctly set the next ID
                nextId = 1;
                foreach (var note in notes)
                {
                    if (note.Id >= nextId)
                    {
                        nextId = note.Id + 1;
                    }
                }
            }
        }
    }
}