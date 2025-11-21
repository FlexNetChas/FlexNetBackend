namespace FlexNet.Domain.Entities.Schools;

public record SchoolProgram(
    string Code,
    string Name,
    IReadOnlyList<StudyPath>? StudyPaths);