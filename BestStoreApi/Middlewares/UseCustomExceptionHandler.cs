using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using DigitalStore.Core.DTOs;
using DigitalStore.Service.Exceptions;
using SendGrid.Helpers.Errors.Model;

namespace DigitalStore.API.Middlewares
{
    public static class UseCustomExceptionHandler
    {
        public static void UseCustomExpection(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(config =>
            {
                config.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

                    int statusCode;
                    string errorMessage;

                    if (exceptionFeature != null)
                    {
                        statusCode = exceptionFeature.Error switch
                        {
                            ClientSideException => 400,
                            NotFoundException => 404,
                            InternalServerErrorException => 500,
                            InvalidTokenException => 401,
                            UnauthorizedActionException => 403,
                            _ => 500
                        };

                        errorMessage = exceptionFeature.Error.Message;
                    }
                    else
                    {
                        statusCode = 500;
                        errorMessage = "An unexpected error occurred.";
                    }

                    context.Response.StatusCode = statusCode;

                    var response = CustomResponseDto<NoContentDto>.Fail(statusCode, errorMessage);

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                });
            });
        }
    }
}
