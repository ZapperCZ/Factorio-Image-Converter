using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Image_Converter
{
    class UsableBlocks
    {
        [JsonProperty("u-blocks")]
        public List<UBlock> UBlocks { get; set; }

        [JsonProperty("u-tiles")]
        public List<UTile> UTiles { get; set; }
    }
}
