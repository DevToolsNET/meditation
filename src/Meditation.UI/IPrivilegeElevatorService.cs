namespace Meditation.UI
{
    public interface IPrivilegeElevatorService
    {
        bool IsElevated();

        void RestartAsElevated();
    }
}
