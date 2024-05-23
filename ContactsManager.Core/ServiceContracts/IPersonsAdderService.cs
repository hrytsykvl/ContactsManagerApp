using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Add person to the list
        /// </summary>
        /// <param name="personAddRequest"></param>
        /// <returns></returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);
    }
}
