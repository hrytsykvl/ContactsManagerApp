using System;
using System.Collections.Generic;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using Services;
using Moq;
using RepositoryContracts;
using FluentAssertions;
using AutoFixture;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly Fixture _fixture;

        public CountriesServiceTest()
        {
            _fixture = new Fixture();
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
            _countriesService = new CountriesService(_countriesRepository);
        }

        #region AddCountry
        // When Country is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        {
            // Arrange
            CountryAddRequest request = null;

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                // Act
                await _countriesService.AddCountry(request);
            });
        }

        // When CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Act
                await _countriesService.AddCountry(request);
            });
        }

        // When CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest? request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest? request2 = new CountryAddRequest() { CountryName = "USA" };

            Country country = request1.ToCountry();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName("USA"))
                .ReturnsAsync(country);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        // When you supply proper country name, it should add the country to existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();
            Country country = country_request.ToCountry();
            CountryResponse country_response = country.ToCountryResponse();

            _countriesRepositoryMock
             .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
             .ReturnsAsync(country);

            _countriesRepositoryMock
             .Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
             .ReturnsAsync(null as Country);


            //Act
            CountryResponse country_from_add_country = await _countriesService.AddCountry(country_request);

            country.CountryID = country_from_add_country.CountryID;
            country_response.CountryID = country_from_add_country.CountryID;

            //Assert
            country_from_add_country.CountryID.Should().NotBe(Guid.Empty);
            country_from_add_country.Should().BeEquivalentTo(country_response);
        }
        #endregion

        #region GetAllCountries
        [Fact]
        // The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_ToBeEmptyList()
        {
            //Arrange
            List<Country> country_empty_list = new List<Country>();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(country_empty_list);

            //Act
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            //Assert
            actual_country_response_list.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCountries_ShouldHaveFewCountries()
        {
            List<Country> country_list = new List<Country>() {
            _fixture.Build<Country>()
            .With(temp => temp.Persons, null as List<Person>).Create(),
            _fixture.Build<Country>()
            .With(temp => temp.Persons, null as List<Person>).Create()
            };

            List<CountryResponse> country_response_list = country_list.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(country_list);

            //Act
            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //Assert
            actualCountryResponseList.Should().BeEquivalentTo(country_response_list);
        }
        #endregion

        #region GetCountryByCountryID

        [Fact]
        //If we supply null as CountryID , we should return null as CountryResponse
        public async Task GetCountryByCountryID_NullCountryID_ToBeNull()
        {
            //Arrange
            Guid? countryID = null;

            _countriesRepositoryMock
             .Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
             .ReturnsAsync(null as Country);

            //Act
            CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryID(countryID);


            //Assert
            country_response_from_get_method.Should().BeNull();
        }

        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID_ToBeSuccessful()
        {
            //Arrange
            Country country = _fixture.Build<Country>()
              .With(temp => temp.Persons, null as List<Person>)
              .Create();
            CountryResponse country_response = country.ToCountryResponse();

            _countriesRepositoryMock
             .Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
             .ReturnsAsync(country);

            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country.CountryID);


            //Assert
            country_response_from_get.Should().Be(country_response);
        }
        #endregion
    }
}
