using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsDeleterService
    {
        /// <summary>
        /// Deletes person based on the given person id
        /// </summary>
        /// <param name="personID">PersonID to delete</param>
        /// <returns>Returns true, if deletion is successfull, otherwise false</returns>
        Task<bool> DeletePerson(Guid? personID);
    }
}
