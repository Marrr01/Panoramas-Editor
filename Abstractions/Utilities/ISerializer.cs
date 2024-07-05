using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal interface ISerializer
    {
        public void WriteData(IEnumerable<ImageSettings> data);
    }
}
