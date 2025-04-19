using System;
using System.Collections.Generic;
using VacationApp.Expenses;
using VacationApp.Trips;


namespace VacationApp.UI
{
    public class ExpenseUI
    {
private readonly ExpenseManager expenseManager;
        private readonly TripManager tripManager;
        
        public ExpenseUI(ExpenseManager expenseManager, TripManager tripManager)
        {
            this.expenseManager = expenseManager;
            this.tripManager = tripManager;
        }
        
        // show expense tracking menu
        public void ShowExpenseMenu(int tripId, string tripName)
        {
            while (true)
            {
                Console.Clear();
                DrawHeader($"Expenses: {tripName}");
                
                // show expense summary
                decimal totalExpenses = expenseManager.GetTotalExpenses(tripId);
                Console.WriteLine($"Total Expenses: ${totalExpenses:F2} USD");
                
                string[] options = {
                    "View All Expenses",
                    "Add New Expense",
                    "Expense Summary",
                    "View/Edit Currency Exchange Rates",
                    "Back to Main Menu"
                };
                
                Console.WriteLine("\nUse ↑/↓ arrow keys to navigate, Enter to select:");
                Console.WriteLine();
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // view all expenses
                        ViewAllExpenses(tripId, tripName);
                        break;
                    case 1: // add new expense
                        AddNewExpense(tripId);
                        break;
                    case 2: // expense summary
                        ShowExpenseSummary(tripId, tripName);
                        break;
                    case 3: // set currency exchange rate
                        SetCurrencyExchange();
                        break;
                    case 4: // back to main menu
                        return;
                }
            }
        }
        
