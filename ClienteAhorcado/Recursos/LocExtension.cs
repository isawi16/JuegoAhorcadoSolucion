﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ClienteAhorcado.Recursos
{
    public class LocExtension 
    {
        public string Key { get; set; }

        public LocExtension(string key)
        {
            Key = key;
        }

        /*public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Properties.Resources.ResourceManager.GetString(Key, CultureInfo.CurrentUICulture) ?? $"!{Key}!";
        }*/
    }
}
