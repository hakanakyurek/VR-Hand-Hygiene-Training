using VRTK;

namespace ManusVR.Teleportation
{
    public class VRTK_MVRPointer : VRTK_Pointer
    {
        /// <summary>
        /// Execute the teleportation procedure. Implemented this way because VRTK doesn't expose teleportation methods, because they assume a Vive controller is always used.
        /// </summary>
        public virtual void ExecuteDestinationDecided()
        {
            ExecuteSelectionButtonAction();
        }
    }
}
