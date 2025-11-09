using EggFramework;
using EggFramework.ObjectPool;
using EggFramework.AudioSystem;
using EggFramework.TimeSystem;
using EggFramework.Storage;
using QFramework;

namespace KOJC
{
    public sealed class KOJCApp : Architecture<KOJCApp>
    {
        protected override void Init()
        {
            RegisterSystem<IAudioSystem>(new AudioSystem());
            RegisterSystem<IObjectPoolSystem>(new ObjectPoolSystem());
            RegisterSystem<IFileSystem>(new FileSystem());
            RegisterSystem<ITimeSystem>(new TimeSystem());
            RegisterSystem<IQuickShotSystem>(new QuickShotSystem());
            RegisterUtility<IStorage>(new JsonStorage());
        }
    }
}