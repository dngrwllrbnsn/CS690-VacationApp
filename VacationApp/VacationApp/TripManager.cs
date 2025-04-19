using System;
using System.Collections.Generic;
using VacationApp.Photos;
using VacationApp.Expenses;
using VacationApp.Notes;
using VacationApp.DailyLog;

namespace VacationApp.Trips
{    
    //class to store vacation info
    public class Trip
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive{ get; set; }
    }

    //class to handle vacation operations
    public class TripManager
    {
        private List<Trip> trips = new List<Trip>();
        private int nextId = 1;

        //create a new trip
        public Trip CreateTrip(string name, string destination, DateTime startDate, DateTime endDate)
        {
            var trip = new Trip
            {
                Id = nextId++,
                Name = name,
                Destination = destination,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = false
            };

            trips.Add(trip);
            return trip;
        }

        //retrieve list of all trips
        public List<Trip> GetAllTrips()
        {
            return trips;
        }

        //retrieve a specific trip by its ID
        public Trip GetTrip(int tripId)
        {
            return trips.Find(trip => trip.Id == tripId);
        }

        //set a vacation as active
        public void SetActiveTrip(int tripId)
        {
            //deactivate all vacations
            foreach (var trip in trips)
            {
                trip.IsActive = false;
            }

            //activate user's selected vacation
            var selectedTrip = GetTrip(tripId);
            if(selectedTrip != null)
            {
                selectedTrip.IsActive = true;
            }
        }

        //retrieve the currently active vacation
        public Trip GetActiveTrip()
        {
            return trips.Find(trip => trip.IsActive);
        }

        //update a vacation's info
        public bool UpdateTrip(int tripId, string name, string destination, DateTime startDate, DateTime endDate)
        {
            var trip = GetTrip(tripId);
            if(trip != null)
            {
                trip.Name = name;
                trip.Destination = destination;
                trip.StartDate = startDate;
                trip.EndDate = endDate;
                return true;
            }
            return false;
        }

        //delete a vacation
        public bool DeleteTrip(int tripId, PhotoManager photoManager, ExpenseManager expenseManager,
                                NoteManager noteManager, DailyLogManager dailyLogManager)
        {
            var trip = GetTrip(tripId);
            if(trip != null)
            {
                // delete all associated trip data
                photoManager.DeletePhotosByTripId(trip.Id);
                expenseManager.DeleteExpensesByTripId(trip.Id);
                noteManager.DeleteNotesByTripId(trip.Id);
                dailyLogManager.DeleteDailyLogsByTripId(trip.Id);

                // delete the trip
                bool result = trips.Remove(trip);

                // if deleted trip was active trip, clear the active trip
                //if(activeTrip != null && activeTrip.Id == trip.Id)
                //{
                //    activeTrip = null;
                //}
                //return result;
            }
            return false;
        }

        //for serialization; load vacations from storage
        public void SetTrips(List<Trip> loadedTrips)
        {
            if (loadedTrips != null)
            {
                trips = loadedTrips;

                //identify the highest ID to correctly label the next ID
                nextId = 1;
                foreach(var trip in trips)
                {
                    if(trip.Id >= nextId)
                    {
                        nextId = trip.Id + 1;
                    }
                }
            }
        }
    }
}


