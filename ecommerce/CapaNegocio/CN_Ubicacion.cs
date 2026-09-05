using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_Ubicacion
    {
        private CD_Ubicacion objCapaDato = new CD_Ubicacion();

        public List<Estado> ObtenerEstado()
        {
            return objCapaDato.ObtenerEstado();
        }

        public List<Municipio> ObtenerMunicipio(string idestado)
        {
            return objCapaDato.ObtenerMunicipio(idestado);
        }

        public List<Localidad> ObtenerLocalidad(string idestado, string idmunicipio)
        {
            return objCapaDato.ObtenerLocalidad(idestado, idmunicipio);
        }
    }
}
