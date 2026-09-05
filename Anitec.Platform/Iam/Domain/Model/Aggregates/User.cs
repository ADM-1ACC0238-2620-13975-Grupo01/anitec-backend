using System.Text.Json.Serialization;

namespace Anitec.Platform.Iam.Domain.Model.Aggregates;

/**
 * <summary>
 *     The user aggregate
 * </summary>
 * <remarks>
 *     This class is used to represent a user
 * </remarks>
 */
public partial class User(string username, string passwordHash, string fullName, string role)
{
    public User() : this(string.Empty, string.Empty, string.Empty, "Rancher")
    {
        
    }

    public int Id { get; }
    public string Username { get; private set; } = username;
    public string FullName { get; private set; } = fullName;
    public string Role { get; private set; } = role;

    [JsonIgnore] public string PasswordHash { get; private set; } = passwordHash;

    /**
     * <summary>
     *     Update the username
     * </summary>
     * <param name="username">The new username</param>
     * <returns>The updated user</returns>
     */
    public User UpdateUsername(string username)
    {
        Username = username;
        return this;
    }

    /**
     * <summary>
     *     Update the password hash
     * </summary>
     * <param name="passwordHash">The new password hash</param>
     * <returns>The updated user</returns>
     */
    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }

    public User UpdateProfile(string fullName, string role)
    {
        FullName = fullName;
        Role = role;
        return this;
    }
}
