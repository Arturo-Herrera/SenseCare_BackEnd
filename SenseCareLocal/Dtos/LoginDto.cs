﻿namespace SenseCareLocal.DTOs;

public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
}
