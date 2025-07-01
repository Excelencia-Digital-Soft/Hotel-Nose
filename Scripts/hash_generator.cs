using System;
using Microsoft.AspNetCore.Identity;

// Simple script to generate the correct Identity password hash for "Pass123"
var passwordHasher = new PasswordHasher<object>();
string defaultPassword = "Pass123";
string hashedPassword = passwordHasher.HashPassword(null, defaultPassword);

Console.WriteLine("=== DEFAULT PASSWORD HASH FOR MIGRATION ===");
Console.WriteLine($"Password: {defaultPassword}");
Console.WriteLine($"Hashed: {hashedPassword}");
Console.WriteLine();

// Verify the hash works
var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, defaultPassword);
Console.WriteLine($"Verification: {result}");