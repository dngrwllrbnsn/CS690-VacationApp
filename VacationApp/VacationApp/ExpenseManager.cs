using System;
using System.Collections.Generic;
using System.Linq; //for querying capabilities

namespace VacationApp.Expenses
{
    //class for storing expense details
    public class Expense
    {
        public int Id { get; set; }
        public int TripId{ get; set; }
        public decimal Amount{ get; set; }
        public string Description{ get; set; }
        public string Currency { get; set; }
        public DateTime Date{ get; set; }
        public string Category{ get; set; }
    }

    //class to handle expense operations
    public class ExpenseManager
    {
        private List<Expense> expenses = new List<Expense>();
        private int nextId = 1;
        private Dictionary<string, decimal> exchangeRates = new Dictionary<string, decimal>
        {
            // example exchange rates (base currency: USD)
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "JPY", 110.0m },
            { "CAD", 1.25m },
            { "AUD", 1.35m }
        };
        
        // add a new expense
        public Expense AddExpense(int tripId, decimal amount, string description, string currency, DateTime date, string category)
        {
            var expense = new Expense
            {
                Id = nextId++,
                TripId = tripId,
                Amount = amount,
                Description = description,
                Currency = currency,
                Date = date,
                Category = category
            };
            
            expenses.Add(expense);
            return expense;
        }
        
        // get all the expenses for a specific trip
        public List<Expense> GetExpensesForTrip(int tripId)
        {
            return expenses.FindAll(e => e.TripId == tripId);
        }
        
        // get a specific expense by ID
        public Expense GetExpense(int expenseId)
        {
            return expenses.Find(e => e.Id == expenseId);
        }
        
        // update an expense
        public bool UpdateExpense(int expenseId, decimal amount, string description, string currency, DateTime date, string category)
        {
            var expense = GetExpense(expenseId);
            if (expense != null)
            {
                expense.Amount = amount;
                expense.Description = description;
                expense.Currency = currency;
                expense.Date = date;
                expense.Category = category;
                return true;
            }
            return false;
        }
        
        // delete an expense
        public bool DeleteExpense(int expenseId)
        {
            var expense = GetExpense(expenseId);
            if (expense != null)
            {
                return expenses.Remove(expense);
            }
            return false;
        }

        // delete all expenses associated with a deleted vacation
        public void DeleteExpensesByTripId(int tripId)
        {
            expenses.RemoveAll(expense => expense.TripId == tripId);
        }
        
        // calculate total expenses for a trip
        public decimal GetTotalExpenses(int tripId, string targetCurrency = "USD")
        {
            decimal total = 0;
            foreach (var expense in GetExpensesForTrip(tripId))
            {
                total += ConvertCurrency(expense.Amount, expense.Currency, targetCurrency);
            }
            return total;
        }
        
        // get expenses grouped by category
        public Dictionary<string, decimal> GetExpensesByCategory(int tripId, string targetCurrency = "USD")
        {
            var result = new Dictionary<string, decimal>();
            
            var tripExpenses = GetExpensesForTrip(tripId);
            var categories = tripExpenses.Select(e => e.Category).Distinct();
            
            foreach (var category in categories)
            {
                decimal categoryTotal = 0;
                foreach (var expense in tripExpenses.Where(e => e.Category == category))
                {
                    categoryTotal += ConvertCurrency(expense.Amount, expense.Currency, targetCurrency);
                }
                result[category] = categoryTotal;
            }
            
            return result;
        }
        
        // convert expense amount between currencies
        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            // if same currency or currency not found, return original amount
            if (fromCurrency == toCurrency || !exchangeRates.ContainsKey(fromCurrency) || !exchangeRates.ContainsKey(toCurrency))
            {
                return amount;
            }
            
            // convert to USD first (as base currency)
            decimal amountInUSD = amount / exchangeRates[fromCurrency];
            
            // then convert from USD to target currency
            return amountInUSD * exchangeRates[toCurrency];
        }
        
        // get available currencies
        public List<string> GetAvailableCurrencies()
        {
            return exchangeRates.Keys.ToList();
        }
        
        // update exchange rate
        public void UpdateExchangeRate(string currency, decimal rate)
        {
            if (exchangeRates.ContainsKey(currency))
            {
                exchangeRates[currency] = rate;
            }
            else
            {
                exchangeRates.Add(currency, rate);
            }
        }
        
        // for serialization; get all expenses
        public List<Expense> GetAllExpenses()
        {
            return expenses;
        }
        
        // for serialization; load expenses from storage
        public void SetExpenses(List<Expense> loadedExpenses)
        {
            if (loadedExpenses != null)
            {
                expenses = loadedExpenses;
                
                // identify the highest ID in order to correctly set the next ID
                nextId = 1;
                foreach (var expense in expenses)
                {
                    if (expense.Id >= nextId)
                    {
                        nextId = expense.Id + 1;
                    }
                }
            }
        }
    }
}
