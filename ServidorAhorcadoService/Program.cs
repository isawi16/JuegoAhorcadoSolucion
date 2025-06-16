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
                // Crear el binding con sesión confiable
                var binding = new WSDualHttpBinding
                {
                    MaxReceivedMessageSize = 65536,
                    OpenTimeout = TimeSpan.FromSeconds(10),
                    CloseTimeout = TimeSpan.FromSeconds(10),
                    ReceiveTimeout = TimeSpan.FromMinutes(10),
                    SendTimeout = TimeSpan.FromSeconds(10)
                };

                // Configurar sesión confiable explícitamente
                binding.ReliableSession.InactivityTimeout = TimeSpan.FromMinutes(10);

                // Agregar endpoint
              //  host.AddServiceEndpoint(typeof(IAhorcadoService), binding, "");

                // Habilitar metadata
                if (!host.Description.Behaviors.Any(b => b is ServiceMetadataBehavior))
                {
                    host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
                }

                try
                {
                    host.Open();
                    Console.WriteLine("✅ Servicio Ahorcado corriendo.");
                    Console.WriteLine("Presiona Enter para cerrar...");
                    Console.ReadLine(); // Esto mantiene la consola abierta
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error al iniciar el servicio: " + ex.Message);
                    Console.WriteLine("Presiona Enter para salir...");
                    Console.ReadLine(); // Permite ver el error antes de cerrar
                    host.Abort();
                }
            }
        }
    }
}
