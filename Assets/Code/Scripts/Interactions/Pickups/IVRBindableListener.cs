using VRTest.Input;
using VRTest.Player;

namespace VRTest.Interactions.Pickups
{
    public interface IVRBindableListener
    {
        void Trigger(PlayerHand hand, VRBindable bindable, InputWrapper input);
    }
}