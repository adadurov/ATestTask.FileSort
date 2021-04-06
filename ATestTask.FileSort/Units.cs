namespace AltTestTask.FileSort
{
    public static class Units
    {
        public static long BytesPerKiB => 1024;

        public static long BytesPerMiB => BytesPerKiB * BytesPerKiB;

        public static long BytesPerGiB => BytesPerKiB * BytesPerKiB * BytesPerKiB;

        public static int MegaItems => 1000000;
    }
}
