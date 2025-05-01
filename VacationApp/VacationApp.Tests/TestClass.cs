using VacationApp.Trips;
using VacationApp.Photos;
using VacationApp.Expenses;
using VacationApp.Notes;

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

            Assert.NotNull(deserializedTrip);
            Assert.Equal(trip.Id, deserializedTrip.Id);
            Assert.Equal(trip.Name, deserializedTrip.Name);
            Assert.Equal(trip.Destination, deserializedTrip.Destination);
            Assert.Equal(trip.StartDate.Date, deserializedTrip.StartDate.Date);
            Assert.Equal(trip.EndDate.Date, deserializedTrip.EndDate.Date);
        }
    }
}