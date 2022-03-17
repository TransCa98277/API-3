using System.Collections.Generic;

namespace api.web.Implementations
{
    public interface IManageVRDParameters
    {
        IDictionary<string, string> VRDParameters { get; set; }
    }
}