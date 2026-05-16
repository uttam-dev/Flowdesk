using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
// api/requests               POST
// api/requests               GET
// api/requests/{id}          GET
// api/requests/{id}/approve  POST
// api/requests/{id}/reject   POST
// api/requests/{id}/assign   POST
// api/requests/{id}/status   POST
// api/requests/{id}/comments POST
// api/requests/{id}/comments GET
    
        
    }
}
