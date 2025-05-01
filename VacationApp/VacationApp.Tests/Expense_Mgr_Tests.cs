using VacationApp.Expenses;

//dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
//reportgenerator -reports:"./coverage.cobertura.xml" -targetdir:"cobertura" -reporttypes:html


namespace VacationApp.Tests
{
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
}