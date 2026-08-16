namespace AS.Application.DTOs.Lista;

public class MigracionCloudinaryResultDTO
{
    public int TotalProcesados { get; set; }
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
    public List<MigracionCloudinaryErrorDTO> Errores { get; set; } = [];
}

public class MigracionCloudinaryErrorDTO
{
    public int IdLista { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
