using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexNet.Application.Interfaces
{
    public interface IApiKeyProvider
    {
        Task<string> GetApiKeyAsync(CancellationToken cancellation = default);
    }
}