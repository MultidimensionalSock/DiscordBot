using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic8.Structures
{
    public enum InteractionType
    {
        PING = 1,
        SLASH_COMMAND = 2,
        COMPONENT_INTERACTION = 3,
        AUTOCOMPLETE = 4,
        MODAL_SUBMISSIONS = 5
    }
}
