

namespace Common.Models.Api
{
    using System.Collections.Generic;
    public abstract class ApiModel
    {
    }

    public class Apistring : ApiModel
    {
        public string Item { get; set; }
    }

    public class ApiKeyValuePairs : ApiModel
    {
        public List<KeyValuePair<string, string>> KVP { get; set; }
    }
}
