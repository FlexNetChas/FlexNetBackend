using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices;

public interface IAiClient
{
   Task<Result<string>> CallAsync(string prompt);
}