using Microsoft.AspNetCore.Mvc;

namespace AudioTextGeneration.src.main.Controllers
{
    [ApiController]
    [Route("Audio")]
    public class AudioController : Controller
    {
        [HttpPost("Send")]
        public IActionResult ReceiveAudio()
        {
            return Content("test");
        }

        // [HttpPost("Send")]
        // public String Test() {
        //     return "I am testing";
        // }
    }
}