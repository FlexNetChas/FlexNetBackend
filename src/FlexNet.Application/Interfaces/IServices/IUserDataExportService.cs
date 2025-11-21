using FlexNet.Application.DTOs.Export;

namespace FlexNet.Application.Interfaces.IServices;
   ///<summary>
   /// Service for GDPR Article 20 - Right to Data Portability
   ///</summary>
public interface IUserDataExportService
{
   /// <summary>
   /// Exports all user data in JSON-compatible format
   /// </summary>
   /// <param name="userId">The user ID to export data for</param>
   /// <returns>Complete user data export DTO</returns>
   Task<UserDataExportDto> ExportUserDataAsync(int userId);
}