using Microsoft.AspNetCore.Identity;
using hotel.Models.Identity;

namespace hotel.Scripts;

/// <summary>
/// Utility class to generate the correct Identity password hash for "Pass123"
/// This ensures all migrated users have the same default password
/// </summary>
public static class GenerateDefaultPasswordHash
{
    public static void Main()
    {
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var dummyUser = new ApplicationUser(); // We only need this for the generic type
        
        string defaultPassword = "Pass123";
        string hashedPassword = passwordHasher.HashPassword(dummyUser, defaultPassword);
        
        Console.WriteLine("=== DEFAULT PASSWORD HASH FOR MIGRATION ===");
        Console.WriteLine($"Password: {defaultPassword}");
        Console.WriteLine($"Hashed: {hashedPassword}");
        Console.WriteLine();
        Console.WriteLine("Use this hash in the create_identity_tables.sql script:");
        Console.WriteLine($"'{hashedPassword}' as PasswordHash");
        Console.WriteLine();
        Console.WriteLine("=== VERIFICATION TEST ===");
        
        // Verify the hash works correctly
        var verificationResult = passwordHasher.VerifyHashedPassword(dummyUser, hashedPassword, defaultPassword);
        Console.WriteLine($"Verification result: {verificationResult}");
        
        // Test with wrong password
        var wrongVerification = passwordHasher.VerifyHashedPassword(dummyUser, hashedPassword, "wrongpassword");
        Console.WriteLine($"Wrong password test: {wrongVerification}");
    }
}