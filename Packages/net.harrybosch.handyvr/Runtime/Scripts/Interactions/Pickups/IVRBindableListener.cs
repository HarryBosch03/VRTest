using HandyVR.Player;
using HandyVR.Player.Input;

namespace HandyVR.Interactions.Pickups
{
    public interface IVRBindableListener
    {
        void Trigger(PlayerHand hand, VRBindable bindable, HandInput.InputWrapper input);
    }
}