public class UpdateUserDTO
{
    public int Id { get; set; }

    public string? Nombre { get; set; }
    public string? ApellidoPa { get; set; }
    public string? ApellidoMa { get; set; }
    public DateTime? FecNac { get; set; }
    public string? Sexo { get; set; }

    public string? DirColonia { get; set; }
    public string? DirCalle { get; set; }
    public string? DirNum { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Contrasena { get; set; }

    public bool? Activo { get; set; }

    public UserRole? IDTipoUsuario { get; set; }
}