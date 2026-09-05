namespace Anitec.Platform.Iam.Interfaces.Rest.Resources;

public record SignUpResource(string Username, string Password, string FullName = "", string Role = "Rancher");
