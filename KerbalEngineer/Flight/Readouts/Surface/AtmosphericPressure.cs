﻿// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2015 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

namespace KerbalEngineer.Flight.Readouts.Surface
{
    using Extensions;
    using Sections;

    public class AtmosphericPressure : ReadoutModule
    {
        public AtmosphericPressure()
        {
            Name = "Atmos. Pressure";
            ShortName = "Pressure";
            Category = ReadoutCategory.GetCategory("Surface");
            HelpString = "Displays the current atmospheric pressure.";
            IsDefault = false;
        }

        public override void Draw(Unity.Flight.ISectionModule section)
        {
            if (AtmosphericProcessor.ShowDetails)
            {
                DrawLine(AtmosphericProcessor.StaticPressure.ToPressure(section.IsHud ? HudDecimalPlaces : DecimalPlaces), section);
            }
        }

        public override void Reset()
        {
            FlightEngineerCore.Instance.AddUpdatable(AtmosphericProcessor.Instance);
        }

        public override void Update()
        {
            AtmosphericProcessor.RequestUpdate();
        }
    }
}