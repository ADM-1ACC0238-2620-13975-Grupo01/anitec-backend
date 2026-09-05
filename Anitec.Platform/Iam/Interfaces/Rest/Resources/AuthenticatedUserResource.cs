namespace Anitec.Platform.Iam.Interfaces.Rest.Resources;

public record AuthenticatedUserResource(int Id, string Username, string FullName, string Role, string Token);
