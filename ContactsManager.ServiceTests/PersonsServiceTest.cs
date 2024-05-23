using ServiceContracts;
using System;
using Xunit;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;
using Serilog;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsGetterService _personGetterService;
        private readonly IPersonsAdderService _personAdderService;
        private readonly IPersonsUpdaterService _personUpdaterService;
        private readonly IPersonsDeleterService _personDeleterService;
        private readonly IPersonsSorterService _personSorterService;

        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerGetterMock = new Mock<ILogger<PersonsGetterService>>();
            var loggerAdderMock = new Mock<ILogger<PersonsAdderService>>();
            var loggerDeleterMock = new Mock<ILogger<PersonsDeleterService>>();
            var loggerSorterMock = new Mock<ILogger<PersonsSorterService>>();
            var loggerUpdaterMock = new Mock<ILogger<PersonsUpdaterService>>();

            _personGetterService = new PersonsGetterService(_personsRepository, loggerGetterMock.Object, diagnosticContextMock.Object);
            _personAdderService = new PersonsAdderService(_personsRepository, loggerAdderMock.Object, diagnosticContextMock.Object);
            _personDeleterService = new PersonsDeleterService(_personsRepository, loggerDeleterMock.Object, diagnosticContextMock.Object);
            _personSorterService = new PersonsSorterService(_personsRepository, loggerSorterMock.Object, diagnosticContextMock.Object);
            _personUpdaterService = new PersonsUpdaterService(_personsRepository, loggerUpdaterMock.Object, diagnosticContextMock.Object);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        // When we supply null value it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            PersonAddRequest? personAddRequest = null;

            Func<Task> action = async () =>
            {
                await _personAdderService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        // When we supply null value as PersonName it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            PersonAddRequest? personAddRequest =
                _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();
            // When PersonRepository.AddPerson is called with person object, it should return the same person object
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            Func<Task> action = async () =>
            {
                await _personAdderService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        // When we supply proper person details, it should insert the person into the list
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@example.com")
                .Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse person_repsonse_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_add = await _personAdderService.AddPerson(personAddRequest);
            person_repsonse_expected.PersonID = person_response_from_add.PersonID;


            // Assert
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_repsonse_expected);
        }

        #endregion

        #region GetPersonPersonID
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            // Arrange
            Guid? personID = null;

            // Act
            PersonResponse? person_response_from_get = await _personGetterService.GetPersonByPersonID(personID);

            // Assert
            // Assert.Null(person_response_from_get);
            person_response_from_get.Should().BeNull();
        }

        // Supply valid person id
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
        {
            // Arrange
            Person person =
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create();
            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            // Act
            PersonResponse? person_response_from_get = await _personGetterService.GetPersonByPersonID(person.PersonID);

            // Assert
            // Assert.Equal(person_response_from_add, person_response_from_get);
            person_response_from_get.Should().Be(person_response_expected);
        }
        #endregion

        #region GetAllPersons

        // Should return empty list by default
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList()
        {
            var persons = new List<Person>();
            _personsRepositoryMock
                .Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
            List<PersonResponse> persons_from_get = await _personGetterService.GetAllPersons();

            // Assert
            // Assert.Empty(persons_from_get);
            persons_from_get.Should().BeEmpty();
        }

        // Add few persons, and it should return the same persons we added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> persons_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

           
            // print persons_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in persons_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> persons_response_list_from_get = await _personGetterService.GetAllPersons();

            // print persons_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_response_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            // Assert
            persons_response_list_from_get.Should().BeEquivalentTo(persons_response_list_expected);
        }
        #endregion

        #region GetFilteredPersons
        //If the search text is empty and search by is PersonName, it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> persons_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            // print persons_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in persons_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPerson(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);
            // Act
            List<PersonResponse> persons_list_from_search = await _personGetterService.GetFilteredPersons(nameof(Person.PersonName), "");

            // print persons_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            // Assert
            persons_list_from_search.Should().BeEquivalentTo(persons_response_list_expected);
        }

        // We will add few persons and search by PersonName. It should return the persons with the given name
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> persons_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            // print persons_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in persons_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPerson(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);
            // Act
            List<PersonResponse> persons_list_from_search = await _personGetterService.GetFilteredPersons(nameof(Person.PersonName), "sa");

            // print persons_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            // Assert
            persons_list_from_search.Should().BeEquivalentTo(persons_response_list_expected);
        }
        #endregion

        #region GetSortedPersons
        // When we sort based on PersonName in descending order, it should return the persons sorted by PersonName in descending order
        [Fact]
        public async Task GetSortedPersons_SortByPersonName_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> persons_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock
                .Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);


            // print persons_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in persons_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            List<PersonResponse> allPersons = await _personGetterService.GetAllPersons();
            // Act
            List<PersonResponse> persons_response_list_from_sort = await _personSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName),
                SortOrderOptions.DESC);

            // print persons_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_response_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            // persons_response_list_from_add = persons_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();

            // Assert
            // persons_response_list_from_sort.Should().BeEquivalentTo(persons_response_list_from_add);
            persons_response_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
        }
        #endregion

        #region UpdatePerson
        // When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request = null;

            // Act
            Func<Task> action = async () =>
            {

                await _personUpdaterService.UpdatePerson(person_update_request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        // When we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request =
                _fixture.Build<PersonUpdateRequest>()
                .Create();

            // Act
            Func<Task> action = async () =>
            {
                await _personUpdaterService.UpdatePerson(person_update_request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // When person name is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            Person person =
                _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "example@gmail.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();

            // Act
            Func<Task> action = async () =>
            {
                await _personUpdaterService.UpdatePerson(person_update_request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // Add new person and update name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation_ToBeSuccessful()
        {
            // Arrange
            
            Person person =
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example@gmail.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

            _personsRepositoryMock
                .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            _personsRepositoryMock
                .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_update = await _personUpdaterService.UpdatePerson(person_update_request);


            // Assert
            // Assert.Equal(person_response_from_get, person_response_from_update);
            person_response_from_update.Should().Be(person_response_expected);
        }
        #endregion

        #region DeletePerson
        // If you supply valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            Person person =
                _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Smith")
                .With(temp => temp.Email, "example@gmail.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Female")
                .Create();

            _personsRepositoryMock
                .Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            // Act
            bool isDeleted = await _personDeleterService.DeletePerson(person.PersonID);

            // Assert
            // Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        // If you supply invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            // Act
            bool isDeleted = await _personDeleterService.DeletePerson(Guid.NewGuid());

            // Assert
            // Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
