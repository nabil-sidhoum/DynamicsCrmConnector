using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tools.DynamicsCRM.Exceptions
{
    /// <summary>  
    /// Produces a populated exception from an error message in the content of an HTTP response.   
    /// </summary>  
    public class CrmHttpResponseException : Exception
    {
        #region Properties  
        private static string _stackTrace;

        /// <summary>  
        /// Gets a string representation of the immediate frames on the call stack.  
        /// </summary>  
        public override string StackTrace => _stackTrace;
        #endregion Properties  

        #region Constructors  
        /// <summary>  
        /// Initializes a new instance of the CrmHttpResponseException class.  
        /// </summary>  
        /// <param name="content">The populated HTTP content in Json format.</param>  
        public CrmHttpResponseException(HttpContent content)
            : base(ExtractMessageFromContent(content)) { }

        /// <summary>  
        /// Initializes a new instance of the CrmHttpResponseException class.  
        /// </summary>  
        /// <param name="content">The populated HTTP content in Json format.</param>  
        /// <param name="innerexception">The exception that is the cause of the current exception, or a null reference  
        /// if no inner exception is specified.</param>  
        public CrmHttpResponseException(HttpContent content, Exception innerexception)
            : base(ExtractMessageFromContent(content), innerexception) { }

        #endregion Constructors  

        #region Methods  
        /// <summary>  
        /// Extracts the CRM specific error message and stack trace from an HTTP content.   
        /// </summary>  
        /// <param name="content">The HTTP content in Json format.</param>  
        /// <returns>The error message.</returns>  
        private static string ExtractMessageFromContent(HttpContent content)
        {
            string message = string.Empty;
            string downloadedContent = content.ReadAsStringAsync().Result;
            if (content.Headers.ContentType.MediaType.Equals("text/plain", StringComparison.Ordinal))
            {
                message = downloadedContent;
            }
            else if (content.Headers.ContentType.MediaType.Equals("application/json", StringComparison.Ordinal))
            {
                JObject jcontent = (JObject)JsonConvert.DeserializeObject(downloadedContent);

                // An error message is returned in the content under the 'error' key.   
                if (jcontent.ContainsKey("error"))
                {
                    JObject error = (JObject)jcontent.Property("error").Value;
                    message = (string)error.Property("message").Value;
                }
                else if (jcontent.ContainsKey("Message"))
                {
                    message = (string)jcontent.Property("Message").Value;
                }

                if (jcontent.ContainsKey("StackTrace"))
                {
                    _stackTrace = (string)jcontent.Property("StackTrace").Value;
                }
            }
            else if (content.Headers.ContentType.MediaType.Equals("text/html", StringComparison.Ordinal))
            {
                message = "HTML content that was returned is shown below.";
                message += "\n\n" + downloadedContent;
            }
            else
            {
                message = $"No handler is available for content in the {content.Headers.ContentType.MediaType} format.";
            }
            return message;
        }
        #endregion Methods  
    }
}