using VacationApp.Notes;

//dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
//reportgenerator -reports:"./coverage.cobertura.xml" -targetdir:"cobertura" -reporttypes:html


namespace VacationApp.Tests
{
    public class NoteManagerTests
        {
            private readonly NoteManager noteManager;
            private readonly int tripId = 1;

            public NoteManagerTests()
            {
                noteManager = new NoteManager();
            }

            [Fact]
            public void AddNote_AddsNewNote()
            {
                string title = "Test Note";
                string content = "This is a test note content";
                var tags = new List<string> { "test", "important" };

                var note = noteManager.AddNote(tripId, title, content, tags);
                var notes = noteManager.GetNotesForTrip(tripId);

                Assert.Single(notes);
                Assert.Equal(1, note.Id);
                Assert.Equal(tripId, note.TripId);
                Assert.Equal(title, note.Title);
                Assert.Equal(content, note.Content);
                Assert.Equal(tags, note.Tags);
                Assert.True((DateTime.Now - note.CreatedDate).TotalSeconds < 5); // Created within the last 5 seconds
            }

            [Fact]
            public void AddNote_WithNullTags_CreatesEmptyTagsList()
            {
                string title = "Test Note";
                string content = "This is a test note content";

                var note = noteManager.AddNote(tripId, title, content);

                Assert.NotNull(note.Tags);
                Assert.Empty(note.Tags);
            }

            [Fact]
            public void GetNotesForTrip_ReturnsOnlyTripNotes()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                noteManager.AddNote(trip1, "Note 1 for Trip 1", "Content 1");
                noteManager.AddNote(trip1, "Note 2 for Trip 1", "Content 2");
                noteManager.AddNote(trip2, "Note 1 for Trip 2", "Content 3");

                var trip1Notes = noteManager.GetNotesForTrip(trip1);
                var trip2Notes = noteManager.GetNotesForTrip(trip2);

                Assert.Equal(2, trip1Notes.Count);
                Assert.Single(trip2Notes);
                Assert.All(trip1Notes, n => Assert.Equal(trip1, n.TripId));
                Assert.All(trip2Notes, n => Assert.Equal(trip2, n.TripId));
            }

            [Fact]
            public void GetNote_ReturnsCorrectNote()
            {
                var note = noteManager.AddNote(tripId, "Test Note", "Content");
                int noteId = note.Id;

                var retrievedNote = noteManager.GetNote(noteId);

                Assert.NotNull(retrievedNote);
                Assert.Equal(noteId, retrievedNote.Id);
                Assert.Equal("Test Note", retrievedNote.Title);
            }

            [Fact]
            public void GetNote_ReturnsNullForNonExistentId()
            {
                int nonExistentId = 999;

                var result = noteManager.GetNote(nonExistentId);

                Assert.Null(result);
            }

            [Fact]
            public void UpdateNote_UpdatesExistingNote()
            {
                var note = noteManager.AddNote(tripId, "Original Title", "Original Content");
                int noteId = note.Id;
                
                string newTitle = "Updated Title";
                string newContent = "Updated Content";
                var newTags = new List<string> { "updated", "important" };

                bool result = noteManager.UpdateNote(noteId, newTitle, newContent, newTags);
                var updatedNote = noteManager.GetNote(noteId);

                Assert.True(result);
                Assert.Equal(newTitle, updatedNote.Title);
                Assert.Equal(newContent, updatedNote.Content);
                Assert.Equal(newTags, updatedNote.Tags);
                Assert.Equal(note.CreatedDate, updatedNote.CreatedDate); // CreatedDate should not change
            }

            [Fact]
            public void UpdateNote_ReturnsFalseForNonExistentId()
            {
                int nonExistentId = 999;

                bool result = noteManager.UpdateNote(nonExistentId, "Title", "Content", new List<string>());

                Assert.False(result);
            }

            [Fact]
            public void DeleteNote_RemovesNote()
            {
                var note = noteManager.AddNote(tripId, "Test Note", "Content");
                int noteId = note.Id;

                bool result = noteManager.DeleteNote(noteId);
                var notes = noteManager.GetNotesForTrip(tripId);

                Assert.True(result);
                Assert.Empty(notes);
            }

