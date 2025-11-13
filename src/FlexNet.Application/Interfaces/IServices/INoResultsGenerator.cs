using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices;

public interface INoResultsGenerator
{
   Task<Result<string>> GenerateAsync(string rawMessage, SchoolRequestInfo schoolRequest, UserContextDto userContextDto); 
}