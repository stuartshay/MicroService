using System.Threading.Tasks;

namespace MicroService.Service.Services
{
    public interface ICalculationService
    {
        Task<double> CalculatePercentile(double[] sequence, double excelPercentile);
    }
}
