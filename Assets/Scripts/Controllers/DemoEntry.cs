using UnityEngine;
using EggFramework.Modules.Launch;
using QFramework;

namespace KOJC
{
    public sealed class DemoEntry : AbstractController
    {
        private void Awake()
        {
            var lfsm = new LaunchFSM(KOJCApp.Interface);
            lfsm.OnLaunchComplete(() => this.SendEvent(new ArchitectureInitFinishEvent()));
            lfsm.Start();
        }
    }
}