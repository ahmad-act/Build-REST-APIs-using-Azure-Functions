using BookInformationService.BusinessLayer;
using BookInformationService.DTOs;
using BookInformationService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace BookInformationService
{
    public class BookInformationFunction
    {
        private readonly ILogger<BookInformationFunction> _logger;
        private readonly IBookInformationBL _bookInformationBL;

        public BookInformationFunction(ILogger<BookInformationFunction> logger, IBookInformationBL bookInformationBL)
        {
            _logger = logger;
            _bookInformationBL = bookInformationBL;
        }

        [Function("ListBookinformation")]
        public async Task<HttpResponseData> ListBookinformation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bookinformations")] HttpRequestData req)
        {
            _logger.LogInformation("Entered: ListBookinformation()");

            try
            {
                List<BookInformationDisplayDto>? bookInformationsDisplayDto = await _bookInformationBL.GetBookInformations();

                if (bookInformationsDisplayDto == null || bookInformationsDisplayDto.Count == 0)
                {
                    _logger.LogWarning("No book information found.");
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new { Message = "Book informations not found" });
                    return notFoundResponse;
                }

                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                await okResponse.WriteAsJsonAsync(bookInformationsDisplayDto);
                return okResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ListBookinformation(). Exception: {ExceptionMessage}", ex.Message);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred. Please try again later." });
                return errorResponse;
            }
        }

        [Function("GetBookInformation")]
        public async Task<HttpResponseData> GetBookInformation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bookinformations/{id}")] HttpRequestData req, Guid id)
        {
            _logger.LogInformation("Get book with ID {id}.", id);

            try
            {
                BookInformationDisplayDto? bookInformationDisplayDto = await _bookInformationBL.GetBookInformation(id);

                if (bookInformationDisplayDto != null)
                {
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(bookInformationDisplayDto);
                    return response;
                }

                _logger.LogWarning("Book with ID {id} not found.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetBookInformation(). ID: {Id}. Exception: {ExceptionMessage}", id, ex.Message);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred. Please try again later." });
                return errorResponse;
            }
        }

        [Function("CreateBookInformation")]
        public async Task<HttpResponseData> CreateBookInformation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bookinformations")] HttpRequestData req)
        {
            _logger.LogInformation("Adding a new book.");

            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var newBookInformation = JsonConvert.DeserializeObject<BookInformationUpdateDto>(requestBody);

                if (newBookInformation != null)
                {
                    BookInformationDisplayDto? bookInformationDisplayDto = await _bookInformationBL.CreateBookInformation(newBookInformation);

                    var response = req.CreateResponse(HttpStatusCode.Created);
                    await response.WriteAsJsonAsync(bookInformationDisplayDto);
                    return response;
                }

                _logger.LogWarning("Invalid book information provided. Request body: {RequestBody}", requestBody);
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { Message = "Invalid book information provided." });
                return badRequestResponse;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "Cosmos DB error while creating book information. Status code: {StatusCode}, Activity ID: {ActivityId}, Error message: {ErrorMessage}",
                    cosmosEx.StatusCode, cosmosEx.ActivityId, cosmosEx.Message);

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Message = "Error occurred while interacting with the database." });
                return errorResponse;
            }
            catch (JsonSerializationException jsex)
            {
                _logger.LogError(jsex, "Failed to deserialize request body. Request body: {RequestBody}", await req.ReadAsStringAsync());
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { Message = "Invalid request payload." });
                return badRequestResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CreateBookInformation(). Exception: {ExceptionMessage}", ex.Message);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred. Please try again later." });
                return errorResponse;
            }
        }

        [Function("UpdateBookInformation")]
        public async Task<HttpResponseData> UpdateBookInformation([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "bookinformations/{id}")] HttpRequestData req, Guid id)
        {
            _logger.LogInformation("Updating book with ID {id}.", id);

            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var updatedBookInformation = JsonConvert.DeserializeObject<BookInformationUpdateDto>(requestBody);

                if (updatedBookInformation != null)
                {
                    BookInformationDisplayDto? existingBookInformation = await _bookInformationBL.UpdateBookInformation(id, updatedBookInformation);

                    if (existingBookInformation != null)
                    {
                        var response = req.CreateResponse(HttpStatusCode.OK);
                        await response.WriteAsJsonAsync(existingBookInformation);
                        return response;
                    }

                    _logger.LogWarning("Book with ID {id} not found for update.", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _logger.LogWarning("Invalid book information provided for update. Request body: {RequestBody}", requestBody);
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { Message = "Invalid book information provided." });
                return badRequestResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateBookInformation(). ID: {Id}. Exception: {ExceptionMessage}", id, ex.Message);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred. Please try again later." });
                return errorResponse;
            }
        }

        [Function("DeleteBookInformation")]
        public async Task<HttpResponseData> DeleteBookInformation([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "bookinformations/{id}")] HttpRequestData req, Guid id)
        {
            _logger.LogInformation("Deleting book with ID {id}.", id);

            try
            {
                BookInformationDisplayDto bookInformationDisplayDto = await _bookInformationBL.DeleteBookInformation(id);

                if (bookInformationDisplayDto != null)
                {
                    var response = req.CreateResponse(HttpStatusCode.NoContent);
                    return response;
                }

                _logger.LogWarning("Book with ID {id} not found for deletion.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteBookInformation(). ID: {Id}. Exception: {ExceptionMessage}", id, ex.Message);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = "An unexpected error occurred. Please try again later." });
                return errorResponse;
            }
        }
    }
}
