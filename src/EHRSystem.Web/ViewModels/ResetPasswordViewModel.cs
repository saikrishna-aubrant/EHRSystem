// [Requirement: US-AUTH-03]
public class ResetPasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }  // AC: Password complexity
} 