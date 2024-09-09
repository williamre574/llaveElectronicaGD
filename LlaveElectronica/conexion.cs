using System;
using System.Data;
using System.Data.SqlClient;

namespace LlaveElectronica
{
    internal class Conexion
    {
        private readonly string connectionString = "User ID=WRodriguez;Database=GuatemalaDigital;PASSWORD=Guate1234;server=guatemaladigital.org;Connect Timeout=30";

        public DataTable EjecutaQueryDT(string query, ref DataTable dt)
        {
            dt.Clear();

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                using (SqlDataAdapter dad = new SqlDataAdapter(query, cnn))
                {
                    dad.SelectCommand.CommandTimeout = 180;  // Tiempo de espera de la consulta
                    try
                    {
                        dad.Fill(dt);
                    }
                    catch (SqlException ex)
                    {
                        // Manejo de la excepción SQL (puedes loguearlo o manejarlo como necesites)
                        Console.WriteLine("Error al ejecutar la consulta: " + ex.Message);
                    }
                    finally
                    {
                        dad.Dispose();
                    }
                }
                cnn.Close();
            }

            return dt;  // Devuelve el DataTable con los resultados
        }
    }
}
