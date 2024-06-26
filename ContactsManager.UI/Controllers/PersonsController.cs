﻿using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilter;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
	[Route("[controller]")]
	//[TypeFilter(typeof(ResponseHeaderActionFilter),
	//Arguments = new object[] { "My-Key-From-Controller", "My-Value-From-Controller", 3 }, Order = 3)]
	[ResponseHeaderFilterFactory("My-Key-From-Controller", "My-Value-From-Controller", 3)]
	[TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonAlwaysRunResultFilter))]
    public class PersonsController : Controller
	{
		private readonly IPersonsGetterService _personsGetterService;
		private readonly IPersonsAdderService _personsAdderService;
		private readonly IPersonsSorterService _personsSorterService;
		private readonly IPersonsDeleterService _personsDeleterService;
		private readonly IPersonsUpdaterService _personsUpdaterService;

		private readonly ICountriesService _countriesService;
		private readonly ILogger<PersonsController> _logger;

		public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService,
			IPersonsSorterService personsSorterService,
			IPersonsDeleterService personsDeleterService, IPersonsUpdaterService personsUpdaterService,
			ICountriesService countriesService, ILogger<PersonsController> logger)
		{
			_personsAdderService = personsAdderService;
			_personsSorterService = personsSorterService;
			_personsDeleterService = personsDeleterService;
			_personsUpdaterService = personsUpdaterService;
			_personsGetterService = personsGetterService;

			_countriesService = countriesService;
			_logger = logger;
		}

		[Route("[action]")]
		[Route("/")]
		[TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
		// [TypeFilter(typeof(ResponseHeaderActionFilter),
			// Arguments = new object[] { "My-Key-From-Action", "My-Value-From-Action", 1 }, Order = 1)]
		[ResponseHeaderFilterFactory("My-Key-From-Action", "My-Value-From-Action", 1)]
		[ServiceFilter(typeof(PersonsListResultFilter))]
		[SkipFilter]
		public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName),
			SortOrderOptions sortOrder = SortOrderOptions.ASC)
		{
			_logger.LogInformation("Index action method of PersonsController called");

			_logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");
			// Search
			// ViewBag in PersonsListActionFilter
			List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

			// Sort
			List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

			return View(sortedPersons);
		}

		[Route("[action]")]
		[HttpGet]
        [ResponseHeaderFilterFactory("my-key", "my-value", 4)]
        public async Task<IActionResult> Create()
		{
			List<CountryResponse> countries = await _countriesService.GetAllCountries();
			ViewBag.Countries = countries.Select(temp =>
				new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });
			// <option value="1">Vlad</option>);


			return View();
		}

		[HttpPost]
		[Route("[action]")]
		[TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
		[TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false })]
		public async Task<IActionResult> Create(PersonAddRequest personRequest)
		{
			// call the service method
			PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);

			return RedirectToAction("Index", "Persons");
		}

		[HttpGet]
		[Route("[action]/{personID}")] // Eg:persons/edit/1
		[TypeFilter(typeof(TokenResultFilter))]
		public async Task<IActionResult> Edit(Guid personID)
		{
			PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
			if(personResponse == null)
			{
				return RedirectToAction("Index");
			}
			PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View(personUpdateRequest);
		}

		[HttpPost]
		[Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
		[TypeFilter(typeof(TokenAuthorizationFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
		{
			PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

			if (personResponse == null)
			{
				return RedirectToAction("Index");
			}

			PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
			return RedirectToAction("Index");
		}

		[HttpGet]
		[Route("[action]/{personID}")]
		public async Task<IActionResult> Delete(Guid? personID)
		{
			PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);

			if(personResponse == null)
			{
				return RedirectToAction("Index");
			}

			return View(personResponse);
		}

		[HttpPost]
		[Route("[action]/{personID}")]
		public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
		{
			PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personUpdateRequest.PersonID);
			if(personResponse == null)
			{
				return RedirectToAction("Index");
			}
			await _personsDeleterService.DeletePerson(personUpdateRequest.PersonID);
			return RedirectToAction("Index");
		}

		[Route("PersonsPDF")]
		public async Task<IActionResult> PersonsPDF()
		{
			// Get list of persons
			List<PersonResponse> persons = await _personsGetterService.GetAllPersons();

			// Return view as PDF
			return new ViewAsPdf("PersonsPDF", persons, ViewData)
			{
				PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
				PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
			};
		}

		[Route("PersonsCSV")]
		public async Task<IActionResult> PersonsCSV()
		{
			MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();
			return File(memoryStream, "application/octet-stream", "persons.csv");
		}

		[Route("PersonsExcel")]
		public async Task<IActionResult> PersonsExcel()
		{
			MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
			return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
		}
	}
}
