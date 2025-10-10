namespace ChorePlay.Api.Shared.Domain.Exceptions;

public sealed class ConflictException(string message) : Exception(message);

