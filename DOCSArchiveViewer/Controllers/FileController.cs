using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.ServiceModel.Configuration;
using DOCSArchiveViewer.IIPAX;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Web;

namespace DOCSArchiveViewer.Controllers
{
    public class FileController : ApiController
    {

        private string soapTmpl = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:arc=\"http://www.idainfront.se/schema/archive-2.2\"><soapenv:Header/><soapenv:Body><arc:GetFileContent callerId = \"?\" ><arc:Id>@id</arc:Id></arc:GetFileContent></soapenv:Body></soapenv:Envelope>";

        // GET: api/File
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        private Uri getServiceUri()
        {
            Uri url = null;
            ClientSection c = (ClientSection)ConfigurationManager.GetSection("system.serviceModel/client");
            foreach (ChannelEndpointElement e in c.Endpoints)
            {
                if (e.Name == "ArchivePort")
                {
                    url = e.Address;
                    break;
                }

            }
            return url;
        }

        // GET: api/File/5
        public HttpResponseMessage Get(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        private async Task<HttpResponseMessage> GetFileContent(Uri url, HttpContent content)
        {
            using (HttpClient httpreq = new HttpClient())
            {
                return await httpreq.PostAsync(url, content);
            }
        }

        // POST: api/File
        public async Task<HttpResponseMessage> Post([FromBody]FileQuery query)
        {
            string id = query.id;

            HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.OK);

            Uri url = getServiceUri();

            string reqBody = soapTmpl.Replace("@id", id);

            HttpContent content = new StringContent(reqBody, System.Text.Encoding.UTF8, "application/xml");

            using (HttpClient httpreq = new HttpClient())
            {
                HttpResponseMessage resp = await httpreq.PostAsync(url, content);
                if (resp.IsSuccessStatusCode)
                {
                    MultipartMemoryStreamProvider mprov2 = await resp.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider());
                    MemoryStream ms = new MemoryStream();
                    HttpContent respContent = new StreamContent(ms);
                    foreach (HttpContent c in mprov2.Contents)
                    {
                        Console.WriteLine(c.Headers.ContentType);
                        if (c.Headers.ContentType.ToString().StartsWith("application/octet-stream"))
                        {
                            await c.CopyToAsync(ms);

                            ms.Seek(0, SeekOrigin.Begin);
                            respContent.Headers.ContentType = c.Headers.ContentType;
                            respContent.Headers.ContentLength = ms.Length;

                        }
                        else if (c.Headers.ContentType.ToString().StartsWith("application/xop+xml"))
                        {
                            XmlDocument d = new XmlDocument();
                            d.Load(await c.ReadAsStreamAsync());
                            string fileName = d.DocumentElement.SelectSingleNode("//*[local-name()='File']/*[local-name()='DisplayName']").FirstChild.Value;
                            respContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                            respContent.Headers.ContentDisposition.FileName = Uri.EscapeDataString(fileName);

                        }
                    }
                    result.Content = respContent;
                }
            }
            return result;
        }

        // PUT: api/File/5
        public HttpResponseMessage Put(int id, [FromBody]string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE: api/File/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }
    }

    public class FileQuery
    {
        public string id { get; set; }
    }
}