            [Fact]
            public void DeleteNote_ReturnsFalseForNonExistentId()
            {
                int nonExistentId = 999;

                bool result = noteManager.DeleteNote(nonExistentId);

                Assert.False(result);
            }

            [Fact]
            public void DeleteNotesByTripId_RemovesAllTripNotes()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                noteManager.AddNote(trip1, "Note 1", "Content 1");
                noteManager.AddNote(trip1, "Note 2", "Content 2");
                noteManager.AddNote(trip2, "Note 3", "Content 3");

                noteManager.DeleteNotesByTripId(trip1);
                var trip1Notes = noteManager.GetNotesForTrip(trip1);
                var trip2Notes = noteManager.GetNotesForTrip(trip2);

                Assert.Empty(trip1Notes);
                Assert.Single(trip2Notes);
            }

            [Fact]
            public void SearchNotes_FindsNotesByTitleAndContent()
            {
                noteManager.AddNote(tripId, "Meeting Notes", "Discussed project timeline");
                noteManager.AddNote(tripId, "Ideas", "Project implementation ideas");
                noteManager.AddNote(tripId, "Todo", "Complete the timeline");

                var searchResults1 = noteManager.SearchNotes(tripId, "timeline");
                var searchResults2 = noteManager.SearchNotes(tripId, "project");
                var searchResults3 = noteManager.SearchNotes(tripId, "nonexistent");

                Assert.Equal(2, searchResults1.Count); // "Meeting Notes" and "Todo" contain "timeline"
                Assert.Equal(2, searchResults2.Count); // "Meeting Notes" and "Ideas" contain "project"
                Assert.Empty(searchResults3); // No notes contain "nonexistent"
            }

            [Fact]
            public void SearchNotes_IsCaseInsensitive()
            {
                noteManager.AddNote(tripId, "Test Note", "This is a TEST");

                var searchResults1 = noteManager.SearchNotes(tripId, "test");
                var searchResults2 = noteManager.SearchNotes(tripId, "TEST");

                Assert.Single(searchResults1);
                Assert.Single(searchResults2);
            }

            [Fact]
            public void SearchNotesByTag_FindsNotesByTag()
            {
                noteManager.AddNote(tripId, "Note 1", "Content 1", new List<string> { "important", "work" });
                noteManager.AddNote(tripId, "Note 2", "Content 2", new List<string> { "personal", "important" });
                noteManager.AddNote(tripId, "Note 3", "Content 3", new List<string> { "work" });

                var tagResults1 = noteManager.SearchNotesByTag(tripId, "important");
                var tagResults2 = noteManager.SearchNotesByTag(tripId, "work");
                var tagResults3 = noteManager.SearchNotesByTag(tripId, "nonexistent");

                Assert.Equal(2, tagResults1.Count); // Notes 1 and 2 have "important" tag
                Assert.Equal(2, tagResults2.Count); // Notes 1 and 3 have "work" tag
                Assert.Empty(tagResults3); // No notes have "nonexistent" tag
            }

            [Fact]
            public void SetNotes_LoadsNotesCorrectly()
            {
                var notesToLoad = new List<Note>
                {
                    new Note { Id = 101, TripId = tripId, Title = "Note 1", Content = "Content 1", CreatedDate = DateTime.Now.AddDays(-1), Tags = new List<string> { "tag1" } },
                    new Note { Id = 102, TripId = tripId, Title = "Note 2", Content = "Content 2", CreatedDate = DateTime.Now.AddDays(-2), Tags = new List<string> { "tag2" } }
                };

                noteManager.SetNotes(notesToLoad);
                var loadedNotes = noteManager.GetAllNotes();

                Assert.Equal(2, loadedNotes.Count);
                Assert.Contains(loadedNotes, n => n.Id == 101 && n.Title == "Note 1");
                Assert.Contains(loadedNotes, n => n.Id == 102 && n.Title == "Note 2");
                
                // Add a new note to verify nextId is set correctly
                var newNote = noteManager.AddNote(tripId, "New Note", "New Content");
                Assert.Equal(103, newNote.Id);
            }

            [Fact]
            public void SetNotes_HandlesNullInput()
            {
                noteManager.AddNote(tripId, "Test Note", "Test Content");
                
                noteManager.SetNotes(new List<Note>());

                var notes = noteManager.GetNotesForTrip(tripId);
                
                Assert.Empty(notes); // Original note should still be there
            }
        }
}