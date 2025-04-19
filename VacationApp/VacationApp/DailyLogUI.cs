using System;
using System.Collections.Generic;
using System.IO;
using VacationApp.DailyLog;
using VacationApp.Trips;

namespace VacationApp.UI
{
    public class DailyLogUI
    {
        private readonly DailyLogManager dailyLogManager;
        private readonly TripManager tripManager;
        private DateTime currentDate;
        private List<DateTime> datesWithActivity;
        
        public DailyLogUI(DailyLogManager dailyLogManager, TripManager tripManager)
        {
            this.dailyLogManager = dailyLogManager;
            this.tripManager = tripManager;
        }
        
        // show the daily log main menu
        public void ShowDailyLogMenu(int tripId, string tripName)
        {
            var tripDetails = tripManager.GetTrip(tripId);

            //get all dates with items for this vacation
            datesWithActivity = dailyLogManager.GetDatesWithActivity(tripId);

            //for no dates, show a message
            if(datesWithActivity.Count == 0)
            {
                Console.Clear();
                DrawHeader($"Daily Log: {tripName}");
                Console.WriteLine("No items recorded for this vacation yet.");
                Console.WriteLine("\nPress any key to return to the main menu");
                Console.ReadKey();
                return;
            }

            //set current date as the most recent date with an item or today (if possible)
            if(datesWithActivity.Contains(DateTime.Today))
            {
                currentDate = DateTime.Today;
            }
            else if (DateTime.Today > tripDetails.EndDate && datesWithActivity.Any())
            {
                //use the last date with activity
                currentDate = datesWithActivity.Max();
            }
            else if (DateTime.Today < tripDetails.StartDate && datesWithActivity.Any())
            {
                //use the first date with activity
                currentDate = datesWithActivity.Min();
            }
            else
            {
                //closest date to today that has activity items
                var futureActivityDates = datesWithActivity.Where(date => date >= DateTime.Today).ToList();
                var pastActivityDates = datesWithActivity.Where(date => date < DateTime.Today).ToList();

                if (futureActivityDates.Any())
                {
                    currentDate = futureActivityDates.Min(); //closest future date
                }
                else if(pastActivityDates.Any())
                {
                    currentDate = pastActivityDates.Max(); //most recent past date
                }
                else
                {
                    //defer to vacation start date
                    currentDate = tripDetails.StartDate;
                }
            }

            while (true)
            {
                Console.Clear();
                DrawHeader($"Daily Log: {tripName}");
                
                var trip = tripManager.GetTrip(tripId);
                
                Console.WriteLine($"Vacation Dates: {trip.StartDate:MMM d} - {trip.EndDate:MMM d, yyyy}");
                Console.WriteLine();

                //display all dates with activity items
                Console.WriteLine("Days with activities:");
                int count = 0;
                foreach(var date in datesWithActivity.OrderByDescending(date => date))
                {
                    //highligh current date
                    if(date.Date == currentDate.Date)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"* {date: MMM d, yyyy}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {date: <MMM d, yyyy}");
                    }

                    count++;

                    //limit # of dates displayed to make it user-friendly
                    if(count >= 10)
                    {
                        if(datesWithActivity.Count > 10)
                        {
                            Console.WriteLine($"  ... and {datesWithActivity.Count-10} more");
                        }
                        break;
                    }
                }

                Console.WriteLine();
                DisplayDailyLog(tripId, currentDate);

                string[] options = {
                    "Previous Day with Activity",
                    "Next Day with Activity",
                    "Select Date",
                    "Edit Daily Log",
                    "Export Daily Log",
                    "Back to Main Menu"
                };

                //disable navigation options if there are no previous/next days with activity
                bool hasPrevious = dailyLogManager.GetPreviousActivityDate(tripId, currentDate).HasValue;
                bool hasNext = dailyLogManager.GetNextActivityDate(tripId, currentDate).HasValue;

                if (!hasPrevious)
                {
                    options[0] = "Previous Day with Activity (None)";
                }

                if(!hasNext)
                {
                    options[1] = "Next Day with Activity (None)";
                }

                Console.WriteLine("\nUse ↑/↓ arrow keys to navigate, ENTER to select:");
                Console.WriteLine();

                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // previous day
                        var prevDate = dailyLogManager.GetPreviousActivityDate(tripId, currentDate);
                        if (prevDate.HasValue)
                        {
                            currentDate = prevDate.Value;
                        }
                        break;
                    case 1: // next day
                        var nextDate = dailyLogManager.GetNextActivityDate(tripId, currentDate);
                        if (nextDate.HasValue)
                        {
                            currentDate = nextDate.Value;
                        }
                        break;
                    case 2: // select date
                        {
                            var tripInfo = tripManager.GetTrip(tripId);
                            
                            // Initialize the date variable with the result from SelectDate
                            DateTime selectedDate = SelectDate(tripInfo.StartDate, tripInfo.EndDate);
                            
                            // Check if the selected date has activity
                            bool hasActivity = dailyLogManager.GetDailyTimeline(tripId, selectedDate).Count > 0;
                            
                            // If no activity and there are dates with activity, find closest one
                            if (!hasActivity && datesWithActivity.Count > 0)
                            {
                                Console.WriteLine("\nNo activities recorded for the selected date.");
                                Console.WriteLine("Finding closest date with activity...");
                                
                                // Find closest date manually
                                DateTime closestDate = datesWithActivity[0];
                                double closestDistance = Math.Abs((closestDate - selectedDate).TotalDays);
                                
                                for (int i = 1; i < datesWithActivity.Count; i++)
                                {
                                    double distance = Math.Abs((datesWithActivity[i] - selectedDate).TotalDays);
                                    if (distance < closestDistance)
                                    {
                                        closestDistance = distance;
                                        closestDate = datesWithActivity[i];
                                    }
                                }
                                
                                selectedDate = closestDate;
                                Console.WriteLine($"Showing log for {selectedDate:MMM d, yyyy} instead.");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            }
                            
                            // Update the current date
                            currentDate = selectedDate;
                        }
                        break;
 
