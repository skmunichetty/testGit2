using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Jetstar.Jedis.Checkin.ApplicationService.Checkin.Mobile;
using Jetstar.Jedis.Checkin.ApplicationService.Checkin.SendBoardingPass;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.BoardingPass;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.BoardingPassStatus;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.SendBoardingPass;
using Jetstar.Jedis.Checkin.ApplicationService.Models.ResponseModel.Checkin.BoardingPass;
using Jetstar.Jedis.Checkin.ApplicationService.Models.ResponseModel.Checkin.BoardingPass.ErrorMessages;
using Jetstar.Jedis.Checkin.ApplicationService.Models.ResponseModel.Checkin.BoardingPassAvailable;
using Jetstar.Jedis.Checkin.QueryService.Checkin.BoardingPass;
using Jetstar.Jedis.Common.ErrorEntryProviders;
using Jetstar.Jedis.Common.ErrorEntryProviders.Nsk.Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace Jetstar.Jedis.Checkin.API.Controllers.Checkin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/checkin")]
    public class BoardingPassController : ControllerBase
    {
        private readonly IMobileBoardingPassService _boardingPassService;
        private readonly ISendBoardingPassService _sendBoardingPassService;
        private readonly IBoardingPassQueryService _boardingPassQueryService;

        public BoardingPassController(IMobileBoardingPassService boardingPassService,
            ISendBoardingPassService sendBoardingPassService,
            IBoardingPassQueryService boardingPassQueryService)
        {
            _boardingPassService = boardingPassService;
            _sendBoardingPassService = sendBoardingPassService;
            _boardingPassQueryService = boardingPassQueryService;
        }

        [Route("{pnr}/mobile-boarding-pass")]
        [HttpPost]
        [SwaggerOperation(Description = "This API returns all available boarding passes, for all passengers, on a given booking. Note that if some passengers or segments are not eligible for mobile boarding passes they are simply not included, the response does not indicate if mobile boarding passes were not issued.")]
        [ProducesResponseType(typeof(BoardingPassResponse), 200)]
        [ProducesResponseType(typeof(BoardingPassErrorMessageModel), 400)]
        public async Task<BoardingPassResponse> GenerateBoardingPasseResponse([BindRequired] string pnr,
            [FromBody] BoardingPassRequestModelV1 request)
        {
            if (request == null)
            {
                throw new JedisErrorMessageException(
                    new List<ErrorEntry>
                    {
                        RetrieveBookingErrorEntryProvider.EmailAndNameEmpty
                    }, HttpStatusCode.BadRequest);
            }

            var mobileBoardingPassResponsesList = await _boardingPassService.GetMobileBoardingPasses(
                pnr,
                request.MapToBookingIdentifierQueryModel());

            var response = mobileBoardingPassResponsesList.MapToBoardingPassResponse();

            return response;
        }

        /// <param name="pnr">Flight Record Locator. Must be 6 characters.</param>
        [Route("{pnr}/boarding-pass/send")]
        [HttpPost]
        [ProducesResponseType(202)]
        [ProducesResponseType(typeof(SendBoardingPassErrorMessageModel), 400)]
        public async Task<SendBoardingPassResponse> SendBoardingPass(
            [BindRequired][FromRoute]string pnr,
            [FromBody]SendBoardingPassRequestModel request)
        {
            return await _sendBoardingPassService.SendBoardingPass(pnr, request);
        }

        /// <param name="pnr">Flight Record Locator. Must be 6 characters.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BoardingPassAvailableResponse), 200)]
        [ProducesResponseType(typeof(BoardingPassStatusErrorMessageModel), 400)]
        [Route("{pnr}/boarding-pass/availability")]
        public async Task<BoardingPassAvailableResponse> GetBoardingPassAvailable(
            [BindRequired][FromRoute]string pnr,
            [FromBody]BoardingPassAvailableQueryModel request)
        {
            var result = await _boardingPassQueryService.GetBoardingPassAvailable(pnr, request);
            return result.ToResponse();
        }
    }
}
