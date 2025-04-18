﻿using System;
using System.Collections.Generic;
using System.Linq; //for querying capabilities
using VacationApp.Photos;
using VacationApp.Expenses;
using VacationApp.Notes;

namespace VacationApp.DailyLog
{

    // class to represent a daily log entry
    public class DailyLogEntry
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public bool IsAutoGenerated { get; set; }
    }
    
    // class to represent a timeline item
    public class ActivityItem
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } // "Photo", "Expense", "Note"
        public int ItemId { get; set; }
        public string Description { get; set; }
    }
    
    // class to handle daily log operations
    public class DailyLogManager
    {
        private List<DailyLogEntry> dailyLogs = new List<DailyLogEntry>();
        private int nextId = 1;
        
        private readonly PhotoManager photoManager;
        private readonly ExpenseManager expenseManager;
        private readonly NoteManager noteManager;
        
        public DailyLogManager(PhotoManager photoManager, ExpenseManager expenseManager, NoteManager noteManager)
        {
            this.photoManager = photoManager;
            this.expenseManager = expenseManager;
            this.noteManager = noteManager;
        }

        //get all dates that have items for a specific vacation
        public List<DateTime> GetDatesWithActivity(int tripId)
        {
            var datesWithActivity = new HashSet<DateTime>();

            //check photos
            foreach(var photo in photoManager.GetPhotosForTrip(tripId))
            {
                datesWithActivity.Add(photo.CaptureDate.Date);
            }

            //check expenses
            foreach(var expense in expenseManager.GetExpensesForTrip(tripId))
            {
                datesWithActivity.Add(expense.Date.Date);
            }

            //check notes
            foreach(var note in noteManager.GetNotesForTrip(tripId))
            {
                datesWithActivity.Add(note.CreatedDate.Date);
            }

            return datesWithActivity.OrderBy(date => date).ToList();
        }

        //check if a date has any items 
        public bool HasActivity(int tripId, DateTime date)
        {
            //remove time from date
            date = date.Date;

            //check for items
            var timeline = GetDailyTimeline(tripId, date);
            return timeline.Count > 0;
        }

        //get the previous date that has activity
        public DateTime? GetPreviousActivityDate(int tripId, DateTime currentDate)
        {
            var datesWithActivity = GetDatesWithActivity(tripId);
            return datesWithActivity.LastOrDefault(date => date < currentDate.Date);
        }

        //get the next date that has activity
        public DateTime? GetNextActivityDate(int tripId, DateTime currentDate)
        {
            var datesWithActivity = GetDatesWithActivity(tripId);
            return datesWithActivity.FirstOrDefault(date => date > currentDate.Date);
        }
        
        // get or create a daily log for a specific date
        public DailyLogEntry GetDailyLog(int tripId, DateTime date)
        {
            // normalize date to remove time component
            date = date.Date;
            
            // try to find existing log
            var log = dailyLogs.Find(log => log.TripId == tripId && log.Date.Date == date);
            
            // if not found, create a new one
            if (log == null)
            {
                log = new DailyLogEntry
                {
                    Id = nextId++,
                    TripId = tripId,
                    Date = date,
                    Summary = GenerateSummary(tripId, date),
                    IsAutoGenerated = true
                };
                
                dailyLogs.Add(log);
            }

            //if log already exists and is auto-generated, refresh its summary to include new data
            else if (log.IsAutoGenerated)
            {
                log.Summary = GenerateSummary(tripId, date);
            }
            
            return log;
        }
        
        // update a daily log
        public bool UpdateDailyLog(int logId, string summary)
        {
            var log = dailyLogs.Find(log => log.Id == logId);
            if (log != null)
            {
                log.Summary = summary;
                log.IsAutoGenerated = false;
                return true;
            }
            return false;
        }

        // get daily logs for a trip
        public List<DailyLogEntry> GetDailyLogsForTrip(int tripId)
        {
            var logs = dailyLogs.FindAll(log => log.TripId == tripId);

            //refresh all auto-generated logs to ensure they have the latest data
            foreach(var log in logs)
            {
                if(log.IsAutoGenerated)
                {
                    log.Summary = GenerateSummary(tripId, log.Date);
                }
            }

            return logs;
        }
        
        // generate activity timeline for a day
        public List<ActivityItem> GetDailyTimeline(int tripId, DateTime date)
        {
            // normalize date to remove time component
            date = date.Date;
            DateTime nextDay = date.AddDays(1);
            
            var timeline = new List<ActivityItem>();
            
            // add photos taken on this day
            foreach (var photo in photoManager.GetPhotosForTrip(tripId))
            {
                if (photo.CaptureDate.Date == date)
                {
                    timeline.Add(new ActivityItem
                    {
                        Timestamp = photo.CaptureDate,
                        Type = "Photo",
                        ItemId = photo.Id,
                        Description = photo.Notes ?? "Photo" + (photo.Location != null ? " at " + photo.Location : "")
                    });
                }
            }
            
            // add expenses from this day
            foreach (var expense in expenseManager.GetExpensesForTrip(tripId))
            {
                if (expense.Date.Date == date)
                {
                    timeline.Add(new ActivityItem
                    {
                        Timestamp = expense.Date,
                        Type = "Expense",
                        ItemId = expense.Id,
                        Description = $"{expense.Amount} {expense.Currency} - {expense.Category} - {expense.Description}"
                    });
                }
            }
            
            // add notes created on this day
            foreach (var note in noteManager.GetNotesForTrip(tripId))
            {
                if (note.CreatedDate.Date == date)
                {
                    timeline.Add(new ActivityItem
                    {
                        Timestamp = note.CreatedDate,
                        Type = "Note",
                        ItemId = note.Id,
                        Description = note.Title
                    });
                }
            }
            
            // sort timeline by timestamp
            return timeline.OrderBy(i => i.Timestamp).ToList();
        }

        // generate a summary for a day
        private string GenerateSummary(int tripId, DateTime date)
        {
            // normalize date to remove time component
            date = date.Date;
            
            // get timeline for the day
            var timeline = GetDailyTimeline(tripId, date);
            
            if (timeline.Count == 0)
            {
                return "No activities recorded for this day.";
            }
            
            // count items by type
            int photoCount = timeline.Count(i => i.Type == "Photo");
            int expenseCount = timeline.Count(i => i.Type == "Expense");
            int noteCount = timeline.Count(i => i.Type == "Note");
            
            // calculate total expenses for the day
            decimal totalExpenses = 0;
            var expenses = expenseManager.GetExpensesForTrip(tripId).Where(e => e.Date.Date == date);
            foreach (var expense in expenses)
            {
                totalExpenses += expenseManager.ConvertCurrency(expense.Amount, expense.Currency, "USD");
            }
            
            // generate summary
            string summary = $"Daily Summary for {date:MMMM d, yyyy}:\n\n";
            
            if (photoCount > 0)
                summary += $"- {photoCount} photo{(photoCount != 1 ? "s" : "")} taken\n";
                
            if (expenseCount > 0)
                summary += $"- {expenseCount} expense{(expenseCount != 1 ? "s" : "")} recorded (${totalExpenses:F2} USD)\n";
                
            if (noteCount > 0)
                summary += $"- {noteCount} note{(noteCount != 1 ? "s" : "")} created\n";
                
            summary += "\nTimeline:\n";
            
            foreach (var item in timeline)
            {
                summary += $"{item.Timestamp:HH:mm} - {item.Type}: {item.Description}\n";
            }
            
            return summary;
        }

        // export a daily log
        public string ExportDailyLog(int tripId, DateTime date, string format = "Text")
        {
            var log = GetDailyLog(tripId, date);
            
            if (format.ToLower() == "text")
            {
                return log.Summary;
            }
            else if (format.ToLower() == "csv")
            {
                var timeline = GetDailyTimeline(tripId, date);
                string csv = "Time,Type,Description\n";
                
                foreach (var item in timeline)
                {
                    // handle escape quotes in description
                    string escapedDesc = item.Description.Replace("\"", "\"\"");
                    csv += $"{item.Timestamp:HH:mm},\"{item.Type}\",\"{escapedDesc}\"\n";
                }
                
                return csv;
            }
            
            return log.Summary; // default to text format
        }

        // for serialization; get all daily logs
        public List<DailyLogEntry> GetAllDailyLogs()
        {
            return dailyLogs;
        }
        
        // for serialization; load daily logs from storage
        public void SetDailyLogs(List<DailyLogEntry> loadedLogs)
        {
            if (loadedLogs != null)
            {
                dailyLogs = loadedLogs;
                
                // identify the highest ID in order to correctly set the next ID
                nextId = 1;
                foreach (var log in dailyLogs)
                {
                    if (log.Id >= nextId)
                    {
                        nextId = log.Id + 1;
                    }
                }
            }
        }

        //refresh a log with the latest data
        public bool RefreshDailyLog(int logId)
        {
            var log = dailyLogs.Find(log => log.Id == logId);
            if (log != null && log.IsAutoGenerated)
            {
                log.Summary = GenerateSummary(log.TripId, log.Date);
                return true;
            }
            return false;
        }

        // delete daily logs associated with a deleted vacation
        public void DeleteDailyLogsByTripId(int tripId)
        {
            dailyLogs.RemoveAll(log => log.TripId == tripId);
        }
    }
}