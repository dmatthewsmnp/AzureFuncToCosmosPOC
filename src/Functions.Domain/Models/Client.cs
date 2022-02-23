using System;

namespace Functions.Domain.Models;

/// <summary>
/// Placeholder for ODS Client model (just copies MPM Client for now)
/// </summary>
public record Client(Guid id, int ClientNo, string? ClientName, string? BusinessPhone, string? BusinessFax, string? Website, string? Street1, string? Street2, string? Street3, string? City, string? Province, string? PostalCode, string? Country);
