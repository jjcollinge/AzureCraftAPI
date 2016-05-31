using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AzureCraftAPI.Service;
using Newtonsoft.Json;

namespace AzureCraftAPI.Controllers
{
    [Route("api/[controller]")]
    public class PartitionsController : Controller
    {
        // GET: api/values
        [HttpGet]
        public string Get()
        {
            var request = this.HttpContext.Request;
            var lastname = request.Query["lastname"].First();

            AlphabetPartitionService alphabetService = AlphabetPartitionService.GetSingleton();
            var partition = alphabetService.GetPartitionInfo(lastname);

            var json = JsonConvert.SerializeObject(partition);
            return json;
        }
    }
}
