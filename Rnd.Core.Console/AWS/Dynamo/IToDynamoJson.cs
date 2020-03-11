using Newtonsoft.Json;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo
{
    interface IToDynamoJson
    {
        void Write(JsonWriter writer);
    }
}
