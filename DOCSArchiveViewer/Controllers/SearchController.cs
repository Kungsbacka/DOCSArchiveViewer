using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DOCSArchiveViewer.IIPAX;

namespace DOCSArchiveViewer.Controllers
{
    public class SearchController : ApiController
    {
        // GET: api/Search
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // GET: api/Search/5
        public HttpResponseMessage Get(string id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // POST: api/Search
        public IEnumerable<ArchiveObject> Post(SearchQuery sqs)
        {
            ArchivePortTypeClient ar = new ArchivePortTypeClient();

            SearchAips search = new SearchAips();
            search.Options = new SearchOptions();
            search.Options.RequestedAttributes = new string[]
            {
                "arendemening_docs",
                "arendetyp_docs",
                "avslutat_docs",
                "secrecy"
            };
            search.Options.Offset = sqs.offset;
            search.Options.OffsetSpecified = true;
            search.Options.PageSize = sqs.pagesize;
            search.Options.PageSizeSpecified = true;

            Query[] q = new Query[] { new Query() };

            q[0].type = QueryType.DESCENDANT;
            q[0].ObjectType = new string[] { "docs_arende" };

            List<SearchCondition> conds = new List<SearchCondition>();

            foreach(SearchAttribute sq in sqs.attributes)
            {
                SearchCondition cond = new SearchCondition
                {
                    Attribute = sq.attribute,
                    Operator = translateOperator(sq.op),
                    Value = new string[] { sq.value }
                };
                conds.Add(cond);
            }

            q[0].SearchCondition = conds.ToArray();

            search.Query = q;

            SearchAipsResponse response = ar.SearchAips(search);

            return response.ArchiveObject;
        }

        // PUT: api/Search/5
        public HttpResponseMessage Put(int id, [FromBody]string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE: api/Search/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        private Operator translateOperator(string opName)
        {
            switch (opName)
            {
                case "MATCHES":
                    return Operator.MATCHES;
                case "EQUAL":
                    return Operator.EQUAL;
                case "NOT_EQUAL":
                    return Operator.NOT_EQUAL;
                case "LESS":
                    return Operator.LESS;
                case "LESS_OR_EQUAL":
                    return Operator.LESS_OR_EQUAL;
                case "GREATER":
                    return Operator.GREATER;
                case "GREATER_OR_EQUAL":
                    return Operator.GREATER_OR_EQUAL;
                default:
                    return Operator.MATCHES;
            }
        }
    }

    public class SearchQuery
    {
        public int offset { get; set; } = 0;
        public int pagesize { get; set; } = 20;

        public SearchAttribute[] attributes = new SearchAttribute[] { };
    }

    public class SearchAttribute
    {
        public string attribute { get; set; }
        public string op { get;set;}
        public string value { get; set; }

    }
}
