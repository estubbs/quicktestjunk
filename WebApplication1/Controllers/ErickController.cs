using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class ErickController : ApiController
    {
        public string Get()
        {

            throw new InvalidOperationException("bad api");

            string data = "hello world";
            return data;
            
        }
    }
}
