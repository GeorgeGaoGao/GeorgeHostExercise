using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Share.Config
{
    public interface ISoftwareConfigProvider
    {
        SoftwareConfig SoftwareConfig { get; }
        event Action ConfigChanged;
    }
}
