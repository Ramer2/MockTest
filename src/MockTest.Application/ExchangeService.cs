using System.Text.Json;
using System.Text.Json.Nodes;
using MockTest.Models.dto;
using MockTest.Models.models;
using MockTest.Repository;

namespace MockTest.Application;

public class ExchangeService : IExchangeService
{
    private IExchangeRepository _exchangeRepository;

    public ExchangeService(IExchangeRepository exchangeRepository)
    {
        _exchangeRepository = exchangeRepository;
    }

    public bool AddExchange(JsonNode? json)
    {
        var countriesString = json["countries"].ToString();
        if (countriesString == null)
        {
            throw new ArgumentException("No \"countries\" parameter found");
        }

        if (string.IsNullOrEmpty(countriesString))
        {
            throw new ArgumentException("No countries found in the list");
        }
        
        var countryNames = countriesString.Trim().Replace(" ", "").Split(','); 
        // debug
        // foreach (var countryName in countryNames) Console.WriteLine($"Country: {countryName}");
            
        var countries = new List<Country>();
        foreach (var countryName in countryNames)
        {
            var newCountry = _exchangeRepository.GetCountry(countryName);
            if (newCountry == null) 
                throw new ApplicationException($"No such country as '{countryName}' was found.");
            
            countries.Add(newCountry);
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        Currency? currency = null;
        try
        {
            currency = JsonSerializer.Deserialize<Currency>(json, options);
        }
        catch
        {
            throw new ArgumentException("JSON deserailzation failed. Seek help.");
        }

        if (currency == null)
            throw new ArgumentException("JSON deserailzation failed. Seek help.");

        var oldCurrency = _exchangeRepository.GetCurrency(currency.Name);
        if (oldCurrency == null)
            _exchangeRepository.AddCurrency(currency, countries);
        else 
            _exchangeRepository.UpdateCurrency(oldCurrency, currency, countries);
        
        return true;
    }

    public CountryDTO SearchExchangeByCountry(JsonNode? json)
    {
        var nameOfCountry = json["name"].ToString();
        return new CountryDTO
        {
            CountryName = nameOfCountry,
            Currencies = _exchangeRepository.GetCurrenciesByCountry(nameOfCountry).ToArray()
        };
    }

    public Country[] SearchExchangeByCurrency(JsonNode? json)
    {
        var nameOfCurrency = json["name"].ToString();
        return _exchangeRepository.GetCountriesUsedBy(nameOfCurrency).ToArray();
    }
}