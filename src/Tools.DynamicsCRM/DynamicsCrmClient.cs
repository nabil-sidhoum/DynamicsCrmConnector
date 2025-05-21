using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Tools.DynamicsCRM.Configuration;
using Tools.DynamicsCRM.Exceptions;

namespace Tools.DynamicsCRM
{
    public sealed class DynamicsCrmClient : IDynamicsCrmClient
    {
        private const string RETRIEVE_MULTIPLE_NEXTPAGE_KEY = "@odata.nextLink";
        private const string RETRIEVE_MULTIPLE_VALUE_KEY = "value";
        private const string CREATE_ENTITY_ID_KEY = "OData-EntityId";

        private const string FETCHXML_VALUE_KEY = "value";
        private const string FETCHXML_MORERECORDS_KEY = "@Microsoft.Dynamics.CRM.morerecords";
        private const string FETCHXML_PAGINGCOOKIE_KEY = "@Microsoft.Dynamics.CRM.fetchxmlpagingcookie";

        public readonly HttpClient _client;
        private readonly DynamicsCrmAuthentication _authorization;

        public readonly string BaseUrl;
        public readonly string WebApiPath;
        public readonly string Version;

        public string ApiCommonPath
        {
            get
            {
                return Path.Combine(WebApiPath, Version).Replace("\\", "/");
            }
        }

        public HttpClient Client
        {
            get { return _client; }
        }

        public DynamicsCrmClient(HttpClient client, DynamicsCrmAuthentication authentication, DynamicsCrmConfig config)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(config);

            _authorization = authentication;

            BaseUrl = config.BaseUrl;
            WebApiPath = config.WebApiPath;
            Version = config.Version;
            _client = client;
            _client.BaseAddress = new Uri(BaseUrl);
            _client.Timeout = new TimeSpan(0, 2, 0);
        }


