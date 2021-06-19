using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Image_Converter
{
    class Blueprint
    {
        public string description { get; set; }
        public List<Icon> icons { get; set; }
        public List<Entity> entities { get; set; }
        public List<Tile> tiles { get; set; }
        public string item { get; set; }
        public string label { get; set; }
        public long version { get; set; }
    }
}
