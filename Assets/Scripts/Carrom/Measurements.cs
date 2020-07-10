namespace Carrom
{
    public class BoardMeasurements
    {
        public const float Metre     = 1f;
        public const float CmInMetre = 100f;
        public const float Cm        = Metre / CmInMetre;

        public const float Kilogram        = 1f;
        public const float GramsInKilogram = 1000f;
        public const float Gram            = Kilogram / GramsInKilogram;

        //board parameters
        public const float PlayingSide                  = 74f    * Cm;
        public const float FrameWidth                   = 7.6f   * Cm;
        public const float PocketDiameter               = 4.45f  * Cm;
        public const float BaseLineLength               = 47f    * Cm;
        public const float BaseLineWidth                = 3.18f  * Cm;
        public const float BaseLineInnerThickness       = 0.65f  * Cm;
        public const float BaseLineInnerFromFrame       = 10.15f * Cm;
        public const float BaseCircleDiameter           = 3.18f  * Cm;
        public const float BaseCircleInnerDiameter      = 2.45f  * Cm;
        public const float ArrowLength                  = 26.7f  * Cm;
        public const float ArrowPerpendicularFromPocket = 5.0f   * Cm;
        public const float ArrowDecorativeArcDiameter   = 6.35f  * Cm;
        public const float CenterCircleInnerDiameter    = 3.18f  * Cm;
        public const float CenterCircleOuterDiameter    = 17f    * Cm;
        public const float BaseCircleNeighbourDistance  = 1.27f  * Cm;
        
        // carrommen parameters
        public const float CarrommenDiameter            = 3.18f  * Cm;
        public const float CarrommenThickness           = 0.9f   * Cm;
        public const float CarrommenWeight              = 5.5f   * Gram;
        public const float StrikerDiameter              = 4.13f  * Cm;
        public const float StrikerThickness             = 0.9f   * Cm;
        public const float StrikerWeight                = 15f    * Gram;
    }
}