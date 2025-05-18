using System;

namespace Tools.DynamicsCRM.Exceptions
{
    public class CrmTooManyRequestException : Exception
    {
        private const string _defaultmessage = "Too many request were made to the CRM";

        public CrmTooManyRequestException() : base(_defaultmessage) { }
    }
}
