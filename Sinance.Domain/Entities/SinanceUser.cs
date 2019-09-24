using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Domain.Entities
{
    public class SinanceUser : EntityBase
    {
        public string Password { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }
}