        // view all expenses for a vacation
        private void ViewAllExpenses(int tripId, string tripName)
        {
            var expenses = expenseManager.GetExpensesForTrip(tripId);
            
            if (expenses.Count == 0)
            {
                Console.Clear();
                DrawHeader($"Expenses: {tripName}");
                Console.WriteLine("No expenses found for this vacation.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            while (true)
            {
                Console.Clear();
                DrawHeader($"Expenses: {tripName}");
                
                Console.WriteLine("ID  | Date       | Amount       | Category    | Description");
                Console.WriteLine(new string('-', 70));
                
                foreach (var expense in expenses)
                {
                    Console.WriteLine($"{expense.Id:D3} | {expense.Date:yyyy-MM-dd} | " +
                                    $"{expense.Amount:F2} {expense.Currency} | " +
                                    $"{expense.Category,-11} | {expense.Description}");
                }
                
                Console.WriteLine("\nEnter expense ID to view details (or ENTER to cancel): ");
                string input = Console.ReadLine();
                
                if (input == "0" || string.IsNullOrEmpty(input))
                {
                    return;
                }
                
                if (int.TryParse(input, out int expenseId))
                {
                    var expense = expenseManager.GetExpense(expenseId);
                    if (expense != null && expense.TripId == tripId)
                    {
                        EditExpense(expense);
                    }
                    else
                    {
                        Console.WriteLine("Expense not found. Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else
                {Console.WriteLine("Invalid ID. Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }
        
        // edit an expense
        private void EditExpense(Expense expense)
        {
            while (true)
            {
                Console.Clear();
                DrawHeader($"Expense Details: ID {expense.Id}");
                
                Console.WriteLine($"Date: {expense.Date:yyyy-MM-dd}");
                Console.WriteLine($"Amount: {expense.Amount:F2} {expense.Currency}");
                Console.WriteLine($"Category: {expense.Category}");
                Console.WriteLine($"Description: {expense.Description}");
                Console.WriteLine();
                
                string[] options = {
                    "Edit Amount",
                    "Edit Description",
                    "Edit Currency",
                    "Edit Date",
                    "Edit Category",
                    "Delete Expense",
                    "Back to Expenses List"
                };
                
                int selectedOption = ShowMenu(options);
                
                switch (selectedOption)
                {
                    case 0: // edit expense amount
                        Console.WriteLine($"\nCurrent Amount: {expense.Amount:F2}");
                        Console.Write("Enter new Amount: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal newAmount))
                        {
                            decimal oldAmount = expense.Amount;
                            expense.Amount = newAmount;
                            expenseManager.UpdateExpense(expense.Id, expense.Amount, expense.Description, expense.Currency, expense.Date, expense.Category);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\nAmount updated from {oldAmount:F2} to {newAmount:F2}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid amount format.");
                            Console.ResetColor();
                        }
                        break;
                        
                    case 1: // edit expense description
                        Console.WriteLine($"\nCurrent Description: {expense.Description}");
                        Console.Write("Enter new Description: ");
                        string oldDescription = expense.Description;
                        string newDescription = Console.ReadLine();
                        
                        expense.Description = newDescription;
                        expenseManager.UpdateExpense(expense.Id, expense.Amount, expense.Description, expense.Currency, expense.Date, expense.Category);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nDescription updated from \"{oldDescription}\" to \"{newDescription}\"");
                        Console.ResetColor();
                        break;
                        
                    case 2: // edit currency
                        Console.WriteLine($"\nCurrent Currency: {expense.Currency}");
                        Console.WriteLine("Available Currencies:");
                        foreach (var currency in expenseManager.GetAvailableCurrencies())
                        {
                            Console.WriteLine($"- {currency}");
                        }
                        
                        Console.Write("\nEnter new Currency: ");
                        string oldCurrency = expense.Currency;
                        string newCurrency = Console.ReadLine().ToUpper();
                        
                        if (expenseManager.GetAvailableCurrencies().Contains(newCurrency))
                        {
                            expense.Currency = newCurrency;
                            expenseManager.UpdateExpense(expense.Id, expense.Amount, expense.Description, expense.Currency, expense.Date, expense.Category);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\nCurrency updated from {oldCurrency} to {newCurrency}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid currency code.");
                            Console.ResetColor();
                        }
                        break;
                        
                    case 3: // edit date
                        Console.WriteLine($"\nCurrent Date: {expense.Date:yyyy-MM-dd}");
                        Console.Write("Enter new Date [YYYY-MM-DD]: ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime newDate))
                        {
                            DateTime oldDate = expense.Date;
                            expense.Date = newDate;
                            expenseManager.UpdateExpense(expense.Id, expense.Amount, expense.Description, expense.Currency, expense.Date, expense.Category);
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\nDate updated from {oldDate:yyyy-MM-dd} to {newDate:yyyy-MM-dd}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid date format.");
                            Console.ResetColor();
                        }
                        break;
                        
                    case 4: // edit category
                        Console.WriteLine($"\nCurrent Category: {expense.Category}");
                        Console.WriteLine("Common Categories: Food, Transportation, Accommodation, Activities, Shopping, Other");
                        Console.Write("\nEnter new Category: ");
                        string oldCategory = expense.Category;
                        string newCategory = Console.ReadLine();
                        
                        expense.Category = newCategory;
                        expenseManager.UpdateExpense(expense.Id, expense.Amount, expense.Description, expense.Currency, expense.Date, expense.Category);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nCategory updated from \"{oldCategory}\" to \"{newCategory}\"");
                        Console.ResetColor();
                        break;
                        
                    case 5: // delete expense
                        Console.WriteLine($"\nAre you sure you want to delete this expense? (Y/N): ");
                        if (Console.ReadLine()?.ToUpper() == "Y")
                        {
                            if (expenseManager.DeleteExpense(expense.Id))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\nExpense deleted successfully!");
                                Console.ResetColor();
                                Console.WriteLine("\nPress any key to return...");
                                Console.ReadKey();
                                return; // exit back to expense list
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nFailed to delete expense.");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nDelete operation cancelled.");
                        }
                        break;
                        
                    case 6: // back
                        return;
                }
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
        
        // add a new expense
        private void AddNewExpense(int tripId)
        {
            Console.Clear();
            DrawHeader("Add New Expense");
            
            // get expense amount
            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid amount format.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            // get currency
            Console.WriteLine("\nAvailable Currencies:");
            foreach (var currency in expenseManager.GetAvailableCurrencies())
            {
                Console.WriteLine($"- {currency}");
            }
            
            Console.Write("\nCurrency: ");
            string currencyInput = Console.ReadLine().ToUpper();
            
            if (!expenseManager.GetAvailableCurrencies().Contains(currencyInput))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid currency code.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }
            
            // get category
            Console.WriteLine("\nCommon Categories: Food, Transportation, Accommodation, Activities, Shopping, Other");
            Console.Write("Category: ");
            string category = Console.ReadLine();
            
            // get description
            Console.Write("\nDescription: ");
            string description = Console.ReadLine();
            
            // get date (default to today)
            Console.Write("\nDate [YYYY-MM-DD] (leave blank for today): ");
            string dateInput = Console.ReadLine();
            DateTime date = string.IsNullOrWhiteSpace(dateInput) ? DateTime.Today : DateTime.Parse(dateInput);
            
            // create the expense
            var expense = expenseManager.AddExpense(tripId, amount, description, currencyInput, date, category);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nExpense added successfully!");
            Console.ResetColor();
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // show expense summary
        private void ShowExpenseSummary(int tripId, string tripName)
        {
            Console.Clear();
            DrawHeader($"Expenses Summary: {tripName}");
            
            // get base currency (default: USD)
            Console.Write("Display in currency [default: USD]: ");
            string currency = Console.ReadLine().ToUpper();
            if (string.IsNullOrWhiteSpace(currency))
            {
                currency = "USD";
            }
            
            if (!expenseManager.GetAvailableCurrencies().Contains(currency))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid currency code. Using USD.");
                Console.ResetColor();
                currency = "USD";
            }
            
            // get total expenses
            decimal total = expenseManager.GetTotalExpenses(tripId, currency);
            
            // get expenses by category
            var expensesByCategory = expenseManager.GetExpensesByCategory(tripId, currency);
            
            // display summary
            Console.WriteLine($"\nTotal expenses: {total:F2} {currency}");
            Console.WriteLine("\nBreakdown by Category:");
            Console.WriteLine("Category       | Amount");
            Console.WriteLine(new string('-', 30));
            
            foreach (var category in expensesByCategory)
            {
                Console.WriteLine($"{category.Key,-15} | {category.Value:F2} {currency}");
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        
        // set base currency
        private void SetCurrencyExchange()
        {
            Console.Clear();
            DrawHeader("View/Edit Currency Exchange Rates");
            
            Console.WriteLine("This feature allows you to update or add exchange rates.");
            Console.WriteLine("\nCurrent Exchange Rates (base: USD):");
            
            foreach (var currency in expenseManager.GetAvailableCurrencies())
            {
                Console.WriteLine($"- {currency}");
            }
            
            Console.Write("\nSelect currency to update (or type in a new currency to add)(ENTER to cancel): ");
            string currencyInput = Console.ReadLine().ToUpper();
            
            if (!expenseManager.GetAvailableCurrencies().Contains(currencyInput))
            {
                Console.Write($"\nWould you like to add {currencyInput} as new currency? (Y/N)(ENTER to cancel): ");
                if (Console.ReadLine()?.ToUpper() != "Y")
                {
                    Console.WriteLine("\nOperation cancelled.");
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey();
                    return;
                }
            }
            
            Console.Write($"\nEnter exchange rate (1 USD = ? {currencyInput}): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal rate))
            {
                expenseManager.UpdateExchangeRate(currencyInput, rate);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nExchange rate updated successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid rate format.");
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
        
        // method to show expense menu with arrow key navigation
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
