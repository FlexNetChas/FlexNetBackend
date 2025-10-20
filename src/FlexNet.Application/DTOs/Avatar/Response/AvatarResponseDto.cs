namespace FlexNet.Application.DTOs.Avatar.Response;

public record AvatarResponseDto(
    int Id,
    string Style,
    string Personality,
    bool VoiceEnabled,
    string VoiceSelection
);