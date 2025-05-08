using MockTest.Models.models;

namespace MockTest.Repository;

public interface IExchangeRepository
{
    public Country GetCountry(string countryName);

    public Currency GetCurrency(string currencyName);
    
    public List<Currency> GetCurrenciesByCountry(string countryName);

    public List<Country> GetCountriesUsedBy(string currencyName);
    
    public void AddCurrency(Currency newCurrency, List<Country> countries);
    
    public void UpdateCurrency(Currency oldCurrency, Currency newCurrency, List<Country> countries);
}