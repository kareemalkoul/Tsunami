namespace Kareem.Fluid.SPH
{
    public enum NumParticleEnum
    {
        NUM_1K = 1024,
        NUM_2K = 1024 * 2,
        NUM_4K = 1024 * 4,
        NUM_8K = 1024 * 8,
        NUM_16K = 1024 * 16,
        NUM_32K = 1024 * 32,
        NUM_64K = 1024 * 64,
        NUM_128K = 1024 * 128,
        NUM_256K = 1024 * 256,
        NUM_512K = 1024 * 512

    };

    public enum InitParticleWay
    {
        SPHERE = 0,
        CUBE = 1,
    }

    public static class InitParticleWayExtensions
    {
        public static string ToFriendlyString(this InitParticleWay me)
        {
            switch (me)
            {
                case InitParticleWay.SPHERE:
                    return "SPHERE";
                case InitParticleWay.CUBE:
                    return "CUBE";
                default:
                    return "nan";
            }
        }
    }
}
