namespace Tools.DynamicsCRM.Configuration
{
    public class DynamicsCrmConfig
    {
        public string BaseUrl { get; set; }
        public string WebApiPath { get; set; }
        public string Version { get; set; }

        public DynamicsCrmAuthenticationConfig Authentication { get; set; }
    }
}
