using FlexNet.Application.Models;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Interfaces.IServices;

public interface IPromptEnricher
{
    string EnrichWithSchools(string xmlPrompt, List<School> schools);
    string EnrichWithNoResults(string xmlPrompt, SchoolRequestInfo searchCriteria); 
}