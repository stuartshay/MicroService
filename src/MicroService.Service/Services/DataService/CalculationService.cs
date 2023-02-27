using MicroService.Data.Repository;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroService.Service.Services.DataService
{
    public class CalculationService : ICalculationService
    {
        private readonly ITestDataRepository _testDataRepository;

        public CalculationService(ITestDataRepository testDataRepository)
        {
            _testDataRepository = testDataRepository ?? throw new ArgumentNullException(nameof(testDataRepository));
        }

        public async Task<double> CalculatePercentile(double excelPercentile)
        {
            var data = await _testDataRepository.FindAll();
            var array = data.Select(x => x.Data).ToArray();

            var results = FunctionHelper.Percentile(array, excelPercentile);
            return results;
        }
    }
}
