namespace UNI_AIM
{
    internal class CubeArrangementModel
    {
        private double initCubeScale = 0.96;
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabeld { get; set; } = false;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the center cube is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double CubesScale { get;  set; } = 0.96; // GlCube does not use this so you have to change it there :(

        /// <summary>
        /// The angle with which the diamond cube is rotated around the diagonal from bottom right front to top left back.
        /// </summary>
        public double CubesAngle { get;  set; } = 0;

        /// <summary>
        /// The angle with which the diamond cube is rotated around the diagonal from bottom right front to top left back.
        /// </summary>
        public double CubesAngleGlobalY { get; private set; } = 0;

        internal void AdvanceTime(double deltaTime)
        {
            // we do not advance the simulation when animation is stopped
            if (!AnimationEnabeld)
                return;

            // set a simulation time
            Time += deltaTime;

            // lets produce an oscillating scale in time
            CubesScale = 1 + 0.2 * Math.Sin(3 * Time);

            CubesAngle = Time;

            CubesAngleGlobalY = -Time;
        }

        public void ResetScale()
        {
            CubesScale = initCubeScale;
            CubesAngle = 0;
            CubesAngleGlobalY = 0;
        }
    }
}
