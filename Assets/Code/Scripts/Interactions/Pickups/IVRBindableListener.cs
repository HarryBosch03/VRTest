using Input;
using Player;

namespace Interactions.Pickups
{
    public interface IVRBindableListener
    {
        void Trigger(PlayerHand hand, VRBindable bindable, InputWrapper input);
    }
}