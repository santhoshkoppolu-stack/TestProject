using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Logic
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            return result switch
            {
                Created<Message> created => controller.Created(
                    controller.Request.Path,
                    created.Value
                ),

                Updated => controller.Ok(),

                Deleted => controller.NoContent(),

                NotFound nf => controller.NotFound(new { message = nf.Message }),

                Conflict c => controller.Conflict(new { message = c.Message }),

                ValidationError v => controller.BadRequest(v.Errors),

                Success => controller.Ok(),

                _ => controller.StatusCode(500, "Unknown result type")
            };
        }
    }
}
