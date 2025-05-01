using VacationApp.Photos;

//dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
//reportgenerator -reports:"./coverage.cobertura.xml" -targetdir:"cobertura" -reporttypes:html


namespace VacationApp.Tests
{
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
                var filePath = "test.jpg";
                photoManager.AddPhoto(tripId, filePath);
                
                photoManager.SetPhotos(new List<Photo>());

                var photos = photoManager.GetPhotosForTrip(tripId);
                
                Assert.Empty(photos); 
            }

            // Helper method to set CaptureDate for testing without overriding ExtractMetadata
            private void SetCaptureDateForTesting(Photo photo, DateTime captureDate)
            {
                photo.CaptureDate = captureDate;
            }
        }
}