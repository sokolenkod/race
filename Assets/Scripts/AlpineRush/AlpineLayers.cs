namespace AlpineRush
{
    /// <summary>
    /// Layer indices configured in ProjectSettings/TagManager.
    /// Road is drivable mesh. MinimapBlip is rendered only by the minimap camera.
    /// </summary>
    public static class AlpineLayers
    {
        public const int Road = 3;
        public const int Water = 4;
        public const int Ui = 5;
        public const int Vehicle = 6;
        public const int MinimapBlip = 8;

        public static int GroundMask =>
            ~((1 << MinimapBlip) | (1 << Ui) | (1 << 2) | (1 << Water) | (1 << Vehicle));

        public static int WallMask =>
            ~((1 << Road) | (1 << Water) | (1 << MinimapBlip) | (1 << Ui) | (1 << 2) | (1 << Vehicle));
    }
}
