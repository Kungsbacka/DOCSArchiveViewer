using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DOCSArchiveViewer.IIPAX;

namespace DOCSArchiveViewer.Controllers
{
    public class FileController : ApiController
    {
        // GET: api/File
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/File/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/File
        public HttpResponseMessage Post([FromBody]string value)
        {
            ArchivePortTypeClient ar = new ArchivePortTypeClient();

            GetFileContent gfc = new GetFileContent
            {
                Id = value
            };

            GetFileContentResponse response = ar.GetFileContent(gfc);


            HttpResponseMessage result = new HttpResponseMessage();

            if(response.File != null)
            {
                result.StatusCode = HttpStatusCode.OK;

            }


            return result;
            

//            ar.GetFileContent(gfc);
        }

        // PUT: api/File/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/File/5
        public void Delete(int id)
        {
        }
    }
}
