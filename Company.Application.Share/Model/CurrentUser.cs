using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Share.Model
{
    public  class CurrentUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.Now;
    }
}
