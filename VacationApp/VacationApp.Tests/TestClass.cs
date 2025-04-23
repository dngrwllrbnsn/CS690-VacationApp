using Xunit;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using VacationApp;
using VacationApp.Trips;
using VacationApp.Photos;
using VacationApp.Expenses;
using VacationApp.Notes;
using VacationApp.DailyLog;
using VacationApp.Settings;
using VacationApp.UI;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Configuration.Assemblies;
using System.ComponentModel;

//dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
//reportgenerator -reports:"./coverage.cobertura.xml" -targetdir:"cobertura" -reporttypes:html


namespace VacationApp.Tests
{
    public class VacationAppTests
    {
        //Test adding a trip
        [Fact]
        public void CanAddTrip()
        {
            var trip = new Trip
            {
                Id = 1,
                Name = "Skiing Vacation",
                Destination = "Snowmass",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };

            Assert.Equal("Skiing Vacation", trip.Name);
            Assert.Equal("Snowmass", trip.Destination);
            Assert.Equal(1, trip.Id);
            Assert.Equal(DateTime.Now.Date, trip.StartDate.Date);
            Assert.Equal(DateTime.Now.Date.AddDays(7), trip.EndDate.Date);
        }

        //Test adding a photo
        [Fact]
        public void CanAddPhoto()
        {
            var photo = new Photo
            {
                Id = 101,
                TripId = 1,
                FilePath = "snowmass.jpg",
                CaptureDate = DateTime.Now,
                Notes = "Ski lift up Buttermilk"
            };

            Assert.Equal(1, photo.TripId);
            Assert.Equal(101, photo.Id);
            Assert.Equal("snowmass.jpg", photo.FilePath);
            Assert.Equal("Ski lift up Buttermilk", photo.Notes);
            Assert.Equal(DateTime.Now.Date, photo.CaptureDate.Date);
        }

        //Test adding an expense
        [Fact]
        public void CanAddExpense()
        {
            var expense = new Expense
            {
                Id = 201,
                TripId = 1,
                Amount = 50.75m,
                Category = "Food",
                Currency = "USD",
                Date = DateTime.Now,
                Description = "Lunch in the lodge"
            };

            Assert.Equal(201, expense.Id);
            Assert.Equal(1, expense.TripId);
            Assert.Equal(50.75m, expense.Amount);
            Assert.Equal("Food", expense.Category);
            Assert.Equal("USD", expense.Currency);
            Assert.Equal("Lunch in the lodge", expense.Description);
        }

        //Test adding a note
        [Fact]
        public void CanAddNote()
        {
            var note = new Note
            {
                Id = 301,
                TripId = 1,
                Title = "Buttermilk Mountain",
                Content = "I wiped out right in front of the ski lift!",
                CreatedDate = DateTime.Now
            };

            Assert.Equal("Buttermilk Mountain", note.Title);
            Assert.Equal(301, note.Id);
            Assert.Equal(1, note.TripId);
            Assert.Equal("I wiped out right in front of the ski lift!", note.Content);
            Assert.Equal(DateTime.Now.Date, note.CreatedDate.Date);
        }

        //Test JSON serialization/deserialization
        [Fact]
        public void JSONTests()
        {
            var trip = new Trip
            {
                Id = 1,
                Name = "Eire 2025",
                Destination = "Ireland",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddDays(7),
            };

            //Serialize
            var json = System.Text.Json.JsonSerializer.Serialize(trip);

            //Deserialize
            var deserializedTrip = System.Text.Json.JsonSerializer.Deserialize<Trip>(json);

            Assert.Equal(trip.Id, deserializedTrip.Id);
            Assert.Equal(trip.Name, deserializedTrip.Name);
            Assert.Equal(trip.Destination, deserializedTrip.Destination);
            Assert.Equal(trip.StartDate.Date, deserializedTrip.StartDate.Date);
            Assert.Equal(trip.EndDate.Date, deserializedTrip.EndDate.Date);
        }

        /*
        
        
        MANAGER TESTING


        */

        //Expense Manager
        public class ExpenseManagerTests
        {
            private readonly ExpenseManager expenseManager;
            private readonly int tripId = 1;

            public ExpenseManagerTests()
            {
                expenseManager = new ExpenseManager();
            }

