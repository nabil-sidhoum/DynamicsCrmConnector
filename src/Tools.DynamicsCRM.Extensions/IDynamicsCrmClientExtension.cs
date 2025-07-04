using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.DynamicsCRM.Extensions
{
    public static class IDynamicsCrmClientExtension
    {
        public static async Task<T> Retrieve<T>(this IDynamicsCrmClient crmclient, Guid entityid, string[] columnset = null) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            Dictionary<string, object> result = await crmclient.RetrieveAsync(entitysetname, entityid, columnset);
            JObject jentity = JObject.FromObject(result);
            ExpandoObject eo = JsonConvert.DeserializeObject<ExpandoObject>(jentity.ToString(), new ExpandoObjectConverter());
            T classInstance = (T)Activator.CreateInstance(typeof(T), eo);
            return classInstance;
        }

        public static async Task<IEnumerable<T>> RetrieveMultiple<T>(this IDynamicsCrmClient crmclient, string query) where T : Entity, new()
        {
            List<T> entities = new();
            string entitysetname = new T().EntitySetName;
            string resourceandpath = $"{entitysetname}?{query}";
            IEnumerable<Dictionary<string, object>> result = await crmclient.RetrieveMultipleAsync(resourceandpath);
            foreach (var item in result)
            {
                JObject jentity = JObject.FromObject(item);
                ExpandoObject eo = JsonConvert.DeserializeObject<ExpandoObject>(jentity.ToString(), new ExpandoObjectConverter());
                T classInstance = (T)Activator.CreateInstance(typeof(T), eo);
                entities.Add(classInstance);
            }
            return entities;
        }

        public static async Task<Guid> Create<T>(this IDynamicsCrmClient crmclient, T entitytocreate) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            Dictionary<string, object> entitydata = entitytocreate.ToExpandoObject().Where(kvp => kvp.Key != new T().PrimaryIdAttribute).ToDictionary(v => v.Key, v => v.Value);
            Guid entityid = await crmclient.CreateAsync(entitysetname, entitydata);
            return entityid;
        }

        public static async Task<bool> Update<T>(this IDynamicsCrmClient crmclient, T entitytoupdate) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            IDictionary<string, object> entitydata = entitytoupdate.ToExpandoObject();
            bool success = await crmclient.UpdateAsync(entitysetname, entitytoupdate.Id, new Dictionary<string, object>(entitydata));
            return success;
        }

        public static async Task<bool> Delete<T>(this IDynamicsCrmClient crmclient, Guid entityid) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            bool success = await crmclient.DeleteAsync(entitysetname, entityid);
            return success;
        }

        public static async Task<bool> Associate<T>(this IDynamicsCrmClient crmclient, Guid entityid, string relationshipname, string relatedentitysetname, Guid relatedentityid) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            bool success = await crmclient.AssociateAsync(entitysetname, entityid, relationshipname, relatedentitysetname, relatedentityid);
            return success;
        }

        public static async Task<bool> Disassociate<T>(this IDynamicsCrmClient crmclient, Guid entityid, string relationshipname, Guid relatedentityid) where T : Entity, new()
        {
            string entitysetname = new T().EntitySetName;
            bool success = await crmclient.DisassociateAsync(entitysetname, entityid, relationshipname, relatedentityid);
            return success;
        }

        public static async Task<IEnumerable<T>> SendFetchXml<T>(this IDynamicsCrmClient crmclient, string fetchxml, int pagesize = 5000) where T : Entity, new()
        {
            List<T> entities = new();
            string entitysetname = new T().EntitySetName;
            IEnumerable<Dictionary<string, object>> result = await crmclient.SendFetchXmlAsync(entitysetname, fetchxml, pagesize);
            foreach (var item in result)
            {
                JObject jentity = JObject.FromObject(item);
                ExpandoObject eo = JsonConvert.DeserializeObject<ExpandoObject>(jentity.ToString(), new ExpandoObjectConverter());
                T classInstance = (T)Activator.CreateInstance(typeof(T), eo);
                entities.Add(classInstance);
            }
            return entities;
        }


    }
}
