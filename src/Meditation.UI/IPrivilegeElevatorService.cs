using System.Threading.Tasks;

namespace Meditation.UI
{
    public interface IPrivilegeElevatorService
    {
        bool IsElevated();

        Task RestartAsElevated();
    }
}
