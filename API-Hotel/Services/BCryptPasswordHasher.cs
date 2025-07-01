using Microsoft.AspNetCore.Identity;
using hotel.Models.Identity;
using System.Linq;

namespace hotel.Services;

public class BCryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private readonly ILogger<BCryptPasswordHasher>? _logger;
    
    public BCryptPasswordHasher()
    {
        // Constructor without DI - will use Console.WriteLine for logging
    }
    
    public string HashPassword(ApplicationUser user, string password)
    {
        // Use Identity's default hasher for new passwords
        var defaultHasher = new PasswordHasher<ApplicationUser>();
        return defaultHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        // First try to identify if this looks like a BCrypt hash
        bool couldBeBCryptHash = IsPotentialBCryptHash(hashedPassword);
        Console.WriteLine($"Could be BCrypt hash: {couldBeBCryptHash}");
        
        if (couldBeBCryptHash)
        {
            Console.WriteLine("Attempting BCrypt verification methods...");
            
            // Method 1: Try BCrypt.Net-Next standard verification
            try
            {
                Console.WriteLine("Trying BCrypt standard verification...");
                if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
                {
                    Console.WriteLine("✅ BCrypt verification successful with standard mode");
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    Console.WriteLine("❌ BCrypt standard verification returned false");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BCrypt standard verification failed: {ex.Message}");
            }
            
            // Method 2: Try BCrypt.Net-Next Enhanced mode
            try
            {
                Console.WriteLine("Trying BCrypt enhanced verification...");
                if (BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword))
                {
                    Console.WriteLine("✅ BCrypt verification successful with enhanced mode");
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    Console.WriteLine("❌ BCrypt enhanced verification returned false");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BCrypt enhanced verification failed: {ex.Message}");
            }
            
            // Method 3: Try BCrypt with different hash types
            try
            {
                if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword, hashType: BCrypt.Net.HashType.SHA256))
                {
                    Console.WriteLine("BCrypt verification successful with SHA256");
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BCrypt SHA256 verification failed: {ex.Message}");
            }
            
            // Method 4: Try BCrypt with SHA384
            try
            {
                if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword, hashType: BCrypt.Net.HashType.SHA384))
                {
                    Console.WriteLine("BCrypt verification successful with SHA384");
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BCrypt SHA384 verification failed: {ex.Message}");
            }
            
            // Method 5: Try BCrypt with SHA512
            try
            {
                if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword, hashType: BCrypt.Net.HashType.SHA512))
                {
                    Console.WriteLine("BCrypt verification successful with SHA512");
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BCrypt SHA512 verification failed: {ex.Message}");
            }
        }
        
        // Try Identity's default hasher (for Identity hashes or if BCrypt failed)
        try
        {
            var defaultHasher = new PasswordHasher<ApplicationUser>();
            var result = defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            
            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return result;
            }
        }
        catch (Exception)
        {
            // Identity hasher also failed
        }
        
        return PasswordVerificationResult.Failed;
    }
    
    private static bool IsPotentialBCryptHash(string hash)
    {
        // BCrypt hashes have a specific format: $2[a/b/x/y]$[cost]$[22 character salt][31 character hash]
        // Total length should be around 60 characters
        if (string.IsNullOrEmpty(hash) || hash.Length < 59 || hash.Length > 61)
            return false;
            
        // Check if it starts with BCrypt identifier
        if (!(hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || 
              hash.StartsWith("$2x$") || hash.StartsWith("$2y$")))
            return false;
            
        // Check basic structure: should have exactly 3 dollar signs
        var dollarCount = hash.Count(c => c == '$');
        if (dollarCount != 3)
            return false;
            
        // Basic validation of cost parameter (should be between 04 and 31)
        var parts = hash.Split('$');
        if (parts.Length != 4)
            return false;
            
        if (!int.TryParse(parts[2], out int cost) || cost < 4 || cost > 31)
            return false;
            
        // Salt and hash part should be exactly 53 characters (22 + 31)
        if (parts[3].Length != 53)
            return false;
            
        return true;
    }
}