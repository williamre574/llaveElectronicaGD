using LlaveElectronica;
using System;
using System.Data;
using System.Text;
using System.Threading;
using wsGD;



class Program
{
    private static Timer _timer;
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);
    //private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);
    static void Main(string[] args)
    {
        // Configura el temporizador para ejecutar el método cada 24 horas
        _timer = new Timer(Callback, null, TimeSpan.Zero, Intervalo);

        // Mantiene la aplicación en ejecución
        Console.WriteLine("Presione [Enter] para salir...");
        Console.ReadLine();
    }

    private static void Callback(object state)
    {
        try
        {
            MandarAlertaSlack();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al ejecutar la LLave electronica: {ex.Message}");
        }
    }

    static void MandarAlertaSlack()
    {
        string nombreAlerta = "Resoluciones GD";
        StringBuilder consulta = new StringBuilder();
        DataTable dt = new DataTable();
        Conexion conexion = new Conexion();
        ServiceSoapClient client = new ServiceSoapClient();

        consulta.Append(@"
        DECLARE @DiasHabilesParametro INT;
        SELECT @DiasHabilesParametro = Valor 
        FROM parametro 
        WHERE codigoparametro = 385;

        SELECT 
        CodigoResolucion,
        fechaVencimiento,
        dbo.Dias_habiles(GETDATE(), fechaVencimiento) AS DiasHabilesRestantes,
        CASE 
        WHEN dbo.Dias_habiles(GETDATE(), fechaVencimiento) <= @DiasHabilesParametro THEN 
            @DiasHabilesParametro - dbo.Dias_habiles(GETDATE(), fechaVencimiento)
        ELSE 
            NULL
        END AS DiasPasados
        FROM resolucion
        WHERE Estado = 1 
        AND EsElectronico = 1  
        AND fechaVencimiento > GETDATE()
        AND dbo.Dias_habiles(GETDATE(), fechaVencimiento) <= 15;
        ");

        // Convierte el StringBuilder a string antes de pasarlo
        string query = consulta.ToString();
        conexion.EjecutaQueryDT(query, ref dt);

        // Comprueba si hay resultados en la tabla
        if (dt.Rows.Count > 0)
        {
            StringBuilder mensaje = new StringBuilder();
            string color = "#18e33b";  // Color opcional

            foreach (DataRow dr in dt.Rows)
            {
                string fechaVencimiento = dr["FechaVencimiento"] != DBNull.Value ? Convert.ToDateTime(dr["FechaVencimiento"]).ToString("dd/MM/yyyy") : "N/A";
                string codigoResolucion = dr["CodigoResolucion"] != DBNull.Value ? dr["CodigoResolucion"].ToString() : "N/A";
                string diasRestantes = dr["DiasHabilesRestantes"] != DBNull.Value ? dr["DiasHabilesRestantes"].ToString() : "N/A";


                mensaje.AppendLine($"Código de Resolución: {codigoResolucion}");
                mensaje.AppendLine($"Fecha de Vencimiento: {fechaVencimiento}");
                mensaje.AppendLine($"Días Restantes: {diasRestantes}");
                mensaje.AppendLine("*******************************************━");

            }

            client.EnviarAlertaSlack(mensaje.ToString(), "Días restantes para que finalice llave electrónica", nombreAlerta, 98, "Resoluciones GD", color);
        }
    }

}

