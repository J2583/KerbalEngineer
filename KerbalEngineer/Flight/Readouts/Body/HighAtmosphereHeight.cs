﻿// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2014 CYBUTEK
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

#region Using Directives

using KerbalEngineer.Extensions;
using KerbalEngineer.Flight.Sections;

#endregion

namespace KerbalEngineer.Flight.Readouts.Body {
    public class HighAtmosphereHeight : ReadoutModule {
        #region Constructors

        public HighAtmosphereHeight() {
            this.Name = "High Atmosphere Alt.";
            this.Category = ReadoutCategory.GetCategory("Body");
            this.HelpString = "The altitude where the upper atmosphere begins.";
            this.IsDefault = true;
        }

        #endregion

        #region Methods: public

        public override void Draw(Unity.Flight.ISectionModule section) {
            if (FlightGlobals.ActiveVessel.mainBody.atmosphere) {
                this.DrawLine(FlightGlobals.ActiveVessel.mainBody.scienceValues.flyingAltitudeThreshold.ToDistance(section.IsHud ? HudDecimalPlaces : DecimalPlaces), section);
            } else
                this.DrawLine("N/A", section);

        }

        #endregion
    }
}