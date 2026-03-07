using System;

namespace Tools.DynamicsCRM.Exceptions
{
    public class CrmTooManyRequestException : Exception
    {
        private const string DefaultMessage = "Too many request were made to the CRM";

        public CrmTooManyRequestException() : base(DefaultMessage) { }
    }
}