            [Fact]
            public void AddExpense_AddsNewExpense()
            {
                decimal amount = 100.00m;
                string description = "Dinner";
                string currency = "USD";
                DateTime date = new DateTime(2023, 7, 15);
                string category = "Food";

                var expense = expenseManager.AddExpense(tripId, amount, description, currency, date, category);
                var expenses = expenseManager.GetExpensesForTrip(tripId);

                Assert.Single(expenses);
                Assert.Equal(1, expense.Id);
                Assert.Equal(tripId, expense.TripId);
                Assert.Equal(amount, expense.Amount);
                Assert.Equal(description, expense.Description);
                Assert.Equal(currency, expense.Currency);
                Assert.Equal(date, expense.Date);
                Assert.Equal(category, expense.Category);
            }

            [Fact]
            public void GetExpensesForTrip_ReturnsOnlyTripExpenses()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                expenseManager.AddExpense(trip1, 100.00m, "Hotel", "USD", DateTime.Now, "Accommodation");
                expenseManager.AddExpense(trip1, 50.00m, "Lunch", "USD", DateTime.Now, "Food");
                expenseManager.AddExpense(trip2, 75.00m, "Museum", "EUR", DateTime.Now, "Activities");

                var trip1Expenses = expenseManager.GetExpensesForTrip(trip1);
                var trip2Expenses = expenseManager.GetExpensesForTrip(trip2);

                Assert.Equal(2, trip1Expenses.Count);
                Assert.Single(trip2Expenses);
                Assert.All(trip1Expenses, e => Assert.Equal(trip1, e.TripId));
                Assert.All(trip2Expenses, e => Assert.Equal(trip2, e.TripId));
            }

            [Fact]
            public void GetExpense_ReturnsCorrectExpense()
            {
                var expense = expenseManager.AddExpense(tripId, 100.00m, "Test", "USD", DateTime.Now, "Test");
                int expenseId = expense.Id;

                var retrievedExpense = expenseManager.GetExpense(expenseId);

                Assert.NotNull(retrievedExpense);
                Assert.Equal(expenseId, retrievedExpense.Id);
            }

            [Fact]
            public void GetExpense_ReturnsNullForNonExistentId()
            {
                int nonExistentId = 999;

                var result = expenseManager.GetExpense(nonExistentId);

                Assert.Null(result);
            }

            [Fact]
            public void UpdateExpense_UpdatesExistingExpense()
            {
                var expense = expenseManager.AddExpense(tripId, 100.00m, "Original", "USD", DateTime.Now, "Original");
                int expenseId = expense.Id;
                
                decimal newAmount = 150.00m;
                string newDescription = "Updated";
                string newCurrency = "EUR";
                DateTime newDate = new DateTime(2023, 8, 20);
                string newCategory = "Updated";

                bool result = expenseManager.UpdateExpense(expenseId, newAmount, newDescription, newCurrency, newDate, newCategory);
                var updatedExpense = expenseManager.GetExpense(expenseId);

                Assert.True(result);
                Assert.Equal(newAmount, updatedExpense.Amount);
                Assert.Equal(newDescription, updatedExpense.Description);
                Assert.Equal(newCurrency, updatedExpense.Currency);
                Assert.Equal(newDate, updatedExpense.Date);
                Assert.Equal(newCategory, updatedExpense.Category);
            }

            [Fact]
            public void UpdateExpense_ReturnsFalseForNonExistentId()
            {
                int nonExistentId = 999;

                bool result = expenseManager.UpdateExpense(nonExistentId, 100m, "Test", "USD", DateTime.Now, "Test");

                Assert.False(result);
            }

            [Fact]
            public void DeleteExpense_RemovesExpense()
            {
                var expense = expenseManager.AddExpense(tripId, 100.00m, "Test", "USD", DateTime.Now, "Test");
                int expenseId = expense.Id;

                bool result = expenseManager.DeleteExpense(expenseId);
                var expenses = expenseManager.GetExpensesForTrip(tripId);

                Assert.True(result);
                Assert.Empty(expenses);
            }

            [Fact]
            public void DeleteExpense_ReturnsFalseForNonExistentId()
            {
                int nonExistentId = 999;

                bool result = expenseManager.DeleteExpense(nonExistentId);

                Assert.False(result);
            }

