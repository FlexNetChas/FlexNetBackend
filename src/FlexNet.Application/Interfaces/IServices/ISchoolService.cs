using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Interfaces.IServices;

public interface ISchoolService
{
   Task<Result<IEnumerable<School>>> SearchSchoolsAsync(
      SchoolSearchCriteria criteria,
      CancellationToken cancellationToken = default);
   
   Task<Result<School>> GetSchoolByCodeAsync(
      string schoolUnitCode,
      CancellationToken cancellationToken = default);
   
   Task<Result<int>> RefreshCacheAsync(
      CancellationToken cancellationToken = default);
}