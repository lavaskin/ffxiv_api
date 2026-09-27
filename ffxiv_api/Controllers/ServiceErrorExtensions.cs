using ffxiv_api.Models.DTOs;
using ffxiv_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ffxiv_api.Controllers;

public static class ServiceErrorExtensions
{
	public static ActionResult ToActionResult(this ServiceError error)
	{
		var body = new ErrorResponse(error.Message);

		return error.Kind switch
		{
			ServiceErrorKind.NotFound => new NotFoundObjectResult(body),
			ServiceErrorKind.Conflict => new ConflictObjectResult(body),
			_ => new BadRequestObjectResult(body),
		};
	}
}
