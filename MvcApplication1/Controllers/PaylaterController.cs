using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcApplication1.Controllers
{
    public class PaylaterController : ApiController
    {
        [HttpGet]
        [HttpPost]
        public void Handle(string uri)
        {
            var hello = uri;
        }
    }
}
