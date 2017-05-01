namespace Assets.Scripts
{
    public class WellKnown
    {
        public class LayerNames
        {
            // UNITY
            public static string Default = "Default";
            public static string TransparentFx = "TransparentFX";
            public static string IgnoreRaycast = "Ignore Raycast";
            public static string Water = "Water";
            public static string UI = "UI";

            public static string Darkness = "Darkness";
            public static string DrawableDarkness = "DrawableDarkness";
            public static string Invisible = "Invisible";
        }

        public class Tags
        {
            public static string Player = "Player";
            public static string Light = "Light";
            public static string Darkness = "Darkness";
            public static string Reflector = "Reflector";
            public static string Ladder = "Ladder";
            public static string Spikes = "Spikes";
        }

        public static class Axis
        {
            public static string Horizontal = "Horizontal";
            public static string Vertical = "Vertical";
        }

        public static class Buttons
        {
            public static string Jump = "Jump";
            public static string Switch = "Switch";
            public static string Activate = "Activate";
            public static string CallCat = "CallCat";
        }

        public enum Character
        {
            None,
            Boy,
            Cat
        }
    }
}
