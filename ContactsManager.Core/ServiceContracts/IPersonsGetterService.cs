using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsGetterService
    {
        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns></returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns person object based on the person id
        /// </summary>
        /// <param name="personID">Person id to search</param>
        /// <returns></returns>
        Task<PersonResponse?> GetPersonByPersonID(Guid? personID);

        /// <summary>
        /// Returns filtered persons based on the search criteria
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">Search string to search</param>
        /// <returns>Returns all matching persons based on the given search field ans search string</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

		/// <summary>
		/// Returns persons as CSV file
		/// </summary>
		/// <returns>Returns the memory stream with CSV data of persons</returns>
		Task<MemoryStream> GetPersonsCSV();

        /// <summary>
        /// Returns persons as Excel
        /// </summary>
        /// <returns>Return memory stream with Excel data of persons</returns>
        Task<MemoryStream> GetPersonsExcel();
    }
}