            [Fact]
            public void DeleteExpensesByTripId_RemovesAllTripExpenses()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                expenseManager.AddExpense(trip1, 100.00m, "Test1", "USD", DateTime.Now, "Test");
                expenseManager.AddExpense(trip1, 200.00m, "Test2", "USD", DateTime.Now, "Test");
                expenseManager.AddExpense(trip2, 300.00m, "Test3", "USD", DateTime.Now, "Test");

                expenseManager.DeleteExpensesByTripId(trip1);
                var trip1Expenses = expenseManager.GetExpensesForTrip(trip1);
                var trip2Expenses = expenseManager.GetExpensesForTrip(trip2);

                Assert.Empty(trip1Expenses);
                Assert.Single(trip2Expenses);
            }

            [Fact]
            public void GetTotalExpenses_CalculatesCorrectTotal()
            {
                expenseManager.AddExpense(tripId, 100.00m, "Test1", "USD", DateTime.Now, "Test");
                expenseManager.AddExpense(tripId, 200.00m, "Test2", "USD", DateTime.Now, "Test");
                expenseManager.AddExpense(tripId, 50.00m, "Test3", "EUR", DateTime.Now, "Test"); // 50 EUR = ~58.82 USD

                decimal totalUSD = expenseManager.GetTotalExpenses(tripId, "USD");

                Assert.Equal(358.82m, totalUSD, 2); // Precision to 2 decimal places
            }

            [Fact]
            public void GetExpensesByCategory_GroupsExpensesCorrectly()
            {
                expenseManager.AddExpense(tripId, 100.00m, "Dinner", "USD", DateTime.Now, "Food");
                expenseManager.AddExpense(tripId, 50.00m, "Lunch", "USD", DateTime.Now, "Food");
                expenseManager.AddExpense(tripId, 200.00m, "Hotel", "USD", DateTime.Now, "Accommodation");
                expenseManager.AddExpense(tripId, 75.00m, "Museum", "USD", DateTime.Now, "Activities");

                var expensesByCategory = expenseManager.GetExpensesByCategory(tripId);

                Assert.Equal(3, expensesByCategory.Count);
                Assert.Equal(150.00m, expensesByCategory["Food"]);
                Assert.Equal(200.00m, expensesByCategory["Accommodation"]);
                Assert.Equal(75.00m, expensesByCategory["Activities"]);
            }

            [Fact]
            public void ConvertCurrency_ConvertsCorrectly()
            {
                decimal amount = 100.00m;
                string fromCurrency = "USD";
                string toCurrency = "EUR";

                decimal result = expenseManager.ConvertCurrency(amount, fromCurrency, toCurrency);

                Assert.Equal(85.00m, result);
            }

            [Fact]
            public void ConvertCurrency_ReturnsSameAmountForSameCurrency()
            {
                decimal amount = 100.00m;
                string currency = "USD";

                decimal result = expenseManager.ConvertCurrency(amount, currency, currency);

                Assert.Equal(amount, result);
            }

            [Fact]
            public void GetAvailableCurrencies_ReturnsAllCurrencies()
            {
                var currencies = expenseManager.GetAvailableCurrencies();

                Assert.Contains("USD", currencies);
                Assert.Contains("EUR", currencies);
                Assert.Contains("GBP", currencies);
                Assert.Contains("JPY", currencies);
                Assert.Contains("CAD", currencies);
                Assert.Contains("AUD", currencies);
                Assert.Equal(6, currencies.Count);
            }

            [Fact]
            public void UpdateExchangeRate_UpdatesExistingRate()
            {
                string currency = "EUR";
                decimal newRate = 0.90m;
                
                // Save the original amount for verification
                decimal originalAmount = 100.00m;
                decimal originalConverted = expenseManager.ConvertCurrency(originalAmount, "USD", currency);

                expenseManager.UpdateExchangeRate(currency, newRate);
                decimal newConverted = expenseManager.ConvertCurrency(originalAmount, "USD", currency);

                Assert.NotEqual(originalConverted, newConverted);
                Assert.Equal(90.00m, newConverted);
            }

            [Fact]
            public void UpdateExchangeRate_AddsNewCurrency()
            {
                string newCurrency = "CHF";
                decimal rate = 0.92m;

                expenseManager.UpdateExchangeRate(newCurrency, rate);
                var currencies = expenseManager.GetAvailableCurrencies();
                decimal converted = expenseManager.ConvertCurrency(100.00m, "USD", newCurrency);

                Assert.Contains(newCurrency, currencies);
                Assert.Equal(92.00m, converted);
            }

