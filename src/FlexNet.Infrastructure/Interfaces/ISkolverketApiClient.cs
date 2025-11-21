using FlexNet.Infrastructure.Services.Skolverket.DTOs;

namespace FlexNet.Infrastructure.Interfaces;

public interface ISkolverketApiClient
{
    Task<SkolverketListResponse?> GetAllGymnasiumSchoolAsync(CancellationToken cancellationToken = default);
    Task<SkolverketDetailResponse?> GetSchoolDetailAsync(string schoolUnitCode, CancellationToken cancellationToken = default);
    Task<SkolverketProgramsResponse?> GetProgramsAsync(CancellationToken cancellationToken = default);

}