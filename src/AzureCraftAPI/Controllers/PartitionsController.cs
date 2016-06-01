using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AzureCraftAPI.Service;
using Newtonsoft.Json;
using Microsoft.AspNet.Cors;
using AzureCraftAPI.Model;
using System.Diagnostics;

namespace AzureCraftAPI.Controllers
{
    [Route("api/[controller]"), EnableCors("MyPolicy")]
    public class PartitionsController : Controller
    {
        // GET: api/values
        [HttpGet, EnableCors("MyPolicy")]
        public string Get()
        {
            var request = this.HttpContext.Request;
            var lastname = request.Query["lastname"].First();
            Debug.WriteLine($"Name: {lastname}");

            AlphabetPartitionService alphabetService = AlphabetPartitionService.GetSingleton();
            var partition = alphabetService.GetPartitionInfo(lastname);

            var namedPartiton = new NamedPartition()
            {
                Partition = partition,
                Name = lastname
            };

            var json = JsonConvert.SerializeObject(namedPartiton);

            Debug.WriteLine($"Json:\n {json}");
            return json;
        }
    }
}
