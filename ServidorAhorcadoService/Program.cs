using System;
using System.ServiceModel;
using BibliotecaClasesNetFramework.Contratos;

namespace ServidorAhorcadoService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (ServiceHost host = new ServiceHost(typeof(AhorcadoService)))
                {
                    host.Open();
                    Console.WriteLine("✅ Servicio Ahorcado corriendo en: ");
                    foreach (var ep in host.Description.Endpoints)
                        Console.WriteLine(" - " + ep.Address);
                    Console.WriteLine("Presiona Enter para cerrar...");
                    Console.ReadLine();
                    host.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al iniciar el servicio: " + ex.ToString());
                Console.WriteLine("Presiona Enter para salir...");
                Console.ReadLine();
            }
        }
    }
}
