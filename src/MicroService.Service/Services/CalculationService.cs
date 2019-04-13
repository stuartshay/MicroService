using System;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Data.Repository;
using MicroService.Service.Helpers;

namespace MicroService.Service.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ITestDataRepository _testDataRepository;

        public CalculationService(ITestDataRepository testDataRepository)
        {
            _testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
        }

        public async Task<double> CalculatePercentile(double[] sequence, double excelPercentile)
        {
            var data = await _testDataRepository.FindAll();
            var array = data.Select(x => x.Data).ToArray();

            return FunctionHelper.Percentile3(array, excelPercentile);
        }
    }
}
