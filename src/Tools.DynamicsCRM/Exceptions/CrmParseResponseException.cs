using System;

namespace Tools.DynamicsCRM.Exceptions
{
    public class CrmParseResponseException : Exception
    {
        public CrmParseResponseException(string errormessage) : base(errormessage) { }

    }
}
