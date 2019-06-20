using System.Web.Mvc;

namespace Demo.Controllers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Veracity.Authentication.OpenIDConnect.AspNet;

    public class HomeController : Controller
    {
        private static HttpClient client;

        public HomeController()
        {
            client = new HttpClient();
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> CallApiAsync()
        {
            var service = new VeracityPlatformService(client);
            var request = new HttpRequestMessage(HttpMethod.Get, "/platform/my/profile");
            request.Headers.Authorization = await service.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            ViewData["Payload"] = await response.Content.ReadAsStringAsync();
            return View();
        }
    }
}