using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents data access logic for managing Person entity
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Adds a person object to the datastore
        /// </summary>
        /// <param name="person">Person object to add</param>
        /// <returns>Returns the person object after adding it to the table</returns>
        Task<Person> AddPerson(Person person);

        /// <summary>
        /// Returns all persons in the datastore
        /// </summary>
        /// <returns>List of person objects from the table</returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// Returns person object based on the given personId, otherwise it returns null
        /// </summary>
        /// <param name="personId">PersonId tp search</param>
        /// <returns>Person object or null</returns>
        Task<Person?> GetPersonByPersonID(Guid personId);

        /// <summary>
        /// Returns all person based on the given expression
        /// </summary>
        /// <param name="predicate">LINQ expression to check</param>
        /// <returns>All matching persons with given condition</returns>
        Task<List<Person>> GetFilteredPerson(Expression<Func<Person, bool>> predicate);

        /// <summary>
        /// Deletes a person object based on the given personID
        /// </summary>
        /// <param name="personID">Person ID(guid) to delete</param>
        /// <returns>Returns true, if the deletion is successful, otherwise false</returns>
        Task<bool> DeletePersonByPersonID(Guid personID);

        /// <summary>
        /// Updates a person (person name and other details) based on the given person object
        /// </summary>
        /// <param name="person">Person object to update</param>
        /// <returns>Returns the updated person object</returns>
        Task<Person> UpdatePerson(Person person);
    }
}
