using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tools.DynamicsCRM.Test
{
    [TestClass]
    public class DynamicsCrmClientUnitTest
    {
        private static readonly string _crmversion = "v9.2";
        private static readonly string _crmwebapipath = "api/data";

        private static IDynamicsCrmClient _crmconnector;

        [AssemblyInitialize]
        public static void InitAssembly(TestContext context)
        {
            IConfiguration config = new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.json", true, true)
                                        .Build();

            var services = new ServiceCollection();
            services.AddScoped<IConfiguration>(_ => config);

            services.AddDynamicsCRM(config);

            var serviceProvider = services.BuildServiceProvider();

            _crmconnector = serviceProvider.GetService<IDynamicsCrmClient>();
        }

        [TestMethod]
        public void TestGetDynamicsCrmClientProperty()
        {
            DynamicsCrmClient client = _crmconnector as DynamicsCrmClient;
            Assert.AreEqual(_crmversion, client.Version);
            Assert.AreEqual(_crmwebapipath, client.WebApiPath);
        }

        [TestMethod]
        public void TestRetrieveMultiple()
        {
            int courstoretrieve = 6000;
            IEnumerable<Dictionary<string, object>> data;
            string coursquery = "contacts?$select=firstname,lastname&$top=" + courstoretrieve;

            data = _crmconnector.RetrieveMultipleAsync(coursquery)
                                .GetAwaiter()
                                .GetResult();

            Assert.AreEqual(courstoretrieve, data.Count());
        }

        [TestMethod]
        public void TestCreateRetrieveEntity()
        {
            //Create
            string entitysetname = "contacts";
            string[] entitytocreatefieldname =
            [
                "firstname",
                "lastname"
            ];
            Dictionary<string, object> entitytocreate = new()
            {
                { entitytocreatefieldname[0], "test" },
                { entitytocreatefieldname[1], "test" },
            };

            Guid createdentityid = _crmconnector.CreateAsync(entitysetname, entitytocreate)
                                                .GetAwaiter()
                                                .GetResult();

            Assert.AreNotEqual(Guid.Empty, createdentityid);

            //Retrieve
            string[] entitytoretrievefieldname =
            [
                "firstname",
                "lastname"
            ];

            Dictionary<string, object> retrievedfields = _crmconnector.RetrieveAsync(entitysetname, createdentityid, entitytoretrievefieldname)
                                                                      .GetAwaiter()
                                                                      .GetResult();

            Assert.AreEqual(entitytocreate[entitytoretrievefieldname[0]], retrievedfields[entitytoretrievefieldname[0]].ToString());
            Assert.AreEqual(entitytocreate[entitytoretrievefieldname[1]], int.Parse(retrievedfields[entitytoretrievefieldname[1]].ToString()));
            Assert.IsTrue(string.IsNullOrWhiteSpace(retrievedfields[entitytoretrievefieldname[2]].ToString()));
            Assert.IsTrue(string.IsNullOrWhiteSpace(retrievedfields[entitytoretrievefieldname[3]].ToString()));
             }

        [TestMethod]
        public void TestOptionSetRetrieveEntity()
        {
            //Create
            string dynamicsEntityName = "contacts";
            string dynamicsOptionSetName = "civilite";

            var optionsets = _crmconnector.GetOptionSetValueAsync(dynamicsEntityName, dynamicsOptionSetName)
                                          .GetAwaiter()
                                          .GetResult();

            Assert.AreEqual(2, optionsets.Count);
        }

        [TestMethod]
        public void TestAssociateDisAssociateEntity()
        {
            string entitysetname = "";
            Guid entityid = new("");
            string relationshipname = "";
            string relatedentitysetname = "";
            Guid relatedentityid = new("");

            bool result = _crmconnector.AssociateAsync(entitysetname, entityid, relationshipname, relatedentitysetname, relatedentityid)
                                       .GetAwaiter()
                                       .GetResult();

            Assert.AreEqual(result, true);

            result = _crmconnector.DisassociateAsync(entitysetname, entityid, relationshipname, relatedentityid)
                                  .GetAwaiter()
                                  .GetResult();

            Assert.AreEqual(result, true);
        }
    }
}
