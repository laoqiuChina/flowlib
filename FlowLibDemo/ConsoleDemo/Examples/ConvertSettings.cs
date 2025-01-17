
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using FlowLib.Utils.Convert.Settings;
using FlowLib.Utils.Convert;

using System.Xml;


namespace ConsoleDemo.Examples
{
    public class ConvertSettings
    {
        public ConvertSettings()
        {
			string dir = System.AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar;
			
            // Reads DCpp xml
            BaseClient client = null;
            client = new DCDM();
            client.Read(dir + "DCDM-0.495.Favorites.xml");

            // Writes to LDC xml
            BaseClient client2 = new DCpp();
            client2.Hubs = client.Hubs;
            client2.Write(dir + "FlowLib.xml");
        }
    }
}
