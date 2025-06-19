using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ClienteAhorcado.Utilidades
{
    public static class ValidacionesEntrada 
    {

        public static string ValidarNombre(TextBox textBox)
        {
            string nombre = textBox.Text;

            if (string.IsNullOrWhiteSpace(nombre)) {
                Animaciones.SacudirTextBox(textBox);
                return "Mensaje_Validacion_NombreVacio";
            }
            if (nombre.Length < 2) {
                Animaciones.SacudirTextBox(textBox);
                return "El nombre debe tener al menos 2 caracteres.";
            }
            return null;
        }

        public static string ValidarTelefono(TextBox textBox)
        {
            string telefono = textBox.Text;

            if (string.IsNullOrWhiteSpace(telefono)) {
                Animaciones.SacudirTextBox(textBox);
                return "El teléfono no puede estar vacío.";
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(telefono, @"^\+?\d{8,13}$")) {
                Animaciones.SacudirTextBox(textBox);
                return "El teléfono debe contener solo números y tener al menos 8 dígitos.";
            }
            return null;
        }

        public static string ValidarPassword(PasswordBox passwordBox)
        {
            string pass = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(pass)) {
                Animaciones.SacudirPasswordBox(passwordBox);
                return "La contraseña no puede estar vacía.";
            }
            if (pass.Length < 5) {
                Animaciones.SacudirPasswordBox(passwordBox);
                return "La contraseña debe tener al menos 5 caracteres.";
            }
            return null;
        }

        public static string ValidarPasswordTextBox(TextBox textBox)
        {
            string pass = textBox.Text;

            if (string.IsNullOrWhiteSpace(pass))
            {
                Animaciones.SacudirTextBox(textBox);
                return "La contraseña no puede estar vacía.";
            }
            if (pass.Length < 5)
            {
                Animaciones.SacudirTextBox(textBox);
                return "La contraseña debe tener al menos 5 caracteres.";
            }
            return null;
        }

        public static string ValidarFechaNacimiento(DatePicker datePicker)
        {
            DateTime? fecha = datePicker.SelectedDate;

            if (fecha == null) {
                Animaciones.SacudirDatePicker(datePicker);
                return "Debes seleccionar una fecha de nacimiento.";
            }
            if (fecha.Value > DateTime.Now) {
                Animaciones.SacudirDatePicker(datePicker);
                return "La fecha de nacimiento no puede ser en el futuro.";
            }
            if (fecha.Value.Year < 1900) {
                Animaciones.SacudirDatePicker(datePicker);
                return "La fecha de nacimiento no es válida.";
            }
            return null;
        }

        public static string ValidarCorreo(TextBox textBox)
        {
            string correo = textBox.Text;

            if (string.IsNullOrWhiteSpace(correo))
            {
                Animaciones.SacudirTextBox(textBox);
                return "Mensaje_Validacion_CorreoVacio";
            }

            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(correo, patron))
            {
                Animaciones.SacudirTextBox(textBox);
                return "El correo debe contener '@' y el dominio";
            }
            return null;
        }

        public static void ValidarEntrada(TextBox textBox, string patron, int longitudMaxima)
        {
            textBox.TextChanged += (s, e) =>
            {
                string entrada = textBox.Text;
                string limpiado = Regex.Replace(entrada, patron, "");

                if (limpiado.Length > longitudMaxima)
                    limpiado = limpiado.Substring(0, longitudMaxima);

                if (entrada != limpiado)
                {
                    textBox.Text = limpiado;
                    textBox.SelectionStart = limpiado.Length;
                    Animaciones.SacudirTextBox(textBox);
                }
            };
        }
        public static void ValidarEntradaContrasena(PasswordBox passwordBox, string patron, int longitudMaxima)
        {
            passwordBox.PasswordChanged += (s, e) =>
            {
                string entrada = passwordBox.Password;
                string limpiado = Regex.Replace(entrada, patron, "");

                if (limpiado.Length > longitudMaxima)
                    limpiado = limpiado.Substring(0, longitudMaxima);

                if (entrada != limpiado)
                {
                    passwordBox.Password = limpiado;
                    Animaciones.SacudirPasswordBox(passwordBox);
                }
            };
        }



    }
}
