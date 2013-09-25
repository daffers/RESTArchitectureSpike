using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Scratch
{
    [TestFixture]
    public class TestFancyRouting
    {
        [Test]
        public void PushAnObject()
        {

            var payload = new JObject();
            payload.Add("Name", "Dafydd");

            using (var client = new HttpClient())
            {
                client.PostAsync<HttpResponseMessage>("http://localhost:55422/paylater/hello", payload, new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }
    }

    public class SomeData
    {
        public string Name { get; set; }
    }
}
