using TabBlazor;

namespace StatusDashboard.Components.Shared
{
    //tabler icons (MIT), the package ships no icon set
    public static class AppIcons
    {
        private const string Blank = "<path stroke='none' d='M0 0h24v24H0z' fill='none'/>";

        private static IIconType Create(string paths) => new TablerIcon(Blank + paths);

        public static IIconType Dashboard => Create("<path d='M5 4h4a1 1 0 0 1 1 1v6a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1v-6a1 1 0 0 1 1 -1'/><path d='M5 16h4a1 1 0 0 1 1 1v2a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1v-2a1 1 0 0 1 1 -1'/><path d='M15 12h4a1 1 0 0 1 1 1v6a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1v-6a1 1 0 0 1 1 -1'/><path d='M15 4h4a1 1 0 0 1 1 1v2a1 1 0 0 1 -1 1h-4a1 1 0 0 1 -1 -1v-2a1 1 0 0 1 1 -1'/>");

        public static IIconType Server => Create("<path d='M3 4m0 3a3 3 0 0 1 3 -3h12a3 3 0 0 1 3 3v2a3 3 0 0 1 -3 3h-12a3 3 0 0 1 -3 -3z'/><path d='M3 12m0 3a3 3 0 0 1 3 -3h12a3 3 0 0 1 3 3v2a3 3 0 0 1 -3 3h-12a3 3 0 0 1 -3 -3z'/><path d='M7 8l0 .01'/><path d='M7 16l0 .01'/>");

        public static IIconType Heartbeat => Create("<path d='M3 12h4.5l1.5 -6l4 12l2 -9l1.5 3h4.5'/>");

        public static IIconType Database => Create("<path d='M12 6m-8 0a8 3 0 1 0 16 0a8 3 0 1 0 -16 0'/><path d='M4 6v6a8 3 0 0 0 16 0v-6'/><path d='M4 12v6a8 3 0 0 0 16 0v-6'/>");

        public static IIconType Exchange => Create("<path d='M7 10h14l-4 -4'/><path d='M17 14h-14l4 4'/>");

        public static IIconType Stack => Create("<path d='M12 4l-8 4l8 4l8 -4l-8 -4'/><path d='M4 12l8 4l8 -4'/><path d='M4 16l8 4l8 -4'/>");

        public static IIconType Box => Create("<path d='M12 3l8 4.5l0 9l-8 4.5l-8 -4.5l0 -9l8 -4.5'/><path d='M12 12l8 -4.5'/><path d='M12 12l0 9'/><path d='M12 12l-8 -4.5'/>");

        public static IIconType Sun => Create("<path d='M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0'/><path d='M3 12h1m8 -9v1m8 8h1m-9 8v1m-6.4 -15.4l.7 .7m12.1 -.7l-.7 .7m0 11.4l.7 .7m-12.1 -.7l-.7 .7'/>");

        public static IIconType Moon => Create("<path d='M12 3c.132 0 .263 0 .393 0a7.5 7.5 0 0 0 7.92 12.446a9 9 0 1 1 -8.313 -12.454z'/>");

        public static IIconType Refresh => Create("<path d='M20 11a8.1 8.1 0 0 0 -15.5 -2m-.5 -4v4h4'/><path d='M4 13a8.1 8.1 0 0 0 15.5 2m.5 4v-4h-4'/>");

        public static IIconType Alert => Create("<path d='M12 9v4'/><path d='M10.363 3.591l-8.106 13.534a1.914 1.914 0 0 0 1.636 2.871h16.214a1.914 1.914 0 0 0 1.636 -2.87l-8.106 -13.536a1.914 1.914 0 0 0 -3.274 0z'/><path d='M12 16h.01'/>");

        public static IIconType CircleCheck => Create("<path d='M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0'/><path d='M9 12l2 2l4 -4'/>");

        public static IIconType CircleX => Create("<path d='M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0'/><path d='M10 10l4 4m0 -4l-4 4'/>");

        public static IIconType Help => Create("<path d='M12 12m-9 0a9 9 0 1 0 18 0a9 9 0 1 0 -18 0'/><path d='M12 17l0 .01'/><path d='M12 13.5a1.5 1.5 0 0 1 1 -1.5a2.6 2.6 0 1 0 -3 -4'/>");

        public static IIconType Clock => Create("<path d='M3 12a9 9 0 1 0 18 0a9 9 0 0 0 -18 0'/><path d='M12 7v5l3 3'/>");

        public static IIconType ArrowRight => Create("<path d='M5 12l14 0'/><path d='M13 18l6 -6'/><path d='M13 6l6 6'/>");

        public static IIconType ArrowLeft => Create("<path d='M5 12l14 0'/><path d='M5 12l6 6'/><path d='M5 12l6 -6'/>");

        public static IIconType Mail => Create("<path d='M3 7a2 2 0 0 1 2 -2h14a2 2 0 0 1 2 2v10a2 2 0 0 1 -2 2h-14a2 2 0 0 1 -2 -2v-10z'/><path d='M3 7l9 6l9 -6'/>");
    }
}
