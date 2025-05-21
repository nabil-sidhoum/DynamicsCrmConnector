using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tools.DynamicsCRM
{
    public interface IDynamicsCrmClient
    {
        HttpClient Client { get; }

        string ApiCommonPath { get; }

        Task<Dictionary<string, object>> RetrieveAsync(string entitysetname, Guid entityid, string[] columnset = null);

        Task<IEnumerable<Dictionary<string, object>>> RetrieveMultipleAsync(string resourceandquery);

        Task<Guid> CreateAsync(string entitysetname, Dictionary<string, object> entitydata);

        Task<Guid> CreateAsync(string entitysetname, object entitydata);

        Task<bool> UpdateAsync(string entitysetname, Guid entityid, Dictionary<string, object> entitydata);

        Task<bool> DeleteAsync(string entitysetname, Guid entityid);

        Task<Dictionary<int, string>> GetOptionSetValueAsync(string entitylogicalname, string optionsetlogicalname);

        Task<bool> MergeContactAsync(Guid entityid, Guid mergedentityid);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

        Task<bool> AssociateAsync(string entitysetname, Guid entityid, string relationshipname, string relatedentitysetname, Guid relatedentityid);

        Task<bool> DisassociateAsync(string entitysetname, Guid entityid, string relationshipname, Guid relatedentityid);

        Task<IEnumerable<Dictionary<string, object>>> SendFetchXmlAsync(string entitysetname, string fetchXml, int pagesize = 5000);
    }
}
