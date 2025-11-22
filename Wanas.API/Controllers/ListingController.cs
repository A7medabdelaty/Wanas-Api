using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wanas.Application.Interfaces;

namespace Wanas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingController : ControllerBase
    {
        private readonly IListingService _listServ;

        public ListingController(IListingService _listServ)
        {
            this._listServ = _listServ;
        }
    }
}
