﻿using CAMS.API.MiddleWares.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Extensions
{
    public static class ValdationErrorExtension
    {
        public static IServiceCollection Configure(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {

                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var Errors = context.ModelState.Where(x => x.Value!.Errors.Count > 0)
                                       .SelectMany(P => P.Value!.Errors)
                                       .Select(P => P.ErrorMessage)
                                       .ToList();

                    var validationErrorRes = new ValidationErrorCustomResponse()
                    {
                        Errors = Errors
                    };

                    return new BadRequestObjectResult(validationErrorRes);  // used BadRequest because ValidationError is BadRequest

                };
            });

            return services;
        }
    }
}
