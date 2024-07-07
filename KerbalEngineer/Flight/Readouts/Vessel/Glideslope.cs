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

namespace KerbalEngineer.Flight.Readouts.Vessel
{
    #region Using Directives

    using Helpers;

    #endregion

    public class Glideslope : ReadoutModule
    {
        #region Constructors

        public Glideslope() {
            this.Name = "Glideslope";
            this.ShortName = "Glide";
            this.Category = ReadoutCategory.GetCategory("Vessel");
            this.HelpString = "The vertical angle between velocity and the horizon.";
            this.IsDefault = false;
        }

        #endregion

        #region Methods

        public override void Draw(Unity.Flight.ISectionModule section) {
            this.DrawLine(AttitudeProcessor.GlideslopeAngle == AttitudeProcessor.INVALID_ANGLE ? "--" : Units.ToAngle(AttitudeProcessor.GlideslopeAngle, section.IsHud ? HudDecimalPlaces : DecimalPlaces), section);
        }

        public override void Reset() {
            FlightEngineerCore.Instance.AddUpdatable(AttitudeProcessor.Instance);
        }

        public override void Update() {
            AttitudeProcessor.RequestUpdate();
        }

        #endregion
    }
}