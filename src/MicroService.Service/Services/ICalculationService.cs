using System.Threading.Tasks;

namespace MicroService.Service.Services
{
    public interface ICalculationService
    {
        Task<double> CalculatePercentile(float[] sequence, double excelPercentile);
    }
}
