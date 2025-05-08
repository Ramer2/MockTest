using MockTest.Models.models;

namespace MockTest.Models.dto;

public class CountryDTO
{
    public string CountryName { get; set; }
    public Currency[] Currencies { get; set; }

    public CountryDTO()
    {
    }

    public CountryDTO(string countryName, Currency[] currencies)
    {
        CountryName = countryName;
        Currencies = currencies;
    }
}