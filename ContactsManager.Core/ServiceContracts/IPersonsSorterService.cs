using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsSorterService
    {
		/// <summary>
		/// Returns sorted persons based on the sort field and sort order
		/// </summary>
		/// <param name="allPersons">Represents list of persons to sort</param>
		/// <param name="sortBy">Name of the property to sort by</param>
		/// <param name="sortOrder">ASC or DESC</param>
		/// <returns>Returns sorted persons as PersonResponse list</returns>
		Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);
	}
}
