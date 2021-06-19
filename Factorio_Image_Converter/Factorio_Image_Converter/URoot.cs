using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Image_Converter
{
    class URoot
    {
        [JsonProperty("usable-blocks")]
        public UsableBlocks UsableBlocks { get; set; }
    }
}
