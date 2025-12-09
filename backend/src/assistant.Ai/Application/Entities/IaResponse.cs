namespace Assistant.Ai.Application.Entities;

public record IaResponse(string Answer , int TokensUsed, string? Model);