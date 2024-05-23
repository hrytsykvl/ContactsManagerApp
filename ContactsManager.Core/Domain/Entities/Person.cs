﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    /// <summary>
    /// Person domain class
    /// </summary>
    public class Person
    {
        [Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid PersonID { get; set; }

        [StringLength(40)] // nvarchar(40)
        public string? PersonName { get; set; }

		[StringLength(40)]
		public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }

		[StringLength(10)]
		public string? Gender { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid? CountryID { get; set; }

		[StringLength(200)]
		public string? Address { get; set; }

        public bool ReceiveNewsLetters { get; set; }

        public string? TIN { get; set; }

        [ForeignKey("CountryID")]
        public virtual Country? Country { get; set; }

        public override string ToString()
        {
            return $"PersonID: {PersonID}, PersonName: {PersonName}, Email: {Email}, DateOfBirth : {DateOfBirth?.ToString("MM/dd/yyyy")}, Gender: {Gender}, CountryID: {CountryID}, Country: {Country?.CountryName}, Address: {Address}, Receive News Letters: {ReceiveNewsLetters}"; 
        }
    }
}
