using System;
using System.Collections.Generic;
using System.IO;
using MetadataExtractor; //dotnet add package MetadataExtractor
using MetadataExtractor.Formats.Exif;
using System.Linq; //for querying capabilities

namespace VacationApp.Photos
{
    // class that stores photo info
    public class Photo
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string FilePath { get; set; }
        public DateTime CaptureDate { get; set; }
        public string Location { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string Notes { get; set; }

        //extract photo metadata
        public void ExtractMetadata()
        {
            //try to find the file using helper 
            string validPath;
            bool fileExists = FileHelper.TryFindFile(FilePath, out validPath);

            if(fileExists)
            {
                //update path if it was corrected
                if(validPath != FilePath)
                {
                    FilePath = validPath;
                }

                try
                {
                    //read photo metadata and separate categories into groups
                    var groups = ImageMetadataReader.ReadMetadata(FilePath);

                    //try to obtain date taken
                    var subIfdgroup = groups.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                    if (subIfdgroup != null && subIfdgroup.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTaken))
                    {
                        CaptureDate = dateTaken;
                    }
                    else
                    {
                        //use file creation time instead
                        var fileInfo = new FileInfo(FilePath);
                        CaptureDate = fileInfo.CreationTime;
                    }
                }
                catch(Exception ex)
                {
                    //handle exceptions (corrupted image, unsupported file)
                    Console.WriteLine($"Error extracting metadata: {ex.Message}");

                    //go back to file creation time
                    var fileInfo = new FileInfo(FilePath);
                    CaptureDate = fileInfo.CreationTime;
                }
            }
            
            else
            {
                CaptureDate = DateTime.Now;
            }
        }        
    }

    //class to handle photo operations
    public class PhotoManager
    {
        private List<Photo> photos = new List<Photo>();
        private int nextId = 1;

        //add a new photo
        public Photo AddPhoto(int tripId, string filePath)
        {
            var photo = new Photo
            {
                Id = nextId++,
                TripId = tripId,
                FilePath = filePath,
                CaptureDate = DateTime.Now,
                Tags = new List<string>(),
                Notes = ""
            };

            //try to extract metadata
            photo.ExtractMetadata();
            photos.Add(photo);
            return photo;
        }

        //get all photos for a specific trip
        public List<Photo> GetPhotosForTrip(int tripId)
        {
            return photos.FindAll(pic => pic.TripId == tripId);
        }

        //get a specific photo by ID
        public Photo GetPhoto(int photoId)
        {
            return photos.Find(pic => pic.Id == photoId);
        }

        //update photo tags
        public void AddTag(int photoId, string tag)
        {
            var photo = GetPhoto(photoId);
            if(photo != null && !photo.Tags.Contains(tag))
            {
                photo.Tags.Add(tag);
            }
        }

        //update photo notes
        public void UpdateNotes(int photoId, string notes)
        {
            var photo = GetPhoto(photoId);
            if(photo != null)
            {
                photo.Notes = notes;
            }
        }

        //update photo location
        public void UpdateLocation(int photoId, string location)
        {
            var photo = GetPhoto(photoId);
            if(photo != null)
            {
                photo.Location = location;
            }
        }

        //search photos by tag
        public List<Photo> SearchByTag(int tripId, string tag)
        {
            return photos.FindAll(pic => pic.TripId == tripId && pic.Tags.Contains(tag));
        }

        //search photos by location
        public List<Photo> SearchByLocation(int tripId, string location)
        {
            return photos.FindAll(pic => pic.TripId == tripId &&
                                    pic.Location != null &&
                                    pic.Location.ToLower().Contains(location.ToLower()));
        }

        //search photos by date range
        public List<Photo> SearchByDateRange(int tripId, DateTime startDate, DateTime endDate)
        {
            return photos.FindAll(pic => pic.TripId == tripId &&
                                    pic.CaptureDate >= startDate &&
                                    pic.CaptureDate <= endDate);
        }

        //search photos by text in notes
        public List<Photo> SearchByNotes(int tripId, string searchText)
        {
            return photos.FindAll(pic => pic.TripId == tripId &&
                                    pic.Notes != null &&
                                    pic.Notes.ToLower().Contains(searchText.ToLower()));
        }

        //delete a photo
        public bool DeletePhoto(int photoId)
        {
            var photo = GetPhoto(photoId);
            if(photo != null)
            {
                return photos.Remove(photo);
            }
            return false;
        }

        // Delete all photos associated with a deleted vacation
        public void DeletePhotosByTripId(int tripId)
        {
            photos.RemoveAll(pic => pic.TripId == tripId);
        }

        //JSON serialization: get all photos
        public List<Photo> GetAllPhotos()
        {
            return photos;
        }

        //JSON deserialization: set photos & calculate next ID
        public void SetPhotos(List<Photo> loadedPhotos)
        {
            if(loadedPhotos != null)
            {
                photos = loadedPhotos;

                //identify highest ID to set the next ID correctly
                nextId = 1;
                foreach(var photo in photos)
                {
                    if(photo.Id >= nextId)
                    {
                        nextId = photo.Id + 1; 
                    }
                }
            }
        }
    }
}