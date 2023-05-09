using VRTest.Runtime.Scripts.Input;
using VRTest.Runtime.Scripts.Player;

namespace VRTest.Runtime.Scripts.Interactions.Pickups
{
    public interface IVRBindableListener
    {
        void Trigger(PlayerHand hand, VRBindable bindable, InputWrapper input);
    }
}