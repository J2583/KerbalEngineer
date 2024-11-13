// 
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
    using KerbalEngineer.Helpers;
    #region Using Directives

    using System;
    using UnityEngine;

    #endregion

    public class AttitudeProcessor : IUpdatable, IUpdateRequest
    {
        public const double INVALID_ANGLE = -9000000.0;

        #region Fields

        private static readonly AttitudeProcessor instance = new AttitudeProcessor();

        private double heading;
        private double headingRate;
        private double pitch;
        private double pitchRate;
        private double previousHeading;
        private double previousPitch;
        private double previousRoll;
        private double roll;
        private double rollRate;
        private Quaternion surfaceRotation;

        #endregion

        #region Properties
        
        public static double Course { get; private set; }

        public static double Heading
        {
            get { return instance.heading; }
        }

        public static double HeadingRate
        {
            get { return instance.headingRate; }
        }

        public static AttitudeProcessor Instance
        {
            get { return instance; }
        }

        public static double Pitch
        {
            get { return instance.pitch; }
        }

        public static double PitchRate
        {
            get { return instance.pitchRate; }
        }

        public static double Roll
        {
            get { return instance.roll; }
        }

        public static double RollRate
        {
            get { return instance.rollRate; }
        }
        
        public static double GlideslopeAngle { get; private set; }

        public static double DisplacementAngle { get; private set; }
        public static double AttackAngle { get; private set; }
        public static double SideslipAngle { get; private set; }

        public bool UpdateRequested { get; set; }

        #endregion

        #region Methods

        public static void RequestUpdate()
        {
            instance.UpdateRequested = true;
        }

        public void Update()
        {
            var vessel = FlightGlobals.ActiveVessel;

            this.surfaceRotation = this.GetSurfaceRotation(vessel);

            this.previousHeading = this.heading;
            this.previousPitch = this.pitch;
            this.previousRoll = this.roll;

            // This code was derived from MechJeb2's implementation for getting the vessel's surface relative rotation.
            var surfRotEuler = this.surfaceRotation.eulerAngles;
            this.heading = surfRotEuler.y;
            this.pitch = surfRotEuler.x > 180.0f
                ? 360.0f - surfRotEuler.x
                : -surfRotEuler.x;
            this.roll = surfRotEuler.z > 180.0f
                ? 360.0f - surfRotEuler.z
                : -surfRotEuler.z;

            this.headingRate = (this.heading - this.previousHeading) / TimeWarp.fixedDeltaTime;
            this.pitchRate   = (this.pitch - this.previousPitch) / TimeWarp.fixedDeltaTime;
            this.rollRate    = (this.roll - this.previousRoll) / TimeWarp.fixedDeltaTime;


            //Also stolen from MechJeb2
            
            var surfaceVelocity = vessel.obt_velocity - vessel.mainBody.getRFrmVel(vessel.CoMD);
            var surfaceSpeed = surfaceVelocity.magnitude;
            
            if (surfaceSpeed < 0.05) Course = GlideslopeAngle = DisplacementAngle = AttackAngle = SideslipAngle = INVALID_ANGLE;
            else {
                Vector3d normSurfVel = surfaceVelocity.normalized;
                var up = (vessel.CoMD - vessel.mainBody.position).normalized;
                var north = GetNorth(vessel.CoMD, up, vessel.mainBody);
                var eastMaybe = Vector3.Cross(up, north);

                var srfProjNorm = Vector3.ProjectOnPlane(normSurfVel, up).normalized;
                double tempCourse = UtilMath.Rad2Deg * Math.Atan2(Clamp(Vector3.Dot(eastMaybe, srfProjNorm), -1.0, 1.0),
                                                                  Clamp(Vector3.Dot(north, srfProjNorm), -1.0, 1.0));
                if (tempCourse < 0) tempCourse += 360.0;
                Course = double.IsNaN(tempCourse) ? INVALID_ANGLE : tempCourse;

                //Vertical velocity angle relative to the planet's surface
                double tempGS = UtilMath.Rad2Deg *
                                Math.Asin(Mathf.Clamp(Vector3.Dot(up, normSurfVel), -1, 1));
                GlideslopeAngle = double.IsNaN(tempGS) ? INVALID_ANGLE : tempGS;

                // Displacement Angle, angle between surface velocity and the ship-nose vector (KSP "up" vector) -- ignores roll of the craft (0 to 180 degrees)
                double tempAoD = UtilMath.Rad2Deg *
                                 Math.Acos(Mathf.Clamp(Vector3.Dot(vessel.ReferenceTransform.up, normSurfVel), -1, 1));
                DisplacementAngle = double.IsNaN(tempAoD) ? INVALID_ANGLE : tempAoD;

                // Angle of Attack, angle between surface velocity and the ship-nose vector (KSP "up" vector) in the plane that has no ship-right/left in it (-180 to +180 degrees)
                srfProjNorm = Vector3.ProjectOnPlane(normSurfVel, vessel.ReferenceTransform.right).normalized;
                double tempAoA = UtilMath.Rad2Deg * Math.Atan2(Vector3.Dot(srfProjNorm, vessel.ReferenceTransform.forward),
                                                               Vector3.Dot(srfProjNorm, vessel.ReferenceTransform.up));
                AttackAngle = double.IsNaN(tempAoA) ? INVALID_ANGLE : tempAoA;

                // Angle of Sideslip, angle between surface velocity and the ship-nose vector (KSP "up" vector) in the plane that has no ship-top/bottom in it (KSP "forward"/"back"; -180 to +180 degrees)
                srfProjNorm = Vector3.ProjectOnPlane(normSurfVel, vessel.ReferenceTransform.forward).normalized;
                double tempAoS = UtilMath.Rad2Deg * Math.Atan2(Vector3.Dot(srfProjNorm, vessel.ReferenceTransform.right),
                                                               Vector3.Dot(srfProjNorm, vessel.ReferenceTransform.up));
                SideslipAngle = double.IsNaN(tempAoA) ? INVALID_ANGLE : tempAoS;
            }
        }

        private Quaternion GetSurfaceRotation(global::Vessel vessel) { return GetSurfaceRotation(vessel.CoMD, vessel.transform.rotation, vessel.mainBody); }

        private Quaternion GetSurfaceRotation(Vector3d position, Quaternion rotation, CelestialBody mainBody)
        {
            // This code and GetNorth were derived from MechJeb2's implementation for getting the vessel's surface relative rotation.
            var up = (position - mainBody.position).normalized;
            return Quaternion.Inverse(Quaternion.Euler(90.0f, 0.0f, 0.0f) * Quaternion.Inverse(rotation) * Quaternion.LookRotation(GetNorth(position, up, mainBody), up));
        }

        private Vector3d GetNorth(Vector3 position, Vector3d up, CelestialBody mainBody) {
            return Vector3.ProjectOnPlane((mainBody.position + mainBody.transform.up * (float)mainBody.Radius) - position, up).normalized;
        }

        //Not sure why this is in Mathf but not Math...
        public static double Clamp(double value, double min, double max) {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        #endregion
    }
}