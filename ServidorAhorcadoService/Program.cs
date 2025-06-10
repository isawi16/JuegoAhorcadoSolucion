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
            Uri baseAddress = new Uri("http://localhost:8080/AhorcadoService/");

            using (ServiceHost host = new ServiceHost(typeof(AhorcadoService), baseAddress))
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
               // binding.ReliableSession.Enabled = true;
                binding.ReliableSession.InactivityTimeout = TimeSpan.FromMinutes(10);

                // Agregar endpoint
                host.AddServiceEndpoint(typeof(IAhorcadoService), binding, "");

                // Habilitar metadata
                if (!host.Description.Behaviors.Any(b => b is ServiceMetadataBehavior))
                {
                    host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
                }

                try
                {
                    host.Open();
                    Console.WriteLine("✅ Servicio Ahorcado corriendo en: " + baseAddress);
                    Console.WriteLine("Presiona Enter para cerrar...");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error al iniciar el servicio: " + ex.Message);
                    host.Abort();
                }
            }
        }
    }
}
