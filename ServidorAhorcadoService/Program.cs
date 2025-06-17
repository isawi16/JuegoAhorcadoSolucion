using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ServidorAhorcadoService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(AhorcadoService)))
            {
                // Habilitar metadata si no está ya configurado en app.config
                if (!host.Description.Behaviors.Any(b => b is ServiceMetadataBehavior))
                {
                    host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
                }

                try
                {
                    host.Open();
                    Console.WriteLine("✅ Servicio Ahorcado corriendo.");
                    Console.WriteLine("Presiona Enter para cerrar...");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error al iniciar el servicio: " + ex.Message);
                    Console.WriteLine("Presiona Enter para salir...");
                    Console.ReadLine();
                    host.Abort();
                }
            }
        }
    }
}
