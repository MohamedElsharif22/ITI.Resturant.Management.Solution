using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IOrderProgressionService
    {
        Task QueueOrderProgressionAsync(int orderId);
    }
}