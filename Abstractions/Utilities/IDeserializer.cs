using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal interface IDeserializer
    {
        public IEnumerable<ImageSettings> ReadData();
    }
}
