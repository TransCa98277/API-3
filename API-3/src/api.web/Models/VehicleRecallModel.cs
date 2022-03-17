using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.web.Entities
{
    public class VehicleRecallModel
    {
        // TC-Source
        [JsonProperty("recallNumber")]
        public string RecallNumber { get; set; }
        [JsonProperty("manufactureName")]
        public string ManufactureName { get; set; }
        [JsonProperty("makeName")]
        public string MakeName { get; set; }
        [JsonProperty("modelName")]
        public string ModelName { get; set; }
        [JsonProperty("recallYear")]
        public string RecallYear { get; set; }

        // API-1
        public string MANUFACTURER_RECALL_NO_TXT { get; set; }

        // API-2
        public string CATEGORY_ETXT { get; set; }
        public string CATEGORY_FTXT { get; set; }

        // API-3
        public string SYSTEM_TYPE_ETXT { get; set; } 
        public string SYSTEM_TYPE_FTXT { get; set; } 
    }
}
