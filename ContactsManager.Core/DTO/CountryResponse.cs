﻿using System;
using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class that is used as return type for most of CountriesService methods
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }

        // It compares the current object to another object of CountryResponse type and return true, if both values are same;otherwise false
        public override bool Equals(object? obj)
        {
            if(obj == null) return false;

            if(obj.GetType() != typeof(CountryResponse))
            {
                return false;
            }
            CountryResponse countryToCompare = (CountryResponse)obj;
            return CountryID == countryToCompare.CountryID && CountryName == countryToCompare.CountryName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class CountryExtensions
    {
        // Converts from Country object to CountryResponse
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse() { CountryID = country.CountryID, CountryName = country.CountryName };
        }
    }
}
