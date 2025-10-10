using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexNet.Application.Models
{
    public abstract record ConversationMessage(string Role, string Content);
}