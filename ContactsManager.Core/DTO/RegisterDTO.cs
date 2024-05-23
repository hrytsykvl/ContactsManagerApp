using ContactsManager.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsManager.Core.DTO
{
	public class RegisterDTO
	{
		[Required(ErrorMessage = "Person Name is required")]
		public string PersonName { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Email should be in proper email address format")]
		[Remote(action: "IsEmailAlreadyRegistered", controller: "Account", 
			ErrorMessage = "This email is already in use")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone is required")]
		[RegularExpression("^[0-9]*$", ErrorMessage = "Phone Number should contain numbers only")]
		[DataType(DataType.PhoneNumber)]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required(ErrorMessage = "Comfirm Password is required")]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
		public string ConfirmPassword { get; set; }

		public UserTypeOptions UserType { get; set; } = UserTypeOptions.User;
	}
}