            [Fact]
            public void SetExpenses_LoadsExpensesCorrectly()
            {
                var expensesToLoad = new List<Expense>
                {
                    new Expense { Id = 101, TripId = tripId, Amount = 100.00m, Description = "Test1", Currency = "USD", Date = DateTime.Now, Category = "Test" },
                    new Expense { Id = 102, TripId = tripId, Amount = 200.00m, Description = "Test2", Currency = "EUR", Date = DateTime.Now, Category = "Test" }
                };

                expenseManager.SetExpenses(expensesToLoad);
                var loadedExpenses = expenseManager.GetAllExpenses();

                Assert.Equal(2, loadedExpenses.Count);
                Assert.Contains(loadedExpenses, e => e.Id == 101);
                Assert.Contains(loadedExpenses, e => e.Id == 102);
                
                // Add a new expense to verify nextId is set correctly
                var newExpense = expenseManager.AddExpense(tripId, 300.00m, "Test3", "USD", DateTime.Now, "Test");
                Assert.Equal(103, newExpense.Id);
            }
        }

        //Note Manager
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

                var note = noteManager.AddNote(tripId, title, content, null);

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
                noteManager.AddNote(tripId, "Initial Note", "Content");
                
                noteManager.SetNotes(null);
                var notes = noteManager.GetAllNotes();
                
                Assert.Single(notes); // Original note should still be there
            }
        }

        //Photo Manager
        public class PhotoManagerTests
        {
            private readonly PhotoManager photoManager;
            private readonly int tripId = 1;
            private readonly string testFilePath = "test_photo.jpg";

            public PhotoManagerTests()
            {
                photoManager = new PhotoManager();
            }

            [Fact]
            public void AddPhoto_AddsNewPhoto()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                var photos = photoManager.GetPhotosForTrip(tripId);

                Assert.Single(photos);
                Assert.Equal(1, photo.Id);
                Assert.Equal(tripId, photo.TripId);
                Assert.Equal(testFilePath, photo.FilePath);
                Assert.NotNull(photo.Tags);
                Assert.Empty(photo.Tags);
                Assert.Equal("", photo.Notes);
                Assert.True((DateTime.Now - photo.CaptureDate).TotalSeconds < 5); // Created within the last 5 seconds
            }

            [Fact]
            public void GetPhotosForTrip_ReturnsOnlyTripPhotos()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                photoManager.AddPhoto(trip1, "photo1.jpg");
                photoManager.AddPhoto(trip1, "photo2.jpg");
                photoManager.AddPhoto(trip2, "photo3.jpg");

                var trip1Photos = photoManager.GetPhotosForTrip(trip1);
                var trip2Photos = photoManager.GetPhotosForTrip(trip2);

                Assert.Equal(2, trip1Photos.Count);
                Assert.Single(trip2Photos);
                Assert.All(trip1Photos, p => Assert.Equal(trip1, p.TripId));
                Assert.All(trip2Photos, p => Assert.Equal(trip2, p.TripId));
            }

            [Fact]
            public void GetPhoto_ReturnsCorrectPhoto()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                int photoId = photo.Id;

                var retrievedPhoto = photoManager.GetPhoto(photoId);

                Assert.NotNull(retrievedPhoto);
                Assert.Equal(photoId, retrievedPhoto.Id);
                Assert.Equal(testFilePath, retrievedPhoto.FilePath);
            }

            [Fact]
            public void GetPhoto_ReturnsNullForNonExistentId()
            {
                int nonExistentId = 999;

                var result = photoManager.GetPhoto(nonExistentId);

                Assert.Null(result);
            }

            [Fact]
            public void AddTag_AddsTagToPhoto()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                string tag = "vacation";

                photoManager.AddTag(photo.Id, tag);

                var retrievedPhoto = photoManager.GetPhoto(photo.Id);
                Assert.Single(retrievedPhoto.Tags);
                Assert.Contains(tag, retrievedPhoto.Tags);
            }

            [Fact]
            public void AddTag_DoesNotAddDuplicateTag()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                string tag = "vacation";

                photoManager.AddTag(photo.Id, tag);
                photoManager.AddTag(photo.Id, tag); // Try to add the same tag again

                var retrievedPhoto = photoManager.GetPhoto(photo.Id);
                Assert.Single(retrievedPhoto.Tags);
                Assert.Contains(tag, retrievedPhoto.Tags);
            }

            [Fact]
            public void UpdateNotes_UpdatesPhotoNotes()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                string notes = "Beautiful sunset at the beach";

                photoManager.UpdateNotes(photo.Id, notes);

                var retrievedPhoto = photoManager.GetPhoto(photo.Id);
                Assert.Equal(notes, retrievedPhoto.Notes);
            }

            [Fact]
            public void UpdateLocation_UpdatesPhotoLocation()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                string location = "Malibu Beach";

                photoManager.UpdateLocation(photo.Id, location);

                var retrievedPhoto = photoManager.GetPhoto(photo.Id);
                Assert.Equal(location, retrievedPhoto.Location);
            }

            [Fact]
            public void SearchByTag_FindsPhotosByTag()
            {
                var photo1 = photoManager.AddPhoto(tripId, "photo1.jpg");
                var photo2 = photoManager.AddPhoto(tripId, "photo2.jpg");
                var photo3 = photoManager.AddPhoto(tripId, "photo3.jpg");
                
                photoManager.AddTag(photo1.Id, "beach");
                photoManager.AddTag(photo1.Id, "sunset");
                photoManager.AddTag(photo2.Id, "beach");
                photoManager.AddTag(photo3.Id, "mountain");

                var beachPhotos = photoManager.SearchByTag(tripId, "beach");
                var sunsetPhotos = photoManager.SearchByTag(tripId, "sunset");
                var cityPhotos = photoManager.SearchByTag(tripId, "city");

                Assert.Equal(2, beachPhotos.Count);
                Assert.Single(sunsetPhotos);
                Assert.Empty(cityPhotos);
            }

            [Fact]
            public void SearchByLocation_FindsPhotosByLocation()
            {
                var photo1 = photoManager.AddPhoto(tripId, "photo1.jpg");
                var photo2 = photoManager.AddPhoto(tripId, "photo2.jpg");
                var photo3 = photoManager.AddPhoto(tripId, "photo3.jpg");
                
                photoManager.UpdateLocation(photo1.Id, "Malibu Beach");
                photoManager.UpdateLocation(photo2.Id, "Venice Beach");
                photoManager.UpdateLocation(photo3.Id, "Grand Canyon");

                var beachPhotos = photoManager.SearchByLocation(tripId, "beach");
                var canyonPhotos = photoManager.SearchByLocation(tripId, "canyon");
                var cityPhotos = photoManager.SearchByLocation(tripId, "city");

                Assert.Equal(2, beachPhotos.Count);
                Assert.Single(canyonPhotos);
                Assert.Empty(cityPhotos);
            }

            [Fact]
            public void SearchByLocation_IsCaseInsensitive()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                photoManager.UpdateLocation(photo.Id, "Malibu Beach");

                var results1 = photoManager.SearchByLocation(tripId, "malibu");
                var results2 = photoManager.SearchByLocation(tripId, "MALIBU");

                Assert.Single(results1);
                Assert.Single(results2);
            }

            [Fact]
            public void SearchByDateRange_FindsPhotosInRange()
            {
                var photo1 = photoManager.AddPhoto(tripId, "photo1.jpg");
                var photo2 = photoManager.AddPhoto(tripId, "photo2.jpg");
                var photo3 = photoManager.AddPhoto(tripId, "photo3.jpg");
                
                // Set specific dates for testing
                SetCaptureDateForTesting(photo1, new DateTime(2023, 7, 1));
                SetCaptureDateForTesting(photo2, new DateTime(2023, 7, 15));
                SetCaptureDateForTesting(photo3, new DateTime(2023, 7, 30));

                var earlyJulyPhotos = photoManager.SearchByDateRange(
                    tripId, 
                    new DateTime(2023, 7, 1), 
                    new DateTime(2023, 7, 10)
                );
                
                var midJulyPhotos = photoManager.SearchByDateRange(
                    tripId, 
                    new DateTime(2023, 7, 10), 
                    new DateTime(2023, 7, 20)
                );
                
                var allJulyPhotos = photoManager.SearchByDateRange(
                    tripId, 
                    new DateTime(2023, 7, 1), 
                    new DateTime(2023, 7, 31)
                );

                Assert.Single(earlyJulyPhotos);
                Assert.Single(midJulyPhotos);
                Assert.Equal(3, allJulyPhotos.Count);
            }

            [Fact]
            public void SearchByNotes_FindsPhotosByNotesText()
            {
                var photo1 = photoManager.AddPhoto(tripId, "photo1.jpg");
                var photo2 = photoManager.AddPhoto(tripId, "photo2.jpg");
                var photo3 = photoManager.AddPhoto(tripId, "photo3.jpg");
                
                photoManager.UpdateNotes(photo1.Id, "Sunset at the beach");
                photoManager.UpdateNotes(photo2.Id, "Family picnic at the beach");
                photoManager.UpdateNotes(photo3.Id, "Mountain hiking");

                var beachPhotos = photoManager.SearchByNotes(tripId, "beach");
                var sunsetPhotos = photoManager.SearchByNotes(tripId, "sunset");
                var cityPhotos = photoManager.SearchByNotes(tripId, "city");

                Assert.Equal(2, beachPhotos.Count);
                Assert.Single(sunsetPhotos);
                Assert.Empty(cityPhotos);
            }

            [Fact]
            public void SearchByNotes_IsCaseInsensitive()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                photoManager.UpdateNotes(photo.Id, "Sunset at the beach");

                var results1 = photoManager.SearchByNotes(tripId, "sunset");
                var results2 = photoManager.SearchByNotes(tripId, "SUNSET");

                Assert.Single(results1);
                Assert.Single(results2);
            }

            [Fact]
            public void DeletePhoto_RemovesPhoto()
            {
                var photo = photoManager.AddPhoto(tripId, testFilePath);
                int photoId = photo.Id;

                bool result = photoManager.DeletePhoto(photoId);
                var photos = photoManager.GetPhotosForTrip(tripId);

                Assert.True(result);
                Assert.Empty(photos);
            }

            [Fact]
            public void DeletePhoto_ReturnsFalseForNonExistentId()
            {
                int nonExistentId = 999;

                bool result = photoManager.DeletePhoto(nonExistentId);

                Assert.False(result);
            }

            [Fact]
            public void DeletePhotosByTripId_RemovesAllTripPhotos()
            {
                int trip1 = 1;
                int trip2 = 2;
                
                photoManager.AddPhoto(trip1, "photo1.jpg");
                photoManager.AddPhoto(trip1, "photo2.jpg");
                photoManager.AddPhoto(trip2, "photo3.jpg");

                photoManager.DeletePhotosByTripId(trip1);
                var trip1Photos = photoManager.GetPhotosForTrip(trip1);
                var trip2Photos = photoManager.GetPhotosForTrip(trip2);

                Assert.Empty(trip1Photos);
                Assert.Single(trip2Photos);
            }

            [Fact]
            public void SetPhotos_LoadsPhotosCorrectly()
            {
                var photosToLoad = new List<Photo>
                {
                    new Photo { Id = 101, TripId = tripId, FilePath = "photo1.jpg", CaptureDate = DateTime.Now.AddDays(-1) },
                    new Photo { Id = 102, TripId = tripId, FilePath = "photo2.jpg", CaptureDate = DateTime.Now.AddDays(-2) }
                };

                photoManager.SetPhotos(photosToLoad);
                var loadedPhotos = photoManager.GetAllPhotos();

                Assert.Equal(2, loadedPhotos.Count);
                Assert.Contains(loadedPhotos, p => p.Id == 101 && p.FilePath == "photo1.jpg");
                Assert.Contains(loadedPhotos, p => p.Id == 102 && p.FilePath == "photo2.jpg");
                
                // Add a new photo to verify nextId is set correctly
                var newPhoto = photoManager.AddPhoto(tripId, "newPhoto.jpg");
                Assert.Equal(103, newPhoto.Id);
            }

            [Fact]
            public void SetPhotos_HandlesNullInput()
            {
                photoManager.AddPhoto(tripId, "initial.jpg");
                
                photoManager.SetPhotos(null);
                var photos = photoManager.GetAllPhotos();
                
                Assert.Single(photos); // Original photo should still be there
            }

            // Helper method to set CaptureDate for testing without overriding ExtractMetadata
            private void SetCaptureDateForTesting(Photo photo, DateTime captureDate)
            {
                photo.CaptureDate = captureDate;
            }
        }

        //Settings Manager
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
}