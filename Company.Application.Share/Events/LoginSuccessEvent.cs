using Company.Application.Share.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Share.Events
{
    public class LoginSuccessEvent:PubSubEvent<CurrentUser>
    {
        
    }
}
