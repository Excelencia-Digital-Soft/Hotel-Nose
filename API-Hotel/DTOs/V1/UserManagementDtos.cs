namespace hotel.DTOs.V1;

public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int? InstitucionId { get; set; }
    public string? InstitucionName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public bool ForcePasswordChange { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class UpdateUserRolesRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

public class BlockUserRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UserRolesResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class ChangeUserPasswordRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateUserRequestDto
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int? InstitucionId { get; set; }
    public bool? IsActive { get; set; }
    public bool? ForcePasswordChange { get; set; }
    public IList<string>? Roles { get; set; }
}