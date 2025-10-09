using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexNet.Application.Models
{
    public record StudentContext(
        int Age,
        string? Gender,
        string? Education,
        string? Purpose);
}