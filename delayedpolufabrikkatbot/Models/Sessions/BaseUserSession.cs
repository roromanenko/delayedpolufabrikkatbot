using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace delayedpolufabrikkatbot.Models.Sessions
{
	public abstract class BaseUserSession
	{
        public ObjectId UserId { get; set; }
    }
}
