using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.web.Entities
{
    public class TCVehicleRecallDataModel
    {
        // TC VRD Model: https://data.tc.gc.ca/v1.3/api/fra/base-de-donnees-de-rappels-de-vehicules/sommaire-rappel/numero-de-rappel/2015321?format=json
        [JsonProperty("ResultSet")]
        public IEnumerable<IEnumerable<Node>> ResultSet { get; set; }

        public TCVehicleRecallDataModel()
        {
            ResultSet = new List<List<Node>>();
        }
    }

    public class Node
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public Property Value { get; set; }
    }

    public class Property
    {
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Literal")]
        public string Literal { get; set; }
    }
}
