using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins
{
    interface IGrabbable
    {
        bool IsGrabbed { get; }

        bool Grab();

        void LetGo();
    }
}
