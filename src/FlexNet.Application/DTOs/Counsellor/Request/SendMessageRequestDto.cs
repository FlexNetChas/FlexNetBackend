﻿namespace FlexNet.Application.DTOs.Counsellor.Request;

public record SendMessageRequestDto(
    string Message,
    int? Age // Optional for now
);