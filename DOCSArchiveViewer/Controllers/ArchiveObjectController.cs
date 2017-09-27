using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DOCSArchiveViewer.IIPAX;

namespace DOCSArchiveViewer.Controllers
{
    public class ArchiveObjectController : ApiController
    {
        // GET: api/ArchiveObject
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // GET: api/ArchiveObject/5
        public HttpResponseMessage Get(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // POST: api/ArchiveObject
        public Dictionary<string, object> Post([FromBody]ArchiveObjectQuery id)
        {
            ArchivePortTypeClient ar = new ArchivePortTypeClient();
            GetAip request = new GetAip();

            request.Id = id.id;
            GetAipResponse response = ar.GetAip(request);
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach(NameValue a in response.ArchiveObject.Attribute)
            {
                if(a.Value.Length > 0)
                    result.Add(a.name, a.Value[0]);
            }

            if (response.ArchiveObject.Items != null)
            {
                List<object> items = new List<object>();
                foreach (IIPAX.Object i in response.ArchiveObject.Items)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item.Add("object_type", i.ObjectType);
// This is code to just get the number of files
                    int numFiles = 0;
                    try
                    {
                        numFiles = ((IIPAX.Document)i).File.Length;
                    }
                    catch
                    {
                        numFiles = 0;
                    }
                    item.Add("Files", numFiles);
// Below is code to get the actual files                    
/*                    try // Accessing IIPAX.Document.File throws exception when no files are available.
                    {
                        List<object> files = new List<object>();
                        foreach (IIPAX.File f in ((IIPAX.Document)i).File)
                        {
                            Dictionary<string, string> file = new Dictionary<string, string>();
                            file.Add("id", f.Id);
                            file.Add("display_name", f.DisplayName);
                            files.Add(file);
                            //Console.Write(f.DisplayName);
                        }
                        item.Add("Files", files);
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        // Normal behaviour, apparently.
                    } */
                    foreach (NameValue n in i.Attribute)
                    {
                        item.Add(n.name, n.Value[0]);
                    }
                    items.Add(item);
                }
                result.Add("items", items);
            }
            return result;
        }

        // PUT: api/ArchiveObject/5
        public HttpResponseMessage Put(int id, [FromBody]string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE: api/ArchiveObject/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }
    }

    public class ArchiveObjectQuery
    {
        public string id { get; set; }
    }

}
