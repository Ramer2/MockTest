using Microsoft.Data.SqlClient;
using MockTest.Models.models;

namespace MockTest.Repository;

public class ExchangeRepository : IExchangeRepository
{
    private string _connectionString;

    public ExchangeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Country GetCountry(string countryName)
    {
        const string query = "SELECT * FROM Country WHERE Name = @CountryName";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CountryName", countryName);
            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Country
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                reader.Close();
            }
        }
    }

    public Currency GetCurrency(string currencyName)
    {
        const string query = "SELECT * FROM Currency WHERE Name = @CurrencyName";
        
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CurrencyName", currencyName);
            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Currency
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Rate = reader.GetFloat(2)
                    };
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                reader.Close();
            }
        }
    }

    public List<Currency> GetCurrenciesByCountry(string countryName)
    {
        const string query = @"SELECT Curr.Id, Curr.Name, Curr.Rate FROM Currency Curr
                                    JOIN Currency_Country CC ON CC.Currency_Id = Curr.Id
                                    JOIN dbo.Country C on CC.Country_Id = C.Id AND C.Name = @CountryName";

        var currencies = new List<Currency>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CountryName", countryName);
            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        currencies.Add(new Currency
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Rate = reader.GetFloat(2)
                        });
                    }
                }
            }
            finally
            {
                reader.Close();                
            }
        }
        return currencies;
    }

    public List<Country> GetCountriesUsedBy(string currencyName)
    {
        const string query = @"SELECT Country.Id, Country.Name FROM Country  
                                    JOIN dbo.Currency_Country CC on Country.Id = CC.Country_Id 
                                    JOIN dbo.Currency C on C.Id = CC.Currency_Id AND C.Name = @CurrencyName";
        
        var countries = new List<Country>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CurrencyName", currencyName);
            connection.Open();
            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        countries.Add(new Country
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            finally
            {
                reader.Close();
            }
        }
        return countries;
    }

    public void AddCurrency(Currency newCurrency, List<Country> countries)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var insertResult = -1;
                var insertNewCurrencyQuery = "INSERT INTO Currency (Name, Rate) VALUES (@Name, @Rate)";
                SqlCommand insertCommand = new SqlCommand(insertNewCurrencyQuery, connection, transaction);
                insertCommand.Parameters.AddWithValue("@Name", newCurrency.Name);
                insertCommand.Parameters.AddWithValue("@Rate", newCurrency.Rate);

                insertResult = insertCommand.ExecuteNonQuery();
                if (insertResult == -1)
                    throw new ApplicationException("Insert new currency failed.");

                // update the currency (to get the id)
                const string query = "SELECT * FROM Currency WHERE Name = @CurrencyName";
                SqlCommand command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@CurrencyName", newCurrency.Name);
            
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    reader.Read();
                    newCurrency.Id = reader.GetInt32(0);
                }
                finally
                {
                    reader.Close();
                }
                
                foreach (var country in countries)
                    AddCurrency_Country(connection, transaction, newCurrency, country);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    private void AddCurrency_Country(SqlConnection connection, SqlTransaction transaction, Currency currency, Country country)
    {
        var insertPairResult = -1;
        var insertPairQuery = "INSERT INTO Currency_Country (Country_Id, Currency_Id) VALUES (@Country_Id, @Currency_Id)";
        SqlCommand insertCommand = new SqlCommand(insertPairQuery, connection, transaction);
        insertCommand.Parameters.AddWithValue("@Currency_Id", currency.Id);
        insertCommand.Parameters.AddWithValue("@Country_Id", country.Id);
        
        insertPairResult = insertCommand.ExecuteNonQuery();
        if (insertPairResult == -1)
            throw new ApplicationException("Insert new currency-country pair failed.");
    }

    private void DeleteCurrency_Country(SqlConnection connection, SqlTransaction transaction, Currency currency, Country country)
    {
        var deletePairResult = -1;
        var deletePairQuery = "DELETE FROM Currency_Country WHERE Currency_Id = @Currency_Id AND Country_Id = @Country_Id";
        SqlCommand insertCommand = new SqlCommand(deletePairQuery, connection, transaction);
        insertCommand.Parameters.AddWithValue("@Currency_Id", currency.Id);
        insertCommand.Parameters.AddWithValue("@Country_Id", country.Id);
        
        deletePairResult = insertCommand.ExecuteNonQuery();
        if (deletePairResult == -1)
            throw new ApplicationException("Deleting currency-country pair failed.");
    }

    public void UpdateCurrency(Currency oldCurrency, Currency newCurrency, List<Country> newCountries)
    {
        var oldCountries = GetCountriesUsedBy(oldCurrency.Name);

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            
            try
            {
                var updateResult = -1;
                const string updateQuery = "UPDATE Currency SET Name = @Name, Rate = @Rate WHERE Id = @CurrencyId";
                
                SqlCommand command = new SqlCommand(updateQuery, connection, transaction);
                command.Parameters.AddWithValue("@CurrencyId", oldCurrency.Id);
                command.Parameters.AddWithValue("@Name", newCurrency.Name);
                command.Parameters.AddWithValue("@Rate", newCurrency.Rate);
                
                updateResult = command.ExecuteNonQuery();
                if (updateResult == -1)
                    throw new ApplicationException("Updating currency failed.");
                
                // deleting old unused countries
                foreach (var country in oldCountries)
                {
                    if (!newCountries.Contains(country))
                    {
                        DeleteCurrency_Country(connection, transaction, oldCurrency, country);
                    }
                }
                
                // inserting new countries
                foreach (var country in newCountries)
                {
                    if (!oldCountries.Contains(country))
                    {
                        AddCurrency_Country(connection, transaction, oldCurrency, country);
                    }
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}