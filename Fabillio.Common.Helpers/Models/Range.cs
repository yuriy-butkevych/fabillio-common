namespace Fabillio.Common.Helpers.Models;

public record Range<T>(T? From, T? To) where T: struct;