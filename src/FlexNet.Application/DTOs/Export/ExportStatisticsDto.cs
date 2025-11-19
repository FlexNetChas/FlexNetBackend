namespace FlexNet.Application.DTOs.Export;
/// <summary>
/// Statistical summary of exported data
/// </summary>
public record ExportStatisticsDto
(
   int TotalChatSessions, 
    int TotalMessages,
    int AccountAgeInDays
);