﻿using System;
using Amazon.Runtime;
using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3.Database.Dynamo
{
    class DynamoDbCallException : Exception
    {
        public DynamoDbCallException(AmazonWebServiceResponse response)
        {
            Response = response;
        }

        public AmazonWebServiceResponse Response { get; }

        public override string ToString()
        {
            return "Operation failed with following response:" + Environment.NewLine +
                   JsonConvert.SerializeObject(Response) + Environment.NewLine +
                   base.ToString();
        }
    }
}
