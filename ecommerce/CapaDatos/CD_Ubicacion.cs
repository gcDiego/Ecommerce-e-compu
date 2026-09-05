using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Ubicacion
    {
        public List<Estado> ObtenerEstado()
        {
            List<Estado> lista = new List<Estado>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    string query = "select * from ESTADO";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Estado()
                            {
                                IdEstado = dr["IdEstado"].ToString(),
                                Descripcion = dr["Descripcion"].ToString()
                            });
                        }
                    }

                }
            }
            catch (Exception)
            {
                lista = new List<Estado>();

            }

            return lista;
        }

        public List<Municipio> ObtenerMunicipio(string idestado)
        {
            List<Municipio> lista = new List<Municipio>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    string query = "select * from MUNICIPIO where IdEstado = @idestado";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@idestado", idestado);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Municipio()
                            {
                                IdMunicipio = dr["IdMunicipio"].ToString(),
                                Descripcion = dr["Descripcion"].ToString()
                            });
                        }
                    }

                }
            }
            catch (Exception)
            {
                lista = new List<Municipio>();

            }

            return lista;
        }

        public List<Localidad> ObtenerLocalidad(string idestado, string idmunicipio)
        {
            List<Localidad> lista = new List<Localidad>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    string query = "select * from LOCALIDAD where IdMunicipio = @idmunicipio and IdEstado = @idestado";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("idmunicipio", idmunicipio);
                    cmd.Parameters.AddWithValue("@idestado", idestado);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Localidad()
                            {
                                IdLocalidad = dr["IdLocalidad"].ToString(),
                                Descripcion = dr["Descripcion"].ToString()
                            });
                        }
                    }

                }
            }
            catch (Exception)
            {
                lista = new List<Localidad>();

            }

            return lista;
        }
    }
}
