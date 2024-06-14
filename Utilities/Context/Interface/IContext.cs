using System;

namespace Panoramas_Editor
{
    public interface IContext
    {
        void Invoke(Action action);
        void BeginInvoke(Action action);
    }
}