        public async Task<Dictionary<string, object>> RetrieveAsync(string entitysetname, Guid entityid, string[] columnset = null)
        {
            Dictionary<string, object> entity = [];
            string resource = $"{entitysetname}({entityid})";

            string query = string.Empty;

            if (columnset != null && columnset.Length > 0)
                query = $"?$select={string.Join(",", columnset)}";

            HttpRequestMessage request = new(HttpMethod.Get, Path.Combine(ApiCommonPath, resource + query)) { };

            HttpResponseMessage response = await SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    foreach (var attr in body)
                        entity.Add(attr.Key, attr.Value);
                    return entity;
                }
                catch (Exception e)
                {
                    throw new CrmParseResponseException(e.Message);
                }
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }

        }

        public async Task<IEnumerable<Dictionary<string, object>>> RetrieveMultipleAsync(string resourceandquery)
        {
            List<Dictionary<string, object>> data = [];
            string query = Path.Combine(ApiCommonPath, resourceandquery);

            while (!string.IsNullOrWhiteSpace(query))
            {
                HttpRequestMessage request = new(HttpMethod.Get, query) { };

                HttpResponseMessage response = await SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        if (body.ContainsKey(RETRIEVE_MULTIPLE_NEXTPAGE_KEY))
                        {
                            string nextlink = body[RETRIEVE_MULTIPLE_NEXTPAGE_KEY].Value<string>();
                            query = new Uri(nextlink).PathAndQuery;
                        }
                        else
                            query = string.Empty;

                        foreach (var entity in body[RETRIEVE_MULTIPLE_VALUE_KEY])
                            data.Add(entity.ToObject<Dictionary<string, object>>());
                    }
                    catch (Exception e)
                    {
                        throw new CrmParseResponseException(e.Message);
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        throw new CrmTooManyRequestException();
                    throw new CrmHttpResponseException(response.Content);
                }
            }

            return data;
        }

        public async Task<Guid> CreateAsync(string entitysetname, Dictionary<string, object> entitydata)
        {
            string resource = $"{entitysetname}";

            JObject entityObject = JObject.Parse(JsonConvert.SerializeObject(entitydata));

            HttpRequestMessage createHttpRequest = new(HttpMethod.Post, Path.Combine(ApiCommonPath, resource))
            {
                Content = new StringContent(entityObject.ToString(), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await SendAsync(createHttpRequest);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                string recordUri = response.Headers.GetValues(CREATE_ENTITY_ID_KEY).FirstOrDefault();
                string entityid = new Uri(recordUri).Segments.LastOrDefault().Replace(resource + "(", "").Replace(")", "");
                return Guid.Parse(entityid);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<Guid> CreateAsync(string entitysetname, object entitydata)
        {
            string resource = $"{entitysetname}";

            JObject entityObject = JObject.Parse(JsonConvert.SerializeObject(entitydata));

            HttpRequestMessage createHttpRequest = new(HttpMethod.Post, Path.Combine(ApiCommonPath, resource))
            {
                Content = new StringContent(entityObject.ToString(), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await SendAsync(createHttpRequest);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                string recordUri = response.Headers.GetValues(CREATE_ENTITY_ID_KEY).FirstOrDefault();
                string entityid = new Uri(recordUri).Segments.LastOrDefault().Replace(resource + "(", "").Replace(")", "");
                return Guid.Parse(entityid);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<bool> UpdateAsync(string entitysetname, Guid entityid, Dictionary<string, object> entitydata)
        {
            string resource = $"{entitysetname}({entityid})";

            JObject entityObject = JObject.Parse(JsonConvert.SerializeObject(entitydata));

            HttpRequestMessage request = new(HttpMethod.Patch, Path.Combine(ApiCommonPath, resource))
            {
                Content = new StringContent(entityObject.ToString(), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<bool> DeleteAsync(string entitysetname, Guid entityid)
        {
            string resource = $"{entitysetname}({entityid})";

            HttpRequestMessage request = new(HttpMethod.Delete, Path.Combine(ApiCommonPath, resource));

            HttpResponseMessage response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<Dictionary<int, string>> GetOptionSetValueAsync(string entitylogicalname, string optionsetlogicalname)
        {
            Dictionary<int, string> optionsets = [];
            string resource = $"EntityDefinitions(LogicalName='{entitylogicalname}')";

            string query = $"/Attributes(LogicalName='{optionsetlogicalname}')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=OptionSet($select=Options)";

            HttpRequestMessage request = new(HttpMethod.Get, Path.Combine(ApiCommonPath, resource + query)) { };

            HttpResponseMessage response = await SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    dynamic body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    foreach (var attr in body.OptionSet.Options)
                    {
                        int value = int.Parse($"{attr.Value}");
                        IEnumerable<dynamic> labels = attr.Label.LocalizedLabels;
                        string label = $"{labels.First().Label}";
                        optionsets.Add(value, label);
                    }
                    return optionsets;
                }
                catch (Exception e)
                {
                    throw new CrmParseResponseException(e.Message);
                }
            }
            else
            {
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<bool> MergeContactAsync(Guid entityid, Guid mergedentityid)
        {
            string resource = $"Merge";

            HttpRequestMessage request = new(HttpMethod.Post, Path.Combine(ApiCommonPath, resource))
            {
                Content = new StringContent(content: GetMergeContactBody(entityid, mergedentityid).ToString(), encoding: Encoding.UTF8, mediaType: "application/json")
            };

            HttpResponseMessage response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                throw new CrmHttpResponseException(response.Content);
            }
        }

        private static JObject GetMergeContactBody(Guid targetid, Guid subordinateid, bool performparentingchecks = false)
        {
            JObject jresult = [];

            JObject jtarget = new()
            {
                { "contactid", targetid },
                { "@odata.type","Microsoft.Dynamics.CRM.contact" }
            };
            JObject jsubordinate = new()
            {
                { "contactid", subordinateid },
                { "@odata.type","Microsoft.Dynamics.CRM.contact" }
            };

            jresult.Add("Target", jtarget);
            jresult.Add("Subordinate", jsubordinate);
            jresult.Add("PerformParentingChecks", performparentingchecks);

            return jresult;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _client.SendAsync(request.ConfigHeaders(_authorization.Token));
        }

        public async Task<bool> AssociateAsync(string entitysetname, Guid entityid, string relationshipname, string relatedentitysetname, Guid relatedentityid)
        {
            string resource = $"{entitysetname}({entityid})/{relationshipname}";
            Dictionary<string, object> entitydata = new()
            {
                { "@odata.id", Path.Combine(BaseUrl,$"{ApiCommonPath}/{relatedentitysetname}({relatedentityid})").Replace("\\", "/") }
            };

            JObject entityObject = JObject.Parse(JsonConvert.SerializeObject(entitydata));

            HttpRequestMessage request = new(HttpMethod.Post, Path.Combine(ApiCommonPath, resource + "/$ref"))
            {
                Content = new StringContent(entityObject.ToString(), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<bool> DisassociateAsync(string entitysetname, Guid entityid, string relationshipname, Guid relatedentityid)
        {
            string resource = $"{entitysetname}({entityid})/{relationshipname}({relatedentityid})";

            HttpRequestMessage request = new(HttpMethod.Delete, Path.Combine(ApiCommonPath, $"{resource}/$ref"));

            HttpResponseMessage response = await SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new CrmTooManyRequestException();
                throw new CrmHttpResponseException(response.Content);
            }
        }

        public async Task<IEnumerable<Dictionary<string, object>>> SendFetchXmlAsync(string entitysetname, string fetchXml, int pagesize = 5000)
        {
            List<Dictionary<string, object>> data = new();

            XElement fetchNode = XElement.Parse(fetchXml);

            int page = 1;
            fetchNode.SetAttributeValue("page", page);
            fetchNode.SetAttributeValue("count", pagesize);

            string query = Path.Combine(ApiCommonPath, $"{entitysetname}?fetchXml={HttpUtility.UrlEncode(fetchNode.ToString())}");

            while (!string.IsNullOrWhiteSpace(query))
            {
                HttpRequestMessage request = new(HttpMethod.Get, query) { };
                request.Headers.Add("Prefer", "odata.include-annotations=" + "\"Microsoft.Dynamics.CRM.fetchxmlpagingcookie," + "Microsoft.Dynamics.CRM.morerecords\"");

                HttpResponseMessage response = await SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        if (body.ContainsKey(FETCHXML_MORERECORDS_KEY))
                        {
                            bool morerecords = body[FETCHXML_MORERECORDS_KEY].Value<bool>();
                            if (morerecords)
                            {
                                string pagingcookie = body[FETCHXML_PAGINGCOOKIE_KEY].Value<string>();
                                XElement cookieElement = XElement.Parse(pagingcookie);
                                XAttribute pagingcookieAttribute = cookieElement.Attribute("pagingcookie");
                                pagingcookie = HttpUtility.UrlDecode(HttpUtility.UrlDecode(pagingcookieAttribute.Value));

                                fetchNode.SetAttributeValue("paging-cookie", pagingcookie);
                                fetchNode.SetAttributeValue("page", ++page);
                                query = Path.Combine(ApiCommonPath, $"{entitysetname}?fetchXml={HttpUtility.UrlEncode(fetchNode.ToString())}");
                            }
                            else
                                query = string.Empty;
                        }
                        else
                            query = string.Empty;

                        foreach (var entity in body[FETCHXML_VALUE_KEY])
                            data.Add(entity.ToObject<Dictionary<string, object>>());
                    }
                    catch (Exception e)
                    {
                        throw new CrmParseResponseException(e.Message);
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        throw new CrmTooManyRequestException();
                    throw new CrmHttpResponseException(response.Content);
                }
            }

            return data;
        }
    }
}
