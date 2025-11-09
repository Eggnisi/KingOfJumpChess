using QFramework;
using UnityEngine;

namespace KOJC
{
    public class AbstractController : MonoBehaviour, IController
    {
        public IArchitecture GetArchitecture() => KOJCApp.Interface;
    }
}