﻿using CAMS.API.MiddleWares.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        public ActionResult Get(int code)
        {
            var res = new CustomResponse()
            {
                StatusCode = code,
                Message = "This action is not Found"
            };

            return NotFound(res);
        }
    }
}
