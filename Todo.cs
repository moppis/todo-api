using Newtonsoft.Json;

namespace se.omegapoint.todo
{
    public class Todo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}