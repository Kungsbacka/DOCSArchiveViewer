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
        public async Task<HttpResponseMessage> Get(string id)
        {
            HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.NotFound);

            Uri url = getServiceUri();

            string reqBody = soapTmpl.Replace("@id", id);

            HttpContent content = new StringContent(reqBody, System.Text.Encoding.UTF8, "application/xml");

            using (HttpClient httpreq = new HttpClient())
            {
                HttpResponseMessage resp = await httpreq.PostAsync(url, content);
                if (resp.IsSuccessStatusCode)
                {
                    result.StatusCode = HttpStatusCode.OK;
                    MultipartMemoryStreamProvider mprov2 = await resp.Content.ReadAsMultipartAsync();
                    MemoryStream ms = new MemoryStream();
                    HttpContent respContent = new StreamContent(ms);
                    foreach (HttpContent c in mprov2.Contents)
                    {
                        // Binary data part has Content-Type: application/octet-stream
                        if (c.Headers.ContentType.ToString().StartsWith("application/octet-stream"))
                        {
                            await c.CopyToAsync(ms);

                            ms.Seek(0, SeekOrigin.Begin);
                            respContent.Headers.ContentType = c.Headers.ContentType;
                            respContent.Headers.ContentLength = ms.Length;
                        }
                        // SOAP part has Content-Type: application/xop+xml
                        else if (c.Headers.ContentType.ToString().StartsWith("application/xop+xml"))
                        {
                            XmlDocument d = new XmlDocument();
                            d.Load(await c.ReadAsStreamAsync());
                            // Use local-name() in the XPath to avoid namespace issues.
                            string fileName = d.DocumentElement.SelectSingleNode("//*[local-name()='File']/*[local-name()='DisplayName']").FirstChild.Value;
                            respContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                            respContent.Headers.ContentDisposition.FileName = fileName;
                        }
                    }
                    result.Content = respContent;
                }
            }
            return result;
        }

        private async Task<HttpResponseMessage> GetFileContent(Uri url, HttpContent content)
        {
            using (HttpClient httpreq = new HttpClient())
            {
                return await httpreq.PostAsync(url, content);
            }
        }

        /*
         * The GetFileContent method of the IIPAX archive SOAP connection will not work with .NET
         * since the MTOM multipart that holds the actual file data lacks the Content-Transfer-Encoding: binary
         * header. So we just do a raw post which looks like SOAP, and then handle the result as a
         * plain HTTP multipart response.
         * */
        // POST: api/File
        public async Task<HttpResponseMessage> Post([FromBody]FileQuery query)
        {
            string id = query.id;

            HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.NotFound);

            Uri url = getServiceUri();

            string reqBody = soapTmpl.Replace("@id", id);

            HttpContent content = new StringContent(reqBody, System.Text.Encoding.UTF8, "application/xml");

            using (HttpClient httpreq = new HttpClient())
            {
                HttpResponseMessage resp = await httpreq.PostAsync(url, content);
                if (resp.IsSuccessStatusCode)
                {
                    result.StatusCode = HttpStatusCode.OK;
                    MultipartMemoryStreamProvider mprov2 = await resp.Content.ReadAsMultipartAsync();
                    MemoryStream ms = new MemoryStream();
                    HttpContent respContent = new StreamContent(ms);
                    foreach (HttpContent c in mprov2.Contents)
                    {
                        // Binary data part has Content-Type: application/octet-stream
                        if (c.Headers.ContentType.ToString().StartsWith("application/octet-stream"))
                        {
                            await c.CopyToAsync(ms);

                            ms.Seek(0, SeekOrigin.Begin);
                            respContent.Headers.ContentType = c.Headers.ContentType;
                            respContent.Headers.ContentLength = ms.Length;
                        }
                        // SOAP part has Content-Type: application/xop+xml
                        else if (c.Headers.ContentType.ToString().StartsWith("application/xop+xml"))
                        {
                            XmlDocument d = new XmlDocument();
                            d.Load(await c.ReadAsStreamAsync());
                            // Use local-name() in the XPath to avoid namespace issues.
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
