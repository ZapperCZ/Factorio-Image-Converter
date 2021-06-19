using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factorio_Image_Converter
{
    class Entity
    {
        public bool SType = false;
        public bool SDirection = false;
        public int entity_number { get; set; }
        public string name { get; set; }
        public Position position { get; set; }
        public string type { get; set; }
        public int? direction { get; set; }
        public bool ShouldSerializetype()
        {
            return SType;
        }
        public bool ShouldSerializedirection()
        {
            return SDirection;
        }
        public bool ShouldSerializeSType()
        {
            return false;
        }
        public bool ShouldSerializeSDirection()
        {
            return false;
        }
    }
}