                    case 3: // edit daily log
                        EditDailyLog(tripId, currentDate);
                        break;
                    case 4: // export daily log
                        ExportDailyLog(tripId, currentDate);
                        break;
                    case 5: // back to main menu
                        return;
                }
            }
        }
        
        // display a daily log
        private void DisplayDailyLog(int tripId, DateTime date)
        {
            var log = dailyLogManager.GetDailyLog(tripId, date);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Date: {date:MMMM d, yyyy}");
            Console.ResetColor();
            
            if (log.IsAutoGenerated)
            {
                Console.WriteLine("(Auto-generated log)");
            }
            
            Console.WriteLine();
            Console.WriteLine(log.Summary);
        }
        
        // select a date
        private DateTime SelectDate(DateTime startDate, DateTime endDate)
        {
            Console.Clear();
            DrawHeader("Select Date");
            
            Console.WriteLine($"Trip dates: {startDate:MMM d, yyyy} - {endDate:MMM d, yyyy}");

            //display dates with activity
            Console.WriteLine("\nDates with activity:");
            foreach(var date in datesWithActivity.OrderBy(date => date).Take(10))
            {
                Console.WriteLine($"  {date: yyyy-MM-dd}");
            }

            if(datesWithActivity.Count > 10)
            {
                Console.WriteLine($"  ... and {datesWithActivity.Count - 10} more");
            }

            Console.WriteLine("\nEnter date [YYYY-MM-DD]:");
            
            DateTime selectedDate;
            while (true)
            {
                string input = Console.ReadLine();
                
                if (DateTime.TryParse(input, out selectedDate))
                {
                    selectedDate = selectedDate.Date; // remove time component
                    
                    if (selectedDate < startDate)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Date is before trip start. Using trip start date: {startDate:yyyy-MM-dd}");
                        Console.ResetColor();
                        selectedDate = startDate;
                        break;
                    }
                    else if (selectedDate > endDate)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Date is after trip end. Using trip end date: {endDate:yyyy-MM-dd}");
                        Console.ResetColor();
                        selectedDate = endDate;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid date format. Please use YYYY-MM-DD.");
                    Console.ResetColor();
                    Console.Write("Enter date: ");
                }
            }
            
            return selectedDate;
        }
        
        // edit a daily log
        private void EditDailyLog(int tripId, DateTime date)
        {
            var log = dailyLogManager.GetDailyLog(tripId, date);
            
            Console.Clear();
            DrawHeader($"Edit Daily Log: {date:MMMM d, yyyy}");
            
            Console.WriteLine("Current log content:");
            Console.WriteLine(log.Summary);
            
            Console.WriteLine("\nEnter new content (or press ENTER to keep current):");
            Console.WriteLine("Type END on a separate line when finished.");
            
            string content = "";
            string line;
            while (true)
            {
                line = Console.ReadLine();
                if (line == "END") break;
                content += line + Environment.NewLine;
            }
            
            if (!string.IsNullOrWhiteSpace(content))
            {
                dailyLogManager.UpdateDailyLog(log.Id, content.Trim());
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDaily log updated successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nNo changes made.");
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        //export a daily log
        private void ExportDailyLog(int tripId, DateTime date)
        {
            Console.Clear();
            DrawHeader("Export Daily Log");
            
            Console.WriteLine($"Date: {date:MMMM d, yyyy}");
            
            string[] options = {
                "Plain Text (.txt)",
                "CSV (.csv)",
                "Back"
            };
            
            Console.WriteLine("\nExport format:");
            Console.WriteLine();
            
            int selectedOption = ShowMenu(options);
            
            if (selectedOption == 2) // Back
            {
                return;
            }
            
            string format = selectedOption == 0 ? "Text" : "CSV";
            string extension = selectedOption == 0 ? "txt" : "csv";
            
            // generate a default file name
            string defaultFileName = $"vacation_log_{date:yyyyMMdd}.{extension}";
            
            Console.Write($"\nFile path to save [default: {defaultFileName}]: ");
            string filePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = defaultFileName;
            }
            
            try
            {
                string content = dailyLogManager.ExportDailyLog(tripId, date, format);
                File.WriteAllText(filePath, content);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nExport complete! File saved to: " + filePath);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError exporting file: " + ex.Message);
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
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
                // display all menu options
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