using System.Threading.Tasks;

namespace MicroService.Service.Interfaces
{
    public interface ICalculationService
    {
        Task<double> CalculatePercentile(double excelPercentile);
    }
}
