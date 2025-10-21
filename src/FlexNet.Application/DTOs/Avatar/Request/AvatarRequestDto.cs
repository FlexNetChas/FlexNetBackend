namespace FlexNet.Application.DTOs.Avatar.Request;

public record AvatarRequestDto(
    string Style,
    string Personality,
    bool VoiceEnabled,
    string VoiceSelection
);