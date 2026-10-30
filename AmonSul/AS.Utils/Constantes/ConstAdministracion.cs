namespace AS.Utils.Constantes;

public static class ConstAdministracion
{
    // Quienes pueden enviar un correo a todos los usuarios
    private static readonly int[] IdUsuariosAdministradores = [168];

    public static bool EsAdministrador(int idUsuario) => IdUsuariosAdministradores.Contains(idUsuario);
}
