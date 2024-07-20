namespace Project.Network
{
    public static class Simulation
    {
        #region Variables

        public const uint TICKRATE = 64;
        public const uint TICKADJUSTMENTRATE = 20;
        public const uint TICKOFFSET = 18;
        public const uint INPUTAMMOUNT = 20;
        public const uint MINBUFFERSIZE = 2;
        public const uint MAXBUFFERSIZE = 14;
        public const uint MAXADJUSTMENTVALUE = 6;
        public const uint MAXRTTVALUES = 3;
        public const uint DEFAULTJITTERVALUE = 15;
        public const float MAXPOSITIONOFFSET = 0.05f;
        public const float MAXROTATIONOFFSET = 0.05f;

        #endregion
    }
}