using FlexNet.Application.Models;

namespace FlexNet.Application.Interfaces.IServices;

public interface ISchoolSearchDetector
{
    SchoolRequestInfo? DetectSchoolRequest(string userMsg);
}