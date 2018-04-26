using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SomeSite.Controllers
{
    [RoutePrefix("def")]
    public class DefaultController : ApiController
    {
        [Route("get")]
        [HttpGet]
        public string Get()
        {
            return "hello";
        }
    }
}
