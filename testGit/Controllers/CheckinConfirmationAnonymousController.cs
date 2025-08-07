#nullable enable
using System.Net;
using System.Threading.Tasks;
using Jetstar.Jedis.Checkin.ApplicationService.Checkin;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.CheckinBySegment;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.CheckinCredential;
using Jetstar.Jedis.Checkin.ApplicationService.Models.ResponseModel.Checkin.ErrorMessages;
using Jetstar.Jedis.Common.Constants;
using Jetstar.Jedis.Common.Encryption;
using Jetstar.Jedis.Common.Extensions;
using Jetstar.Jedis.Common.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace Jetstar.Jedis.Checkin.API.Controllers.Checkin
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("v{version:apiVersion}/checkin")]
    public class CheckinConfirmationAnonymousController : ControllerBase
    {
        private readonly ICheckinService _checkinService;

        private readonly IEncryptionService _encryptionService;

        public CheckinConfirmationAnonymousController(ICheckinService checkinService, IEncryptionService encryptionService)
        {
            _checkinService = checkinService;
            _encryptionService = encryptionService;
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public string CheckIn([FromBody, BindRequired] CheckinModel model)
        {
            return string.Empty;
        }

        /// <summary>
        /// Check in by segment level
        /// </summary>
        /// <param name="pnr">Flight Record Locator. Must be 6 characters.</param>
        /// <returns></returns>
        [NotFinishYet]
        [AllowAnonymous]
        [HttpPost]
        [Route("{pnr}/segment")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(CheckinBySegmentErrorMessageModel), 400)]
        [Produces("application/json")]
        [SwaggerOperation(Description = "Segment level check in.Support any of number of segments and passenger combinations.<br/>" +
                                        "Check in flow contains 4 steps:<br/>" +
                                        "1.Auto assign seats if passengers hasn't seat<br/>" +
                                        "2.Check in <br/>" +
                                        "3.Add some SSR Code<br/>" +
                                        "4.Add some comments to booking")]
        public async Task<IActionResult> CheckinBySegment([FromRoute, BindRequired]string pnr,
            [FromBody, BindRequired] CheckinBySegmentModel query)
        {
            HttpContext.Request.Headers.Add(Constants.AnonymousUserSessionId, Common.Utility.Utilities.GenerateSessionIdInBase64Format());
            if (query.BookingCredential.IsNotNullOrEmpty())
            {
                var decodedModel = await _encryptionService.ValidateAndDecryptBookingCredential(
                    query.BookingCredential,
                    decryptedBookingCredential => BookingCredentialToQuery(decryptedBookingCredential, query));

                await _checkinService.CheckinBySegment(pnr, decodedModel);
                return StatusCode((int)HttpStatusCode.Created);
            }

            await _checkinService.CheckinBySegment(pnr, query);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [NonAction]
        private CheckinBySegmentModel BookingCredentialToQuery(BookingCredential decryptedBookingCredential, CheckinBySegmentModel query)
        {
            var overriddenCheckinBySegmentModel = query with
            {
                FirstName = decryptedBookingCredential.Firstname,
                LastName = decryptedBookingCredential.Lastname,
                Email = decryptedBookingCredential.Email
            };
            return overriddenCheckinBySegmentModel;
        }
    }
}
