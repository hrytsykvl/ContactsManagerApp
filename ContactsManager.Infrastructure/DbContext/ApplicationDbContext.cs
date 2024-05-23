using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using ContactsManager.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
	{
		public ApplicationDbContext()
		{

		}

		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
		}

		public virtual DbSet<Country> Countries { get; set; }
		public virtual DbSet<Person> Persons { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Country>().ToTable("Countries");
			modelBuilder.Entity<Person>().ToTable("Persons");

			// Seed to Countries
			modelBuilder.Entity<Country>().HasData(
								new Country { CountryID = Guid.Parse("8E6B4FC7-CB37-4B49-B8F7-9DEA226A10FE"), CountryName = "USA" },
								new Country { CountryID = Guid.Parse("29771003-917E-47F0-9C78-48BD85F6DE04"), CountryName = "Canada" },
								new Country { CountryID = Guid.Parse("2B3D3201-159C-4B44-AC6E-149F0725B083"), CountryName = "India" },
								new Country { CountryID = Guid.Parse("2294BAE3-B945-4701-8708-ED94D028BA5F"), CountryName = "Australia" },
								new Country { CountryID = Guid.Parse("16CEA231-76EA-4D38-9E63-99532D390185"), CountryName = "UK" }
								);

			// Seed to Persons
			string personsJson = File.ReadAllText(@"E:\Projects C-type lang\C# Projects\CRUDSolution\CRUDExample\persons.json");
			List<Person> persons = JsonSerializer.Deserialize<List<Person>>(personsJson);
			foreach (Person person in persons)
			{
				modelBuilder.Entity<Person>().HasData(person);
			}

			// Fluent API
			modelBuilder.Entity<Person>().Property(temp => temp.TIN)
				.HasColumnName("TaxIdentificationNumber")
				.HasColumnType("varchar(8)")
				.HasDefaultValue("ABC12345");

			//modelBuilder.Entity<Person>()
			//	.HasIndex(temp => temp.TIN).IsUnique();

			modelBuilder.Entity<Person>()
				.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");

			// Table Relations
			//modelBuilder.Entity<Person>(entity =>
			//{
			//	entity.HasOne<Country>(c => c.Country)
			//	.WithMany(p => p.Persons)
			//	.HasForeignKey(p => p.CountryID);
			//});
		}

		public List<Person> sp_GetAllPersons()
		{
			return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
		}

		public int sp_InsertPerson(Person person)
		{
			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("@PersonID", person.PersonID),
				new SqlParameter("@PersonName", person.PersonName),
				new SqlParameter("@Email", person.Email),
				new SqlParameter("@DateOfBirth", person.DateOfBirth),
				new SqlParameter("@Gender", person.Gender),
				new SqlParameter("@CountryID", person.CountryID),
				new SqlParameter("@Address", person.Address),
				new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
			};

			return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
		}
	}
}
