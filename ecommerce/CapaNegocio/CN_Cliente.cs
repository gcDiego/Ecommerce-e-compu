using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_Cliente
    {
        private CD_Cliente objCapaDato = new CD_Cliente();

        public int Registrar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // Validar campos 9/sep/2025 Diego García
            if (string.IsNullOrEmpty(obj.Nombres) || string.IsNullOrWhiteSpace(obj.Nombres))
            {
                Mensaje = "El nombre del cliente no puede ser vacío";
                return 0;
            }
            else if (string.IsNullOrEmpty(obj.Apellidos) || string.IsNullOrWhiteSpace(obj.Apellidos))
            {
                Mensaje = "El apellido no puede ser vacío";
                return 0;
            }
            else if (string.IsNullOrEmpty(obj.Correo) || string.IsNullOrWhiteSpace(obj.Correo))
            {
                Mensaje = "El correo no puede ser vacío";
                return 0;
            }
            // Validación de contraseña 9/sep/2025 Diego García
            else if (string.IsNullOrEmpty(obj.Clave) || string.IsNullOrWhiteSpace(obj.Clave))
            {
                Mensaje = "La contraseña no puede ser vacía";
                return 0;
            }
            else if (obj.Clave != obj.ConfirmarClave)
            {
                Mensaje = "Las contraseñas no coinciden";
                return 0;
            }

            try
            {
                obj.Clave = CN_Recursos.ConvertirSha256(obj.Clave);
                return objCapaDato.Registar(obj, out Mensaje);
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al registrar cliente: {ex.Message}";
                return 0;
            }
        }

        public List<Cliente> Listar()
        {
            return objCapaDato.Listar();
        }

        public bool CambiarClave(int idcliente, string nuevaclave, out string Mensaje)
        {
            return objCapaDato.CambiarClave(idcliente, nuevaclave, out Mensaje);
        }

        public bool RestablecerClave(int idcliente, string correo, out string Mensaje)
        {
            Mensaje = string.Empty;
            string nuevaclave = CN_Recursos.GenerarClave();
            bool resultado = objCapaDato.RestablecerClave(idcliente, CN_Recursos.ConvertirSha256(nuevaclave), out Mensaje);

            if (resultado)
            {
                string asunto = "Contraseña Restablecida";
                string mensaje_correo = "<h3>Su contraseña fue restablecida correctamente</h3></br><p>Su contraseña para acceder ahora es: !clave!</p>";
                mensaje_correo = mensaje_correo.Replace("!clave!", nuevaclave);

                bool respuesta = CN_Recursos.EnviarCorreo(correo, asunto, mensaje_correo);

                if (respuesta)
                {
                    return true;
                }
                else
                {
                    Mensaje = "No se pudo enviar el correo";
                    return false;
                }
            }
            else
            {
                Mensaje = "No se pudo restablecer la contraseña";
                return false;
            }
        }
    }
}