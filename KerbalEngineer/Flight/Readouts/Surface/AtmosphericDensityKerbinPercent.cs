// 
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

    public class AtmosphericDensityKerbinPercent : ReadoutModule
    {
        public AtmosphericDensityKerbinPercent()
        {
            Name = "Atmos. Density Kerbin Percent";
            ShortName = "Density";
            Category = ReadoutCategory.GetCategory("Surface");
            HelpString = "Displays the current atmospheric density as a percentage of Kerbin's standard sea-level density. Actual sea-level density varies with temperature, so this may be higher or lower than 100% when at sea level.";
            IsDefault = false;
        }

        public override void Draw(Unity.Flight.ISectionModule section)
        {
            if (AtmosphericProcessor.ShowDetails) {
                DrawLine(AtmosphericProcessor.AirDensityKerbinPercent.ToPercent(section.IsHud ? HudDecimalPlaces : DecimalPlaces), section);
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