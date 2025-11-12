using FlexNet.Application.DTOs.AI;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Models;

public record MessageContext(
    ChatSession Session,
    string SanitizedMessage,
    int UserId,
    UserContextDto UserContext,
    List<ConversationMessage> ConversationHistory,
    string ContextMessage);