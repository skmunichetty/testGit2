using System.Net;
using System.Threading.Tasks;
using Jetstar.Jedis.Checkin.ApplicationService.Checkin;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin;
using Jetstar.Jedis.Checkin.ApplicationService.Models.RequestModel.Checkin.CheckinBySegment;
using Jetstar.Jedis.Checkin.ApplicationService.Models.ResponseModel.Checkin.ErrorMessages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace Jetstar.Jedis.Checkin.API.Controllers.Checkin
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("checkin/v{version:apiVersion}")]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _checkinService;

        public CheckinController(ICheckinService checkinService)
        {
            _checkinService = checkinService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public string CheckIn([FromBody, BindRequired] CheckinModel model)
        {
            return string.Empty;
        }

       // Added new line

        /// <summary>
        /// Check in by segment level
        /// </summary>
        /// <param name="pnr">Flight Record Locator. Must be 6 characters.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{pnr}/segment")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(CheckinBySegmentErrorMessageModel), 400)]
        [Produces("application/json")]
        [SwaggerOperation(Description = "Segment level check in.Support any of number of segments and passenger combinations.<br/>" +
                                        "Notice: this api ONLY support PNR grant type authorization!" +
                                        "Check in flow contains 4 steps:<br/>" +
                                        "1.Auto assign seats if passengers hasn't seat<br/>" +
                                        "2.Check in <br/>" +
                                        "3.Add some SSR Code<br/>" +
                                        "4.Add some comments to booking")]
        public async Task<IActionResult> CheckinBySegment([FromRoute, BindRequired]string pnr,
            [FromBody, BindRequired] CheckinBySegmentModel model)
        {
            await _checkinService.CheckinBySegment(pnr, model);

            return StatusCode((int)HttpStatusCode.Created);
        }
    }
}
